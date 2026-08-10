using Microsoft.EntityFrameworkCore;
using movie_tracker_app.Data;
using movie_tracker_app.Models;

namespace movie_tracker_app.Endpoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/movies").WithTags("Movies");

        // GET /api/movies - List all movies (optional filtering by genre or status)
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

        // GET /api/movies/{id} - Get movie by Id
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            return movie is not null ? Results.Ok(movie) : Results.NotFound(new { message = $"Movie with ID {id} not found." });
        })
        .WithName("GetMovieById");

        // POST /api/movies - Create new movie
        group.MapPost("/", async (Movie movie, AppDbContext db) =>
        {
            movie.CreatedAt = DateTime.UtcNow;
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
