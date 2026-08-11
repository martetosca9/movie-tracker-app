using System.Text.Json.Serialization;

namespace movie_tracker_app.Services;

public class TmdbMovieDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    [JsonPropertyName("genre_ids")]
    public List<int>? GenreIds { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenreDto>? Genres { get; set; }

    [JsonPropertyName("credits")]
    public TmdbCreditsDto? Credits { get; set; }

    public string FullPosterUrl => !string.IsNullOrEmpty(PosterPath)
        ? $"https://image.tmdb.org/t/p/w500{PosterPath}"
        : "https://via.placeholder.com/500x750?text=No+Poster";

    public int? ReleaseYear
    {
        get
        {
            if (DateTime.TryParse(ReleaseDate, out var date))
            {
                return date.Year;
            }
            return null;
        }
    }

    public string? GenreNames => Genres is { Count: > 0 }
        ? string.Join(", ", Genres.Select(g => g.Name))
        : null;

    public string? DirectorName => Credits?.Crew?
        .FirstOrDefault(c => c.Job.Equals("Director", StringComparison.OrdinalIgnoreCase))
        ?.Name;
}

public class TmdbGenreDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbCreditsDto
{
    [JsonPropertyName("crew")]
    public List<TmdbCrewMemberDto> Crew { get; set; } = new();
}

public class TmdbCrewMemberDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;
}

public class TmdbSearchResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<TmdbMovieDto> Results { get; set; } = new();
}

public interface ITmdbService
{
    Task<List<TmdbMovieDto>> SearchMoviesAsync(string query);
    Task<List<TmdbMovieDto>> GetPopularMoviesAsync();
    Task<TmdbMovieDto?> GetMovieDetailsAsync(int tmdbId);
}

public class TmdbService : ITmdbService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TmdbService> _logger;

    public TmdbService(HttpClient httpClient, IConfiguration configuration, ILogger<TmdbService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string GetApiKey()
    {
        var configuredKey = _configuration["Tmdb:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            return configuredKey;
        }

        return Environment.GetEnvironmentVariable("TMDB_API_KEY") ?? string.Empty;
    }

    public async Task<List<TmdbMovieDto>> SearchMoviesAsync(string query)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("TMDB API Key missing. Returning demo/empty results.");
            return GetDemoMovies().Where(m => m.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        try
        {
            var url = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={Uri.EscapeDataString(query)}&language=es-ES";
            var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url);
            return response?.Results ?? new List<TmdbMovieDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching TMDB movies");
            return new List<TmdbMovieDto>();
        }
    }

    public async Task<List<TmdbMovieDto>> GetPopularMoviesAsync()
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return GetDemoMovies();
        }

        try
        {
            var url = $"https://api.themoviedb.org/3/movie/popular?api_key={apiKey}&language=es-ES";
            var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url);
            return response?.Results ?? new List<TmdbMovieDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular movies from TMDB");
            return GetDemoMovies();
        }
    }

    public async Task<TmdbMovieDto?> GetMovieDetailsAsync(int tmdbId)
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return GetDemoMovies().FirstOrDefault(m => m.Id == tmdbId);
        }

        try
        {
            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={apiKey}&language=es-ES&append_to_response=credits";
            return await _httpClient.GetFromJsonAsync<TmdbMovieDto>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching TMDB movie details for id {TmdbId}", tmdbId);
            return null;
        }
    }

    private static List<TmdbMovieDto> GetDemoMovies()
    {
        return new List<TmdbMovieDto>
        {
            new()
            {
                Id = 550,
                Title = "Fight Club",
                Overview = "Un oficinista insomne y un fabricante de jabón desocupado forman un club de lucha subterráneo.",
                ReleaseDate = "1999-10-15",
                VoteAverage = 8.4,
                PosterPath = "/pB8O2CYJjyqMjYGsB4vR7WvRofw.jpg",
                Genres = [new TmdbGenreDto { Id = 18, Name = "Drama" }],
                Credits = new TmdbCreditsDto
                {
                    Crew = [new TmdbCrewMemberDto { Name = "David Fincher", Job = "Director" }]
                }
            },
            new()
            {
                Id = 157336,
                Title = "Interstellar",
                Overview = "Un grupo de exploradores viaja a través de un agujero de gusano en el espacio en un intento por asegurar la supervivencia de la humanidad.",
                ReleaseDate = "2014-11-05",
                VoteAverage = 8.4,
                PosterPath = "/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                Genres = [new TmdbGenreDto { Id = 878, Name = "Ciencia ficción" }, new TmdbGenreDto { Id = 18, Name = "Drama" }],
                Credits = new TmdbCreditsDto
                {
                    Crew = [new TmdbCrewMemberDto { Name = "Christopher Nolan", Job = "Director" }]
                }
            },
            new()
            {
                Id = 27205,
                Title = "Inception",
                Overview = "Dom Cobb es un ladrón capaz de adentrarse en los sueños para robar secretos del subconsciente.",
                ReleaseDate = "2010-07-15",
                VoteAverage = 8.4,
                PosterPath = "/oYuLEydvwzK8oA9AJeYyARxWvXt.jpg",
                Genres = [new TmdbGenreDto { Id = 28, Name = "Acción" }, new TmdbGenreDto { Id = 878, Name = "Ciencia ficción" }],
                Credits = new TmdbCreditsDto
                {
                    Crew = [new TmdbCrewMemberDto { Name = "Christopher Nolan", Job = "Director" }]
                }
            },
            new()
            {
                Id = 693134,
                Title = "Dune: Part Two",
                Overview = "Paul Atreides se une a Chani y a los Fremen mientras busca venganza contra los conspiradores que destruyeron a su familia.",
                ReleaseDate = "2024-02-27",
                VoteAverage = 8.3,
                PosterPath = "/czembW0Rk1Ke7jA4Y2fOHmTeEav.jpg",
                Genres = [new TmdbGenreDto { Id = 878, Name = "Ciencia ficción" }, new TmdbGenreDto { Id = 12, Name = "Aventura" }],
                Credits = new TmdbCreditsDto
                {
                    Crew = [new TmdbCrewMemberDto { Name = "Denis Villeneuve", Job = "Director" }]
                }
            }
        };
    }
}
