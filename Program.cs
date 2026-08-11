using Microsoft.EntityFrameworkCore;
using movie_tracker_app.Data;
using movie_tracker_app.Endpoints;
using movie_tracker_app.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ITmdbService, TmdbService>();

// Register AppDbContext with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=movies.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Auto-create database & seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // EnsureCreated does not alter existing DBs; add TmdbId if missing.
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Movies ADD COLUMN TmdbId INTEGER NULL");
    }
    catch
    {
        // Column already exists
    }

    // Backfill one known seed row per title. Older DBs may already contain duplicate titles.
    db.Database.ExecuteSqlRaw("UPDATE Movies SET TmdbId = 27205 WHERE Id = (SELECT Id FROM Movies WHERE Title = 'Inception' AND TmdbId IS NULL AND NOT EXISTS (SELECT 1 FROM Movies WHERE TmdbId = 27205) ORDER BY Id LIMIT 1)");
    db.Database.ExecuteSqlRaw("UPDATE Movies SET TmdbId = 693134 WHERE Id = (SELECT Id FROM Movies WHERE Title = 'Dune: Part Two' AND TmdbId IS NULL AND NOT EXISTS (SELECT 1 FROM Movies WHERE TmdbId = 693134) ORDER BY Id LIMIT 1)");
    db.Database.ExecuteSqlRaw("UPDATE Movies SET TmdbId = 155 WHERE Id = (SELECT Id FROM Movies WHERE Title = 'The Dark Knight' AND TmdbId IS NULL AND NOT EXISTS (SELECT 1 FROM Movies WHERE TmdbId = 155) ORDER BY Id LIMIT 1)");
    db.Database.ExecuteSqlRaw("UPDATE Movies SET TmdbId = 157336 WHERE Id = (SELECT Id FROM Movies WHERE Title = 'Interstellar' AND TmdbId IS NULL AND NOT EXISTS (SELECT 1 FROM Movies WHERE TmdbId = 157336) ORDER BY Id LIMIT 1)");

    db.Database.ExecuteSqlRaw("UPDATE Movies SET TmdbId = NULL WHERE TmdbId IS NOT NULL AND Id NOT IN (SELECT MIN(Id) FROM Movies WHERE TmdbId IS NOT NULL GROUP BY TmdbId)");

    db.Database.ExecuteSqlRaw(
        "CREATE UNIQUE INDEX IF NOT EXISTS IX_Movies_TmdbId ON Movies(TmdbId) WHERE TmdbId IS NOT NULL");
}

// Enable serving wwwroot/index.html on http://localhost:5255
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure OpenAPI in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map Movie API Endpoints
app.MapMovieEndpoints();

app.Run();
