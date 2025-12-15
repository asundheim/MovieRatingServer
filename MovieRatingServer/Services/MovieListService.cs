using System.Text.Json;
using MovieRating.Shared;
using MovieRatingShared;

namespace MovieRatingServer.Services;

public class MovieListService : IMovieListService
{
    private const string _movieDatabaseFileName = "movie-database.json";
    private const int _dailyMovieCount = 3;

    private readonly ITimeService _timeService;
    private readonly List<MovieInfo> _movies;
    private readonly Random _rng = new Random();

    public MovieListService(IWebHostEnvironment env, ITimeService timeService)
    {
        _timeService = timeService;

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

        
        int currentIndex = (_dailyMovieCount * _timeService.GetDailyIndex()) % _movies.Count;
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

    private MovieInfo ConstructMovieInfo(RawMovie rawMovie)
    {
        int ratingsCount = rawMovie.Ratings.Count;
        int randomIndex = _rng.Next(0, ratingsCount);
        RatingInfo ratingInfo = ConstructRatingInfo(rawMovie.Ratings[randomIndex]);

        return new MovieInfo
        {
            Title = rawMovie.Title,
            Year = rawMovie.Year ?? string.Empty,
            Director = rawMovie.Director ?? string.Empty,
            Actors = rawMovie.Actors ?? string.Empty,
            Plot = rawMovie.Plot ?? string.Empty,
            Poster = rawMovie.Poster ?? string.Empty,
            RatingInfo = ratingInfo,
        };
    }

    private static RatingInfo ConstructRatingInfo(RawRating rating)
    {
        return new RatingInfo()
        {
            RatingIndex = rating.Source switch
            {
                "Internet Movie Database" => RatingIndex.IMDB,
                "Rotten Tomatoes" => RatingIndex.RottenTomatoes,
                "Metacritic" => RatingIndex.Metacritic,
                _ => RatingIndex.IMDB
            },
            RatingValue = rating.Value
        };
    }
}