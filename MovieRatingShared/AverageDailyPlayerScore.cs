namespace MovieRatingShared;

public class AverageDailyPlayerScore
{
    public required List<MovieScoreInfo> AverageMovieScores { get; init; }

    public required double AverageOverallScore { get; init; }

    public required int TotalDailyPlayers { get; init; }
}
