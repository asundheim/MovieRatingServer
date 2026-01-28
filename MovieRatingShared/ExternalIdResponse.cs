using System.Text.Json.Serialization;

namespace MovieRatingShared;

public class ExternalIdResponse
{
    [JsonPropertyName("imdb_id")]
    public required string IMDbId { get; set; }
}
