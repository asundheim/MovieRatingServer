
using GatherMovieInfo;
using MovieRating.Shared;
using MovieRatingShared;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services;

public class TMDBService
{
    private readonly HttpClient _client;
    const string OutPath = ".\\tmdb-list-medRating.json";

    private const string searchStringNoAdultFilms = "include_adult=false";
    private const string searchStringLanguage = "&language=en-US";
    private const string searchStringVoteAvgMin = "&vote_average.gte=";
    private const string searchStringVoteAvgMax = "&vote_average.lte=";
    private const string searchStringVoteCountMin = "&vote_count.gte=";
    private const string searchStringPageNumber = "&page=";
    private const string searchStringSortPopDescend = "&sort_by=popularity.desc";

    public TMDBService()
    {
        _client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.themoviedb.org"),
        };
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Constants.TMDBApiKey}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task RequestMovieWithParameters(int startPage, int fetchCount, int minAvgVote, int maxAvgVote, int minVoteCount)
    {
        for (int page = startPage; page < (startPage + fetchCount); page++)
        {
            string searchStringCombined = $"{searchStringNoAdultFilms}{searchStringLanguage}{searchStringPageNumber}{page}{searchStringSortPopDescend}{searchStringVoteAvgMax}{maxAvgVote}{searchStringVoteAvgMin}{minAvgVote}{searchStringVoteCountMin}{minVoteCount}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"/3/discover/movie?{searchStringCombined}", UriKind.Relative),
            };
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                using var parsedResponse = JsonDocument.Parse(body);
                string jsonBody = JsonSerializer.Serialize(parsedResponse, new JsonSerializerOptions { WriteIndented = true });
                await File.AppendAllTextAsync(Path.GetFullPath(OutPath), jsonBody);
                Console.WriteLine($"Appended page {page} to {Path.GetFullPath(OutPath)}");
            }
            
            await Task.Delay(400);
        }
    }

    public async Task FetchMovieDataFromTMDBID(int tmdbID)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/movie/{tmdbID}", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonDocument.Parse(body);
            
            Console.WriteLine(parsedResponse);
        }   
    }

    public async Task<int> GetRevenueInfoFromDB(int tmdbID)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/movie/{tmdbID}", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            TMDBMovieDetailResponse body = (await response.Content.ReadFromJsonAsync<TMDBMovieDetailResponse>())!;
            int revenue = body.Revenue;

            return revenue;
        }
    }

    public async Task<List<string>> GetReviewsFromTmdb(int tmdbID)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/movie/{tmdbID}/reviews?language=en-US&page=1", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            TmdbMovieReviewsResponse body = (await response.Content.ReadFromJsonAsync<TmdbMovieReviewsResponse>())!;
            List<ReviewsResults>? results = body.Results;
            List<string>? reviews = [];

            foreach (ReviewsResults reviewResults in results)
            {
                reviews.Add(reviewResults.ReviewContent ?? string.Empty);
            }

            Console.WriteLine(reviews);
            return reviews;
        }
    }

    public async Task<TMDBExternalDetailData> FetchMovieDataFromIMDBID(string imdbID)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/find/{imdbID}?external_source=imdb_id", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            TMDBExternalDetailResponse body = (await response.Content.ReadFromJsonAsync<TMDBExternalDetailResponse>())!;
            TMDBExternalDetailData data = body.TMDBData[0];

            return data;
        }
    }


    public async Task CombineJsonFiles()
    {
        const string inputPathA = ".\\tmdb-list-lowRating.json";
        const string inputPathB = ".\\tmdb-list-medRating.json";
        const string inputPathC = ".\\tmdb-list-goodRating.json";
        const string outputPath = ".\\tmdb-list-combined.json";

        string contentA = await File.ReadAllTextAsync(Path.GetFullPath(inputPathA));
        string contentB = await File.ReadAllTextAsync(Path.GetFullPath(inputPathB));
        string contentC = await File.ReadAllTextAsync(Path.GetFullPath(inputPathC));

        string splitA = contentA.Split("\"results\": [")[1].Split("\"total_pages\"")[0];
        string splitB = contentB.Split("\"results\": [")[1].Split("\"total_pages\"")[0];
        string splitC = contentC.Split("\"results\": [")[1].Split("\"total_pages\"")[0];

        //a little bit of manual cleanup still required to get rid of some brackets and commas I couldn't figure out
        string combinedContent = splitA + splitB + splitC;

        Console.WriteLine(combinedContent);
        await File.WriteAllTextAsync(Path.GetFullPath(outputPath), combinedContent);
    }


    public async Task PullMovieIDsFromList()
    {
        const string inputPath = ".\\tmdb-list-combined.json";
        const string outputPath = ".\\tmdb-movie-ids.json";

        string content = await File.ReadAllTextAsync(Path.GetFullPath(inputPath));

        var parseInputList = JsonDocument.Parse(content);
        var movieDatabase = parseInputList.RootElement.GetProperty("movie-database");

        for (int i = 0; i <= movieDatabase.GetArrayLength() - 1; i++)
        {
            string id = movieDatabase[i].GetProperty("id").ToString() + ",\n";
            await File.AppendAllTextAsync(Path.GetFullPath(outputPath), id);
        }
    }


    public async Task GetIDofIMDBfromTMDB()
    {
        const string inputPath = ".\\tmdb-movie-ids.json";
        const string outputPath = ".\\tmdb-to-imdb-ids.json";

        string content = await File.ReadAllTextAsync(Path.GetFullPath(inputPath));
        var parseIDList = JsonDocument.Parse(content);
        var idDatabase = parseIDList.RootElement.GetProperty("tmdbIDs");

        for (int i = 0; i < idDatabase.GetArrayLength(); i++)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"/3/movie/{idDatabase[i]}/external_ids", UriKind.Relative),
            };
            using (var response = await _client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                using var parsedResponse = JsonDocument.Parse(body);
                string jsonBody = JsonSerializer.Serialize(parsedResponse, new JsonSerializerOptions { WriteIndented = true });
                await File.AppendAllTextAsync(Path.GetFullPath(outputPath), jsonBody + ",");
            }

            await Task.Delay(250);
        }
    }

    public async Task<List<WatchProvider>> GetWatchProvidersForId(int tmbdId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/movie/{tmbdId}/watch/providers", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var data = JsonSerializer.Deserialize<WatchProviderResponse>(await response.Content.ReadAsStringAsync());

            return data?.Results.US?.Flatrate ?? [];
        }
    }

    public async Task<string> GetIMDbIdFromTMDbId(int tmbdId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/3/movie/{tmbdId}/external_ids", UriKind.Relative),
        };

        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            ExternalIdResponse data = (await response.Content.ReadFromJsonAsync<ExternalIdResponse>())!;

            return data.IMDbId;
        }
    }
}
