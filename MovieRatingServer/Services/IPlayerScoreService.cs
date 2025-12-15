using MovieRatingShared;

namespace MovieRatingServer.Services;

public interface IPlayerScoreService
{
    void AddPlayerScoreInfo(PlayerScoreInfo score);

    AverageDailyPlayerScore GetAverageScores();
}
