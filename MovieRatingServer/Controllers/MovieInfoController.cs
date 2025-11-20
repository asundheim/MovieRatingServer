using Microsoft.AspNetCore.Mvc;
using MovieRating.Shared;
using MovieRatingServer.Services;

namespace MovieRatingServer.Controllers;

[ApiController]
public class MovieInfoController : ControllerBase
{
    private readonly ILogger<MovieInfoController> _logger;
    private readonly IMovieListService _movieListService;

    public MovieInfoController(ILogger<MovieInfoController> logger, IMovieListService movieService)
    {
        _logger = logger;
        _movieListService = movieService;
    }

    [HttpGet("MovieInfo")]
    public DailyMovieInfo Get()
    {
        return _movieListService.GetDailyMovies();
    }
}