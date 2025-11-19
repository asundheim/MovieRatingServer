using System.Text.Json;
using System.Text.Json.Serialization;
using MovieRating.Shared;

namespace MovieRatingServer.Services;

public class MovieListService : IMovieListService
{
    private readonly List<MovieInfo> _movies;
    private DateTime _date;
    private int _dayCount;
    private readonly Random _rng;
    private DateTime _startDate;

    int IMovieListService.DayCount => _dayCount;

    public MovieListService(IWebHostEnvironment env)
    {
        _rng = new Random();

        var path = Path.Combine(env.ContentRootPath, "movie-database.json");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var root = JsonSerializer.Deserialize<RawMovieList>(json, options);
            _movies = root?.MovieDatabase?.Select(m => Map(m, _rng)).ToList() ?? new List<MovieInfo>();
        }
        else
        {
            _movies = new List<MovieInfo>();
        }
    }

    public void SetStartDate(DateTime date)
    {
        _startDate = date;
    }

    private int DateIncrementCount(DateTime startDate, TimeSpan increment)
    {
        var elsapsedTime = DateTime.Now - startDate;
        return (int)(elsapsedTime.Ticks / increment.Ticks);
    }

    public int DayCount()
    {
        _dayCount = 0; 
        if (_date == default) 
        { 
            _date = DateTime.Now; return 0; 
        }
        
        var startDate = new DateTime(2025, 11, 18, 16, 37, 00);
        var elapsed = DateTime.Now - _date;
        var elapsed2 = DateTime.Now - startDate;
        var elapsedSeconds = (int)elapsed.TotalSeconds;

        int increments = DateIncrementCount(startDate, TimeSpan.FromMinutes(1));
        int timeIncrementsPassed = (int)elapsed.TotalMinutes; 
        _dayCount += increments; 
        return _dayCount;
    }


    private MovieInfo GetMovieAt(int index)
    {
        return _movies[index];
    }

    private int[] GetDailyIndexArray(int index)
    {
        int tempIndex = (index*5);
        int[] ints = [tempIndex, tempIndex + 1, tempIndex + 2, tempIndex + 3, tempIndex + 4];

        return ints;
    }

    public IEnumerable<MovieInfo> GetDailyMovies(int numberOfMovies)
    {
        var result = new List<MovieInfo>();
        int numberOfResults = numberOfMovies;
        int resultsReturned = 0;
        int[] dailyIndexes = GetDailyIndexArray(DayCount());

        while (result.Count < numberOfResults && resultsReturned < dailyIndexes.Length)
        {
            var tempIndex = dailyIndexes[resultsReturned];
            if (tempIndex < 0 || tempIndex >= _movies.Count)
            {
                resultsReturned++;
                continue;
            }
            var movie = GetMovieAt(tempIndex);
            if (movie is null)
            {
                resultsReturned++;
                continue;
            }
            result.Add(movie);
            resultsReturned++;
        }
        return result;
    }


    private static MovieInfo Map(RawMovie m, Random rng)
    {
        int ratingsCount = m.Ratings?.Count ?? 0;
        int random = ratingsCount > 0 ? rng.Next(0, ratingsCount) : 0;
        var (source, value) = PickRandomRating(m, random, rng);

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