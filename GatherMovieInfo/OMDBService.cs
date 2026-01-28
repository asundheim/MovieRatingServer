using GatherMovieInfo;
using MovieRating.Shared;
using MovieRatingShared;
using System.Net.Http.Json;
using System.Text.Json;

namespace Services;

public class OMDBService
{
    private readonly HttpClient _client;

    const string RawIds = "";
    const string OutPath = ".\\movie-list.json";
    string[] ids = RawIds.Split(',');

    public OMDBService()
    {
        _client = new HttpClient()
        {
            BaseAddress = new Uri("http://www.omdbapi.com"),
        };
    }

    public async Task OMDBFetchData()
    {
       
        int currentIndex = 0;
        List<RawMovie> movieList = new List<RawMovie>();
        try
        {
            for (int i = 0; i < ids.Length; i++)
            {
                currentIndex = i;
                string id = ids[i];
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"?i={id}&apiKey={Constants.OMDBApiKey}", UriKind.Relative));
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                RawMovie movie = JsonSerializer.Deserialize<RawMovie>(json) ?? throw new Exception("Failed to deserialize to RawMovie");
                movieList.Add(movie);

                Console.WriteLine(movie.Title);

                await Task.Delay(500);
            }
        }
        catch
        {
            Console.WriteLine($"Failed at index {currentIndex}");
            Console.WriteLine("Remaining ids:");
            Console.WriteLine(string.Join(",", ids.Skip(currentIndex)));
        }

        RawMovieList rawMovieList = new RawMovieList() { MovieDatabase = movieList };
        await File.WriteAllTextAsync(Path.GetFullPath(OutPath), JsonSerializer.Serialize(rawMovieList, new JsonSerializerOptions() { WriteIndented = true }));

        Console.WriteLine($"Done. Wrote {rawMovieList.MovieDatabase.Count} movies to {Path.GetFullPath(OutPath)}");
    }



    public async Task<RawMovie> FetchOMDBDataFromIMDBID(string imdbID)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"?i={imdbID}&apiKey={Constants.OMDBApiKey}", UriKind.Relative));


        using (var response = await _client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            RawMovie data = (await response.Content.ReadFromJsonAsync<RawMovie>())!;

            return data;
        }
    }
}
