using MovieRatingShared;
using System.Text.Json.Serialization;

namespace MovieRating.Shared;

public class MovieInfo
{
    [JsonPropertyName("Title")]
    public required string Title { get; set; }

    [JsonPropertyName("Director")]
    public required string Director { get; set; }

    [JsonPropertyName("Year")]
    public required string Year { get; set; }

    [JsonPropertyName("Actors")]
    public required string Actors { get; set; }

    [JsonPropertyName("Plot")]
    public required string Plot { get; set; }

    [JsonPropertyName("Poster")]
    public required string Poster { get; set; }

    [JsonPropertyName("RatingInfo")]
    public required RatingInfo RatingInfo { get; set; }

    [JsonPropertyName("BoxOffice")]
    public required string BoxOffice { get; set; }

    [JsonPropertyName("WatchProviders")]
    public required List<WatchProvider>? WatchProviders { get; set; }

    [JsonPropertyName("Reviews")]
    public required List<string>? Reviews { get; set; }
}

public class RatingInfo
{
    [JsonPropertyName("RatingIndex")]
    public required RatingIndex RatingIndex { get; set; }

    [JsonPropertyName("RatingValue")]
    public required string RatingValue { get; set; }
}

public class RawMovieList
{
    [JsonPropertyName("MovieDatabase")]
    public List<RawMovie> MovieDatabase { get; set; } = new();
}

public class RawMovie
{
    public required string Title { get; set; }
    public string Year { get; set; } = string.Empty;
    public required string Rated { get; set; }
    public required string Released { get; set; }
    public required string Runtime { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Writer { get; set; }
    public required string Actors { get; set; }
    public required string Plot { get; set; }
    public required string Language { get; set; }
    public required string Country { get; set; }
    public required string Awards { get; set; }
    public required string Poster { get; set; }
    public required List<RawRating> Ratings { get; set; }
    public required string Metascore { get; set; }
    public required string imdbRating { get; set; }
    public required string imdbVotes { get; set; }
    public required string imdbID { get; set; }
    public required string Type { get; set; }
    public required string DVD { get; set; }
    public required string BoxOffice { get; set; }
    public required string Production { get; set; }
    public required string Website { get; set; }
    public required string Response { get; set; }
    public int TMDBId { get; set; }
    public List<WatchProvider>? WatchProviders { get; set; }
    public List<string>? Reviews { get; set; }
}

public class RawRating
{
    public required string Source { get; set; }
    public required string Value { get; set; }
}