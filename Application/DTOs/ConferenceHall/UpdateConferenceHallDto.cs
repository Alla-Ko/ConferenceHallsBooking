using System.ComponentModel.DataAnnotations;

namespace ConferenceHalls.Application.DTOs.ConferenceHall;

public class UpdateConferenceHallDto
{
    [MinLength(1)]
    public string? Name { get; set; }

    [Range(1, int.MaxValue)]
    public int? Capacity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true)]
    public decimal? HourlyRate { get; set; }

    public bool? IsActive { get; set; }

    // null: leave as is; []: remove all services
    public List<int>? ServiceIds { get; set; }
}
