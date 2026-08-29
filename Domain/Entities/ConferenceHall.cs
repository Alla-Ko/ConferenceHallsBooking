namespace ConferenceHalls.Domain.Entities;

public class ConferenceHall
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<HallService> AvailableServices { get; set; } = new List<HallService>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
