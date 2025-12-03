using MovieRating.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatherMovieInfo
{
    internal class Program
    {
        const string RawIds = "";
        const string OutPath = ".\\movie-list.json";

        public static async Task Main(string[] args)
        {
            string[] ids = RawIds.Split(',');

            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://www.omdbapi.com"),
            };

            int currentIndex = 0;
            List<RawMovie> movieList = new List<RawMovie>();
            try
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    currentIndex = i;
                    string id = ids[i];
                    var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"?i={id}&apiKey={Constants.ApiKey}", UriKind.Relative));
                    var response = await client.SendAsync(request);
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
    }
}
