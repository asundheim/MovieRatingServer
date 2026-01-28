using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieRatingShared;

public class TMDBMovieDetailResponse
{
    [JsonPropertyName("adult")]
    public required bool Adult { get; set; }

    [JsonPropertyName("backdrop_path")]
    public required string BackdropPath { get; set; }

    //[JsonPropertyName("belongs_to_collection")]
    //public required List<BelongsToCollection> BelongsToCollection { get; set; }

    [JsonPropertyName("budget")]
    public required int Budget { get; set; }

    [JsonPropertyName("genres")]
    public required List<GenreResponse> Genres { get; set; }

    [JsonPropertyName("homepage")]
    public required string Homepage { get; set; }

    [JsonPropertyName("id")]
    public required int TmdbId { get; set; }

    [JsonPropertyName("imdb_id")]
    public required string ImdbId { get; set; }

    [JsonPropertyName("original_language")]
    public required string OriginalLanguage { get; set; }

    [JsonPropertyName("original_title")]
    public required string OriginalTitle { get; set; }

    [JsonPropertyName("overview")]
    public required string Overview { get; set; }

    [JsonPropertyName("popularity")]
    public required double Popularity { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }

    [JsonPropertyName("production_companies")]
    public required List<ProductionCompaniesResponse> ProductionCompanies { get; set; }

    [JsonPropertyName("production_countries")]
    public required List<ProductionCountriesResponse> ProductionCountries { get; set; }

    [JsonPropertyName("release_date")]
    public required string ReleaseDate { get; set; }

    [JsonPropertyName("revenue")]
    public required int Revenue { get; set; }

    [JsonPropertyName("runtime")]
    public required int Runtime { get; set; }

    [JsonPropertyName("spoken_languages")]
    public required List<SpokenLanguagesResponse> SpokenLanguages { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("tagline")]
    public required string Tagline { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("video")]
    public required bool IsVideo { get; set; }

    [JsonPropertyName("vote_average")]
    public required double VoteAvg { get; set; }

    [JsonPropertyName("vote_count")]
    public required int VoteCount { get; set; }
}

public class BelongsToCollection
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public required int BackdropPath { get; set; }
}

public class SpokenLanguagesResponse
{
    [JsonPropertyName("english_name")]
    public required string EnglishName { get; set; }

    [JsonPropertyName("iso_639_1")]
    public required string IsoString { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class ProductionCountriesResponse
{
    [JsonPropertyName("iso_3166_1")]
    public required string IsoString { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class ProductionCompaniesResponse
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("logo_path")]
    public required string LogoPath { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("origin_country")]
    public required string OriginCountry { get; set; }
}

public class GenreResponse
{
    [JsonPropertyName("id")]
    public int GenreId { get; set; }

    [JsonPropertyName("name")]
    public required string GenreName { get; set; }
}


public class TMDBExternalDetailResponse
{
    [JsonPropertyName("movie_results")]
    public required List<TMDBExternalDetailData> TMDBData {  get; set; }


}

public class TMDBExternalDetailData
{
    [JsonPropertyName("adult")]
    public required bool Adult { get; set; }

    [JsonPropertyName("backdrop_path")]
    public required string BackdropPath { get; set; }

    [JsonPropertyName("id")]
    public required int TMDBID { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("original_title")]
    public required string OriginalTitle { get; set; }

    [JsonPropertyName("overview")]
    public required string Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }

    [JsonPropertyName("media_type")]
    public required string MediaType { get; set; }

    [JsonPropertyName("original_language")]
    public required string OriginalLanguage { get; set; }

    [JsonPropertyName("genre_ids")]
    public required List<int> GenreIDs { get; set; }

    [JsonPropertyName("popularity")]
    public required double Popularity { get; set; }

    [JsonPropertyName("release_date")]
    public required string ReleaseDate { get; set; }

    [JsonPropertyName("video")]
    public required bool IsVideo { get; set; }

    [JsonPropertyName("vote_average")]
    public required double VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public required int VoteCount { get; set; }
}


public class TmdbMovieReviewsResponse
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("page")]
    public required int Page { get; set; }

    [JsonPropertyName("results")]
    public required List<ReviewsResults> Results { get; set; }

    [JsonPropertyName("total_pages")]
    public required int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public required int TotalResults { get; set; }
}

public class ReviewsResults
{
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    //[JsonPropertyName("author_details")]
    //public List<AuthorDetails>? AuthorDetails { get; set; }

    [JsonPropertyName("content")]
    public string? ReviewContent { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? ReviewDate { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? ReviewUpdatedDate { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class AuthorDetails
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("avatar_path")]
    public string? AvatarPath { get; set; }

    [JsonPropertyName("rating")]
    public string? Rating { get; set; }
}