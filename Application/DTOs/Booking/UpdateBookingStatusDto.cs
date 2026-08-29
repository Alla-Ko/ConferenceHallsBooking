using ConferenceHalls.Domain.Entities;

namespace ConferenceHalls.Application.DTOs.Booking;

public class UpdateBookingStatusDto
{
    public BookingStatus Status { get; set; }

    public string? Message { get; set; }
}
