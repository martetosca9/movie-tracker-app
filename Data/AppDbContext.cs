using Microsoft.EntityFrameworkCore;
using movie_tracker_app.Models;

namespace movie_tracker_app.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.TmdbId)
            .IsUnique()
            .HasFilter("TmdbId IS NOT NULL");

        // Seed initial movies
        modelBuilder.Entity<Movie>().HasData(
            new Movie
            {
                Id = 1,
                TmdbId = 27205,
                Title = "Inception",
                Overview = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                Director = "Christopher Nolan",
                Genre = "Sci-Fi",
                ReleaseYear = 2010,
                Rating = 9,
                Status = WatchStatus.Completed,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Movie
            {
                Id = 2,
                TmdbId = 693134,
                Title = "Dune: Part Two",
                Overview = "Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.",
                Director = "Denis Villeneuve",
                Genre = "Sci-Fi",
                ReleaseYear = 2024,
                Rating = 9,
                Status = WatchStatus.Completed,
                CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new Movie
            {
                Id = 3,
                TmdbId = 155,
                Title = "The Dark Knight",
                Overview = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                Director = "Christopher Nolan",
                Genre = "Action",
                ReleaseYear = 2008,
                Rating = 10,
                Status = WatchStatus.Completed,
                CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new Movie
            {
                Id = 4,
                TmdbId = 157336,
                Title = "Interstellar",
                Overview = "When Earth becomes uninhabitable in the future, a farmer and ex-NASA pilot, Joseph Cooper, is tasked to pilot a spacecraft, along with a team of researchers, to find a new planet for humans.",
                Director = "Christopher Nolan",
                Genre = "Sci-Fi",
                ReleaseYear = 2014,
                Rating = 9,
                Status = WatchStatus.PlanToWatch,
                CreatedAt = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
