using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieRatingShared;

public class WatchProviderResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("results")]
    public required WatchProviderResults Results { get; set; }
}

public class WatchProviderResults
{
    [JsonPropertyName("US")]
    public WatchProviderCountry? US { get; set; }
}

public class WatchProviderCountry
{
    [JsonPropertyName("link")]
    public required string Link { get; set; }

    [JsonPropertyName("flatrate")]
    public List<WatchProvider>? Flatrate { get; set; }

    [JsonPropertyName("buy")]
    public List<WatchProvider>? Buy { get; set; }

    [JsonPropertyName("rent")]
    public List<WatchProvider>? Rent { get; set; }
}

public class WatchProvider
{
    [JsonPropertyName("logo_path")]
    public required string LogoPath { get; set; }

    [JsonPropertyName("provider_id")]
    public required int ProviderId { get; set; }

    [JsonPropertyName("provider_name")]
    public required string ProviderName { get; set; }

    [JsonPropertyName("display_priority")]
    public required int DisplayPriority { get; set; }
}