using ConferenceHalls.Application.DTOs.Booking;

namespace ConferenceHalls.Application.Services;

public interface IBookingsService
{
    Task<IReadOnlyList<BookingDto>> GetAllAsync(string userId, bool isAdmin);

    Task<BookingDto?> GetByIdAsync(int id, string userId, bool isAdmin);

    Task<BookingDto> CreateAsync(CreateBookingDto dto, string userId);

    Task<StatusUpdateResult> UpdateStatusAsync(
        int id,
        UpdateBookingStatusDto dto,
        string userId,
        bool isAdmin);

    Task<IReadOnlyList<int>> GetHallIdsWithConfirmedOverlapAsync(
        DateTime startAtUtc,
        DateTime endAtUtc);
}

public sealed record StatusUpdateResult(
    int HttpStatusCode,
    BookingDto? Booking,
    string? Error);
