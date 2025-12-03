using System.Text.Json;
using MovieRating.Shared;

namespace MovieRatingServer.Services;

public class MovieListService : IMovieListService
{
    private const string _movieDatabaseFileName = "movie-database.json";
    private readonly DateTime _startDate = new DateTime(2025, 12, 01, 08, 00, 00, DateTimeKind.Utc); // DateTime.Now;
    private readonly double _incrementMinutes = 60;
    private readonly double _incrementDays = 1;
    private const int _dailyMovieCount = 3;

    private readonly List<MovieInfo> _movies;
    private readonly Random _rng;

    public MovieListService(IWebHostEnvironment env)
    {
        _rng = new Random();

        var path = Path.Combine(env.ContentRootPath, _movieDatabaseFileName);

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            RawMovieList root = JsonSerializer.Deserialize<RawMovieList>(json, options) ?? throw new InvalidOperationException("Failed to deserialize movie database");
            _movies = root.MovieDatabase.Select(m => ConstructMovieInfo(m)).ToList();
        }
        else
        {
            throw new InvalidOperationException($"Unable to resolve movie database at {path}");
        }
    }

    public DailyMovieInfo GetDailyMovies()
    {
        var movies = new List<MovieInfo>();

        TimeSpan elapsed = DateTime.Now - _startDate;
        int currentIndex = (_dailyMovieCount * (int)(elapsed.TotalDays / _incrementDays)) % _movies.Count;
        for (int i = 0; i < _dailyMovieCount; i++)
        {
            movies.Add(_movies[(currentIndex + i) % _movies.Count]);
        }

        return new DailyMovieInfo()
        {
            Movies = movies,
            DailyId = currentIndex,
        };
    }

    private MovieInfo ConstructMovieInfo(RawMovie m)
    {
        int ratingsCount = m.Ratings?.Count ?? 0;
        int random = ratingsCount > 0 ? _rng.Next(0, ratingsCount) : 0;
        var (source, value) = PickRandomRating(m, random, _rng);

        return new MovieInfo
        {
            Title = m.Title,
            Year = m.Year ?? string.Empty,
            Director = m.Director ?? string.Empty,
            Actors = m.Actors ?? string.Empty,
            Plot = m.Plot ?? string.Empty,
            Poster = m.Poster ?? string.Empty,
            RatingSource = source,
            RatingValue = value,
            RandomRatingInt = random,
        };
    }

    private static (string Source, string Value) PickRandomRating(RawMovie m, int x, Random rng)
    {
        if (m.Ratings is { Count: > 0 })
        {
            var r = m.Ratings[x];
            return (r.Source ?? string.Empty, r.Value ?? string.Empty);
        }

        if (!string.IsNullOrEmpty(m.imdbRating))
            return ("Internet Movie Database", m.imdbRating);

        return (string.Empty, string.Empty);
    }
}