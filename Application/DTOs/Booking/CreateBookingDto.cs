using System.ComponentModel.DataAnnotations;

namespace ConferenceHalls.Application.DTOs.Booking;

public class CreateBookingDto
{
    [Range(1, int.MaxValue)]
    public int ConferenceHallId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Range(6, 22)]
    public int StartHour { get; set; }

    [Range(7, 23)]
    public int EndHour { get; set; }

    public List<int> ServiceIds { get; set; } = [];
}
