using MovieRatingShared;
using System.Collections.Concurrent;

namespace MovieRatingServer.Services;

public class PlayerScoreService : IPlayerScoreService
{
    private readonly ITimeService _timeService;

    private readonly Lock _lock = new Lock();

    private readonly Dictionary<int, List<int>> _movieScores = new Dictionary<int, List<int>>();
    private readonly Dictionary<double, List<double>> _movieRatings = new Dictionary<double, List<double>>();
    private readonly List<int> _overallScores = new List<int>();

    private int _currentDailyIndex = -1;

    public PlayerScoreService(ITimeService timeService)
    {
        _timeService = timeService;
        _currentDailyIndex = _timeService.GetDailyIndex();
    }

    public void AddPlayerScoreInfo(PlayerScoreInfo score)
    {
        lock (_lock)
        {
            CheckDailyIndex();

            foreach (PlayerMovieScore movieScore in score.MovieScores)
            {
                if (_movieScores.TryGetValue(movieScore.MovieIndex, out List<int>? scores))
                {
                    scores.Add(movieScore.MovieScore);
                }
                else
                {
                    _movieScores[movieScore.MovieIndex] = [movieScore.MovieScore];
                }

                if (_movieRatings.TryGetValue(movieScore.MovieIndex, out List<double>? ratings))
                {
                    ratings.Add(movieScore.MovieRating);
                }
                else
                {
                    _movieRatings[movieScore.MovieIndex] = [movieScore.MovieRating];
                }
            }

            _overallScores.Add(score.OverallScore);
        }
    }

    public AverageDailyPlayerScore GetAverageScores()
    {
        lock (_lock)
        {
            CheckDailyIndex();

            if (_overallScores.Count == 0 || _movieScores.Count == 0 || _movieRatings.Count == 0)
            {
                return new AverageDailyPlayerScore() { AverageMovieScores = [], AverageOverallScore = 0, TotalDailyPlayers = 0 };
            }

            List<MovieScoreInfo> averageMovieScores = new List<MovieScoreInfo>();
            foreach (int movieIndex in _movieScores.Keys)
            {
                averageMovieScores.Add(new MovieScoreInfo()
                {
                    MovieIndex = movieIndex,
                    AverageScore = _movieScores[movieIndex].Average(),
                    AverageRating = _movieRatings[movieIndex].Average(),
                });
            }

            return new AverageDailyPlayerScore()
            {
                AverageMovieScores = averageMovieScores,
                AverageOverallScore = _overallScores.Average(),
                TotalDailyPlayers = _overallScores.Count
            };
        }
    }

    private void CheckDailyIndex()
    {
        int newIndex = _timeService.GetDailyIndex();
        if (_currentDailyIndex != newIndex)
        {
            _currentDailyIndex = newIndex;

            _movieScores.Clear();
            _movieRatings.Clear();
            _overallScores.Clear();
        }
    }
}
