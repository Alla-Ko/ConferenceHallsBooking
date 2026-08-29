namespace ConferenceHalls.Application.Services;

public sealed class BookingPriceResult
{
    public decimal HallHourlyRate { get; init; }

    public decimal HallRentalPrice { get; init; }

    public decimal ServicesPrice { get; init; }

    public decimal TotalPrice { get; init; }
}
