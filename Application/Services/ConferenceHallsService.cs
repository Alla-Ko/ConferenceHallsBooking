using ConferenceHalls.Application.DTOs.ConferenceHall;
using ConferenceHalls.Application.DTOs.HallService;
using ConferenceHalls.Domain.Entities;
using ConferenceHalls.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHalls.Application.Services;

public class ConferenceHallsService(
    AppDbContext context,
    IBookingsService bookingsService) : IConferenceHallsService
{
    public async Task<IReadOnlyList<ConferenceHallDto>> GetAllAsync()
    {
        var halls = await context.ConferenceHalls
            .Include(h => h.AvailableServices)
            .AsNoTracking()
            .ToListAsync();

        return [.. halls.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<ConferenceHallDto>> GetAvailableAsync(
        DateOnly date,
        int startHour,
        int endHour,
        int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1.");
        }

        var (startAt, endAt) = BookingSchedule.ToUtcRange(date, startHour, endHour);

        if (startAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("Cannot search halls in the past.");
        }

        var occupiedHallIds = (await bookingsService
            .GetHallIdsWithConfirmedOverlapAsync(startAt, endAt)).ToList();

        var halls = await context.ConferenceHalls
            .Include(h => h.AvailableServices)
            .AsNoTracking()
            .Where(h =>
                h.IsActive
                && h.Capacity >= capacity
                && !occupiedHallIds.Contains(h.Id))
            .ToListAsync();

        return [.. halls.Select(MapToDto)];
    }

    public async Task<ConferenceHallDto?> GetByIdAsync(int id)
    {
        var hall = await context.ConferenceHalls
            .Include(h => h.AvailableServices)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);

        return hall is null ? null : MapToDto(hall);
    }

    public async Task<ConferenceHallDto> CreateAsync(CreateConferenceHallDto dto)
    {
        var services = await GetServicesByIdsAsync(dto.ServiceIds);

        var hall = new ConferenceHall
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            HourlyRate = dto.HourlyRate,
            IsActive = dto.IsActive,
            AvailableServices = services
        };

        context.ConferenceHalls.Add(hall);
        await context.SaveChangesAsync();

        return MapToDto(hall);
    }

    public async Task<ConferenceHallDto?> UpdateAsync(int id, UpdateConferenceHallDto dto)
    {
        var hall = await context.ConferenceHalls
            .Include(h => h.AvailableServices)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hall is null)
        {
            return null;
        }

        if (dto.Name is not null)
        {
            hall.Name = dto.Name;
        }

        if (dto.Capacity is not null)
        {
            hall.Capacity = dto.Capacity.Value;
        }

        if (dto.HourlyRate is not null)
        {
            hall.HourlyRate = dto.HourlyRate.Value;
        }

        if (dto.IsActive is not null)
        {
            hall.IsActive = dto.IsActive.Value;
        }

        if (dto.ServiceIds is not null)
        {
            var services = await GetServicesByIdsAsync(dto.ServiceIds);
            if (hall.AvailableServices is List<HallService> list)
            {
                list.Clear();
                list.AddRange(services);
            }
            else
            {
                hall.AvailableServices.Clear();
                foreach (var service in services)
                    hall.AvailableServices.Add(service);
            }
        }

        await context.SaveChangesAsync();

        return MapToDto(hall);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hall = await context.ConferenceHalls.FindAsync(id);
        if (hall is null)
        {
            return false;
        }

        context.ConferenceHalls.Remove(hall);
        await context.SaveChangesAsync();
        return true;
    }

    private async Task<List<HallService>> GetServicesByIdsAsync(IReadOnlyCollection<int> serviceIds)
    {
        var distinctIds = serviceIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new();
        }

        var services = await context.HallServices
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync();

        if (services.Count != distinctIds.Count)
        {
            var missingIds = distinctIds.Except(services.Select(s => s.Id));
            throw new ArgumentException(
                $"Hall services not found: {string.Join(", ", missingIds)}");
        }

        return services;
    }

    private static ConferenceHallDto MapToDto(ConferenceHall hall)
    {
        return new ConferenceHallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity,
            HourlyRate = hall.HourlyRate,
            IsActive = hall.IsActive,
            AvailableServices = [.. hall.AvailableServices
                .Select(s => new HallServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price
                })]
        };
    }
}