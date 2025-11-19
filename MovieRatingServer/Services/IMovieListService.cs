using MovieRating.Shared;

namespace MovieRatingServer.Services;

public interface IMovieListService
{
    IEnumerable<MovieInfo> GetDailyMovies(int numberOfMovies);

    int DayCount { get; }
}