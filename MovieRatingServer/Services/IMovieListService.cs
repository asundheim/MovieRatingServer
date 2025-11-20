using MovieRating.Shared;

namespace MovieRatingServer.Services;

public interface IMovieListService
{
    DailyMovieInfo GetDailyMovies();
}