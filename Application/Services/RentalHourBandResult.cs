namespace ConferenceHalls.Application.Services;

public sealed class RentalHourBandResult
{
    public int MorningHours { get; init; }
    public decimal MorningRevenue { get; init; }
    public int PeakHours { get; init; }
    public decimal PeakRevenue { get; init; }
    public int EveningHours { get; init; }
    public decimal EveningRevenue { get; init; }
    public int StandardHours { get; init; }
    public decimal StandardRevenue { get; init; }
}
