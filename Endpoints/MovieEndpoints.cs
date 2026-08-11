using Microsoft.EntityFrameworkCore;
using movie_tracker_app.Data;
using movie_tracker_app.Models;
using movie_tracker_app.Services;

namespace movie_tracker_app.Endpoints;

public record ImportMovieRequest(
    int TmdbId,
    int? Rating,
    WatchStatus Status
);

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/movies").WithTags("Movies");

        // GET /api/movies - List movies in SQLite DB
        group.MapGet("/", async (AppDbContext db, string? genre, WatchStatus? status, string? search) =>
        {
            var query = db.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(genre.ToLower()));
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.ToLower().Contains(search.ToLower()) || 
                                         (m.Director != null && m.Director.ToLower().Contains(search.ToLower())));
            }

            var movies = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            return Results.Ok(movies);
        })
        .WithName("GetMovies");

        // GET /api/movies/external/search - Search TMDB
        group.MapGet("/external/search", async (string query, ITmdbService tmdbService) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest(new { message = "Query string cannot be empty." });
            }

            var results = await tmdbService.SearchMoviesAsync(query);
            return Results.Ok(results);
        })
        .WithName("SearchExternalMovies");

        // GET /api/movies/external/popular - Get Popular TMDB Movies
        group.MapGet("/external/popular", async (ITmdbService tmdbService) =>
        {
            var results = await tmdbService.GetPopularMoviesAsync();
            return Results.Ok(results);
        })
        .WithName("GetPopularExternalMovies");

        // POST /api/movies/import - Import TMDB movie into local SQLite database
        group.MapPost("/import", async (ImportMovieRequest req, AppDbContext db, ITmdbService tmdbService) =>
        {
            var existing = await db.Movies.FirstOrDefaultAsync(m => m.TmdbId == req.TmdbId);
            if (existing is not null)
            {
                return Results.Conflict(new
                {
                    message = $"\"{existing.Title}\" ya está en tu lista.",
                    movie = existing
                });
            }

            var details = await tmdbService.GetMovieDetailsAsync(req.TmdbId);
            if (details is null)
            {
                return Results.NotFound(new { message = $"No se encontró la película de TMDB con id {req.TmdbId}." });
            }

            var movie = new Movie
            {
                TmdbId = details.Id,
                Title = details.Title,
                Overview = details.Overview,
                Director = details.DirectorName,
                Genre = details.GenreNames,
                ReleaseYear = details.ReleaseYear,
                Rating = req.Rating,
                Status = req.Status,
                PosterUrl = details.FullPosterUrl,
                CreatedAt = DateTime.UtcNow
            };

            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            return Results.Created($"/api/movies/{movie.Id}", movie);
        })
        .WithName("ImportMovie");

        // GET /api/movies/{id} - Get movie by Id
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            return movie is not null ? Results.Ok(movie) : Results.NotFound(new { message = $"Movie with ID {id} not found." });
        })
        .WithName("GetMovieById");

        // POST /api/movies - Create new manual movie
        group.MapPost("/", async (Movie movie, AppDbContext db) =>
        {
            movie.CreatedAt = DateTime.UtcNow;
            movie.TmdbId = null;
            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            return Results.Created($"/api/movies/{movie.Id}", movie);
        })
        .WithName("CreateMovie");

        // PUT /api/movies/{id} - Update existing movie
        group.MapPut("/{id:int}", async (int id, Movie updatedMovie, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null) return Results.NotFound(new { message = $"Movie with ID {id} not found." });

            movie.Title = updatedMovie.Title;
            movie.Overview = updatedMovie.Overview;
            movie.Director = updatedMovie.Director;
            movie.Genre = updatedMovie.Genre;
            movie.ReleaseYear = updatedMovie.ReleaseYear;
            movie.Rating = updatedMovie.Rating;
            movie.Status = updatedMovie.Status;
            movie.PosterUrl = updatedMovie.PosterUrl;
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(movie);
        })
        .WithName("UpdateMovie");

        // DELETE /api/movies/{id} - Delete movie
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null) return Results.NotFound(new { message = $"Movie with ID {id} not found." });

            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteMovie");
    }
}
