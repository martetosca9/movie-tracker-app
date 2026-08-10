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
