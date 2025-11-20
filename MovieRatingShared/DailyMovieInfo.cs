namespace MovieRating.Shared;

public class DailyMovieInfo
{
    public required List<MovieInfo> Movies { get; set; }
    public required int DailyId { get; set; }
}
