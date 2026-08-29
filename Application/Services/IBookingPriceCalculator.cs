namespace ConferenceHalls.Application.Services;

public interface IBookingPriceCalculator
{
    BookingPriceResult Calculate(
        decimal hourlyRate,
        DateTime startAtUtc,
        DateTime endAtUtc,
        IReadOnlyCollection<decimal> servicePrices);

    RentalHourBandResult SplitRentalByHourBand(
        decimal hourlyRate,
        DateTime startAtUtc,
        DateTime endAtUtc);
}
