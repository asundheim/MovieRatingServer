using Microsoft.AspNetCore.Mvc;
using MovieRating.Shared;
using MovieRatingServer.Services;
using MovieRatingShared;

namespace MovieRatingServer.Controllers;

[ApiController]
public class MovieInfoController : ControllerBase
{
    private readonly ILogger<MovieInfoController> _logger;
    private readonly IMovieListService _movieListService;
    private readonly IPlayerScoreService _playerScoreService;

    public MovieInfoController(ILogger<MovieInfoController> logger, IMovieListService movieService, IPlayerScoreService playerScoreService)
    {
        _logger = logger;
        _movieListService = movieService;
        _playerScoreService = playerScoreService;
    }

    [HttpGet("MovieInfo")]
    public DailyMovieInfo GetMovieInfo()
    {
        return _movieListService.GetDailyMovies();
    }

    [HttpPost("PlayerScoreInfo")]
    public async Task PostPlayerScoreInfo(PlayerScoreInfo scoreInfo)
    {
        _playerScoreService.AddPlayerScoreInfo(scoreInfo);
    }

    [HttpGet("AverageScoreInfo")]
    public AverageDailyPlayerScore GetAverageScoreInfo()
    {
        return _playerScoreService.GetAverageScores();
    }
}