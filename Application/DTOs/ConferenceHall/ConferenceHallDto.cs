using ConferenceHalls.Application.DTOs.HallService;

namespace ConferenceHalls.Application.DTOs.ConferenceHall;

public class ConferenceHallDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; }

    public List<HallServiceDto> AvailableServices { get; set; } = [];
}
