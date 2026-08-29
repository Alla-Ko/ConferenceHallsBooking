namespace ConferenceHalls.Domain.Entities;

public static class BookingSchedule
{
    public static void ValidateHours(int startHour, int endHour)
    {
        if (endHour <= startHour)
        {
            throw new ArgumentException("End hour must be later than start hour.");
        }

        if (startHour < 6 || endHour > 23)
        {
            throw new ArgumentException("Booking is allowed only from 06:00 to 23:00.");
        }
    }

    public static DateTime ToUtc(DateOnly date, int hour)
    {
        var userLocal = new DateTimeOffset(
            date.Year,
            date.Month,
            date.Day,
            hour,
            0,
            0,
            BookingTimeZone.Offset);

        return DateTime.SpecifyKind(userLocal.UtcDateTime, DateTimeKind.Utc);
    }

    public static (DateTime StartUtc, DateTime EndUtc) ToUtcRange(
        DateOnly date,
        int startHour,
        int endHour)
    {
        ValidateHours(startHour, endHour);
        return (ToUtc(date, startHour), ToUtc(date, endHour));
    }

    public static IQueryable<Booking> WhereOverlaps(
        this IQueryable<Booking> bookings,
        DateTime startAtUtc,
        DateTime endAtUtc) =>
        bookings.Where(b => b.StartAt < endAtUtc && startAtUtc < b.EndAt);

    public static DateTimeOffset ToUserLocal(DateTime utc)
    {
        var utcTime = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return new DateTimeOffset(utcTime).ToOffset(BookingTimeZone.Offset);
    }

    public static int BookableHoursPerDay => 17;

    public static (DateTime StartUtc, DateTime EndExclusiveUtc) ToUtcPeriod(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            throw new ArgumentException("'to' must be on or after 'from'.");
        }

        return (ToUtc(from, 0), ToUtc(to.AddDays(1), 0));
    }

    public static int HoursCount(DateTime startAtUtc, DateTime endAtUtc)
    {
        var localStart = ToUserLocal(startAtUtc);
        var localEnd = ToUserLocal(endAtUtc);
        return localEnd.Hour - localStart.Hour;
    }
}
