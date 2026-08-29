using ConferenceHalls.Application.DTOs.HallService;
using ConferenceHalls.Domain.Entities;

namespace ConferenceHalls.Application.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int ConferenceHallId { get; set; }

    public string ConferenceHallName { get; set; } = string.Empty;

    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset EndAt { get; set; }

    public DateOnly Date { get; set; }

    public int StartHour { get; set; }

    public int EndHour { get; set; }

    public BookingStatus Status { get; set; }

    public string? Message { get; set; }

    public decimal HallHourlyRateSnapshot { get; set; }

    public decimal HallRentalPrice { get; set; }

    public decimal ServicesPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public List<HallServiceDto> Services { get; set; } = [];
}
