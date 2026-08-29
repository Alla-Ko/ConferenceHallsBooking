namespace ConferenceHalls.Application.DTOs.HallService;

public class CreateHallServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}