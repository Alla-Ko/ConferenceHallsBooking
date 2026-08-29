namespace ConferenceHalls.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public int ConferenceHallId { get; set; }

    public ConferenceHall ConferenceHall { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? StatusReason { get; set; }

    public decimal HallHourlyRateSnapshot { get; set; }

    public decimal HallRentalPrice { get; set; }

    public decimal ServicesPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public ICollection<BookingServiceItem> Services { get; set; } = new List<BookingServiceItem>();
}
