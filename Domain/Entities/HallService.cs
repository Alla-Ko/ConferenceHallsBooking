namespace ConferenceHalls.Domain.Entities;

public class HallService
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ICollection<ConferenceHall> ConferenceHalls { get; set; } = new List<ConferenceHall>();
}