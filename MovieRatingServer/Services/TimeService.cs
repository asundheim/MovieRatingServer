namespace MovieRatingServer.Services;

public class TimeService : ITimeService
{
    private readonly DateTime _startDate = new DateTime(2025, 12, 01, 08, 00, 00, DateTimeKind.Utc); // DateTime.Now;
    private readonly double _incrementMinutes = 3;
    private readonly double _incrementDays = 1;

    public int GetDailyIndex()
    {
        TimeSpan elapsed = DateTime.Now - _startDate;

        return (int)(elapsed.TotalDays / _incrementDays);
    }
}
