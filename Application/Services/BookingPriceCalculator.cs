using ConferenceHalls.Domain.Entities;

namespace ConferenceHalls.Application.Services;

public sealed class BookingPriceCalculator : IBookingPriceCalculator
{
    public BookingPriceResult Calculate(
        decimal hourlyRate,
        DateTime startAtUtc,
        DateTime endAtUtc,
        IReadOnlyCollection<decimal> servicePrices)
    {
        var hallRentalPrice = CalculateHallRental(hourlyRate, startAtUtc, endAtUtc);
        var servicesPrice = Round(servicePrices.Sum());

        return new BookingPriceResult
        {
            HallHourlyRate = hourlyRate,
            HallRentalPrice = hallRentalPrice,
            ServicesPrice = servicesPrice,
            TotalPrice = Round(hallRentalPrice + servicesPrice)
        };
    }

    private static decimal CalculateHallRental(
        decimal hourlyRate,
        DateTime startAtUtc,
        DateTime endAtUtc)
    {
        decimal rental = 0;
        foreach (var hour in EnumerateLocalHours(startAtUtc, endAtUtc))
        {
            rental += hourlyRate * MultiplierFor(GetHourBand(hour));
        }

        return Round(rental);
    }

    public RentalHourBandResult SplitRentalByHourBand(
        decimal hourlyRate,
        DateTime startAtUtc,
        DateTime endAtUtc)
    {
        var morningHours = 0;
        var peakHours = 0;
        var eveningHours = 0;
        var standardHours = 0;
        decimal morningRevenue = 0;
        decimal peakRevenue = 0;
        decimal eveningRevenue = 0;
        decimal standardRevenue = 0;

        foreach (var hour in EnumerateLocalHours(startAtUtc, endAtUtc))
        {
            var band = GetHourBand(hour);
            var amount = hourlyRate * MultiplierFor(band);
            switch (band)
            {
                case HourBand.Peak:
                    peakHours++;
                    peakRevenue += amount;
                    break;
                case HourBand.Morning:
                    morningHours++;
                    morningRevenue += amount;
                    break;
                case HourBand.Evening:
                    eveningHours++;
                    eveningRevenue += amount;
                    break;
                default:
                    standardHours++;
                    standardRevenue += amount;
                    break;
            }
        }

        return new RentalHourBandResult
        {
            MorningHours = morningHours,
            MorningRevenue = Round(morningRevenue),
            PeakHours = peakHours,
            PeakRevenue = Round(peakRevenue),
            EveningHours = eveningHours,
            EveningRevenue = Round(eveningRevenue),
            StandardHours = standardHours,
            StandardRevenue = Round(standardRevenue)
        };
    }

    private enum HourBand
    {
        Morning,
        Peak,
        Evening,
        Standard
    }

    private static IEnumerable<int> EnumerateLocalHours(DateTime startAtUtc, DateTime endAtUtc)
    {
        var localStart = BookingSchedule.ToUserLocal(startAtUtc);
        var localEnd = BookingSchedule.ToUserLocal(endAtUtc);
        for (var hour = localStart.Hour; hour < localEnd.Hour; hour++)
        {
            yield return hour;
        }
    }

    private static HourBand GetHourBand(int hour) => hour switch
    {
        >= 12 and < 14 => HourBand.Peak,
        >= 6 and < 9 => HourBand.Morning,
        >= 18 and < 23 => HourBand.Evening,
        >= 9 and < 12 => HourBand.Standard,
        >= 14 and < 18 => HourBand.Standard,
        _ => throw new ArgumentOutOfRangeException(
            nameof(hour),
            hour,
            "Hour is outside the allowed booking window 06:00-23:00.")
    };

    private static decimal MultiplierFor(HourBand band) => band switch
    {
        HourBand.Peak => 1.15m,
        HourBand.Morning => 0.90m,
        HourBand.Evening => 0.80m,
        HourBand.Standard => 1.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(band), band, null)
    };

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
