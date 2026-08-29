using ConferenceHalls.Application.DTOs.ConferenceHall;

namespace ConferenceHalls.Application.Services;

public interface IConferenceHallsService
{
    Task<IReadOnlyList<ConferenceHallDto>> GetAllAsync();

    Task<IReadOnlyList<ConferenceHallDto>> GetAvailableAsync(
        DateOnly date,
        int startHour,
        int endHour,
        int capacity);

    Task<ConferenceHallDto?> GetByIdAsync(int id);

    Task<ConferenceHallDto> CreateAsync(CreateConferenceHallDto dto);

    Task<ConferenceHallDto?> UpdateAsync(int id, UpdateConferenceHallDto dto);

    Task<bool> DeleteAsync(int id);
}
