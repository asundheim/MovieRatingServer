using System.Text.Json.Serialization;

namespace MovieRatingShared;

public class NewMovieDatabase
{
    [JsonPropertyName("movie-database")]
    public required List<NewMovie> NewMovies { get; set; }
}

public class NewMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; } // TMDB ID

    [JsonPropertyName("title")]
    public required string Title { get; set; }
}