using System.ComponentModel.DataAnnotations;

namespace ConferenceHalls.Application.DTOs.ConferenceHall;

public class CreateConferenceHallDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ServiceIds { get; set; } = [];
}
