namespace ConferenceHalls.Domain.Entities;

public static class BookingStatusReasons
{
    public const string TimeSlotOccupied = "Time slot is occupied";
    public const string PriceMismatch = "Price mismatch";
    public const string ServicesNoLongerAvailable = "Selected services are no longer available in this hall";
    public const string HallInactive = "Hall is no longer active";
}
