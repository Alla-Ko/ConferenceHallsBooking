namespace ConferenceHalls.Domain.Entities;

public class BookingServiceItem
{
    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public int HallServiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
