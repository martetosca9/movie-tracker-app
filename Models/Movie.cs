using System.ComponentModel.DataAnnotations;

namespace movie_tracker_app.Models;

public class Movie
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Overview { get; set; }

    [MaxLength(100)]
    public string? Director { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int? ReleaseYear { get; set; }

    /// <summary>
    /// Rating score from 1 to 10
    /// </summary>
    [Range(1, 10)]
    public int? Rating { get; set; }

    public WatchStatus Status { get; set; } = WatchStatus.PlanToWatch;

    [MaxLength(500)]
    public string? PosterUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
