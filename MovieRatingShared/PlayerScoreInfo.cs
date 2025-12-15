namespace MovieRatingShared;

public class PlayerScoreInfo
{
    public required List<PlayerMovieScore> MovieScores { get; init; }
    public required int OverallScore { get; init; }
}
