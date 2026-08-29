using ConferenceHalls.Application.DTOs.Booking;
using ConferenceHalls.Application.DTOs.HallService;
using ConferenceHalls.Domain.Entities;
using ConferenceHalls.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConferenceHalls.Application.Services;

public class BookingsService(
    AppDbContext context,
    IBookingPriceCalculator priceCalculator) : IBookingsService
{
    public async Task<IReadOnlyList<BookingDto>> GetAllAsync(string userId, bool isAdmin)
    {
        var query = context.Bookings
            .Include(b => b.ConferenceHall)
            .Include(b => b.Services)
            .AsNoTracking();

        if (!isAdmin)
        {
            query = query.Where(b => b.UserId == userId);
        }

        var bookings = await query
            .OrderByDescending(b => b.StartAt)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<BookingDto?> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var booking = await context.Bookings
            .Include(b => b.ConferenceHall)
            .Include(b => b.Services)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return null;
        }

        if (!isAdmin && booking.UserId != userId)
        {
            return null;
        }

        return MapToDto(booking);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto, string userId)
    {
        var (startAt, endAt) = BookingSchedule.ToUtcRange(dto.Date, dto.StartHour, dto.EndHour);

        if (startAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("Cannot create a booking in the past.");
        }

        var hall = await context.ConferenceHalls
            .Include(h => h.AvailableServices)
            .FirstOrDefaultAsync(h => h.Id == dto.ConferenceHallId);

        if (hall is null)
        {
            throw new KeyNotFoundException("Conference hall was not found.");
        }

        if (!hall.IsActive)
        {
            throw new ArgumentException("Conference hall is not active.");
        }

        var serviceIds = dto.ServiceIds.Distinct().ToList();
        var selectedServices = hall.AvailableServices
            .Where(s => serviceIds.Contains(s.Id))
            .ToList();

        if (selectedServices.Count != serviceIds.Count)
        {
            var missing = serviceIds.Except(selectedServices.Select(s => s.Id));
            throw new ArgumentException(
                $"Services are not available in this hall: {string.Join(", ", missing)}");
        }

        var price = priceCalculator.Calculate(
            hall.HourlyRate,
            startAt,
            endAt,
            selectedServices.Select(s => s.Price).ToList());

        var booking = new Booking
        {
            UserId = userId,
            ConferenceHallId = hall.Id,
            ConferenceHall = hall,
            StartAt = startAt,
            EndAt = endAt,
            Status = BookingStatus.Pending,
            HallHourlyRateSnapshot = price.HallHourlyRate,
            HallRentalPrice = price.HallRentalPrice,
            ServicesPrice = price.ServicesPrice,
            TotalPrice = price.TotalPrice,
            Services = selectedServices.Select(s => new BookingServiceItem
            {
                HallServiceId = s.Id,
                Name = s.Name,
                Price = s.Price
            }).ToList()
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<IReadOnlyList<int>> GetHallIdsWithConfirmedOverlapAsync(
        DateTime startAtUtc,
        DateTime endAtUtc)
    {
        return await context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .WhereOverlaps(startAtUtc, endAtUtc)
            .Select(b => b.ConferenceHallId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<StatusUpdateResult> UpdateStatusAsync(
        int id,
        UpdateBookingStatusDto dto,
        string userId,
        bool isAdmin)
    {
        if (dto.Status is not (
            BookingStatus.Confirmed or
            BookingStatus.Rejected or
            BookingStatus.Cancelled))
        {
            return new StatusUpdateResult(400, null, "Status must be Confirmed, Rejected or Cancelled.");
        }

        if (!isAdmin)
        {
            return await CancelOwnPendingAsync(id, dto, userId);
        }

        if (dto.Status == BookingStatus.Confirmed)
        {
            return await ConfirmAsync(id);
        }

        return await AdminSetStatusAsync(id, dto.Status, dto.Message);
    }

    private async Task<StatusUpdateResult> CancelOwnPendingAsync(
        int id,
        UpdateBookingStatusDto dto,
        string userId)
    {
        if (dto.Status != BookingStatus.Cancelled)
        {
            return new StatusUpdateResult(403, null, "Users can only cancel their own pending bookings.");
        }

        var booking = await LoadBookingAsync(id);
        if (booking is null)
        {
            return new StatusUpdateResult(404, null, null);
        }

        if (booking.UserId != userId)
        {
            return new StatusUpdateResult(403, null, "You can only cancel your own bookings.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return new StatusUpdateResult(409, null, "Only a pending booking can be cancelled by the user.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.StatusReason = dto.Message;
        await context.SaveChangesAsync();

        return new StatusUpdateResult(200, MapToDto(booking), null);
    }

    private async Task<StatusUpdateResult> AdminSetStatusAsync(
        int id,
        BookingStatus status,
        string? message)
    {
        var booking = await LoadBookingAsync(id);
        if (booking is null)
        {
            return new StatusUpdateResult(404, null, null);
        }

        if (booking.Status is BookingStatus.Rejected or BookingStatus.Cancelled)
        {
            return new StatusUpdateResult(409, null, "This booking is already closed.");
        }

        booking.Status = status;
        booking.StatusReason = message;
        await context.SaveChangesAsync();

        return new StatusUpdateResult(200, MapToDto(booking), null);
    }

    private async Task<StatusUpdateResult> ConfirmAsync(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var booking = await LoadBookingAsync(id);
        if (booking is null)
        {
            return new StatusUpdateResult(404, null, null);
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return new StatusUpdateResult(409, null, "Only a pending booking can be confirmed.");
        }

        var hall = booking.ConferenceHall;

        if (!hall.IsActive)
        {
            return await AutoRejectAsync(booking, BookingStatusReasons.HallInactive, transaction);
        }

        var selectedIds = booking.Services.Select(s => s.HallServiceId).ToList();
        var currentServices = hall.AvailableServices
            .Where(s => selectedIds.Contains(s.Id))
            .ToList();

        if (currentServices.Count != selectedIds.Count)
        {
            return await AutoRejectAsync(
                booking,
                BookingStatusReasons.ServicesNoLongerAvailable,
                transaction);
        }

        var currentPrice = priceCalculator.Calculate(
            hall.HourlyRate,
            booking.StartAt,
            booking.EndAt,
            currentServices.Select(s => s.Price).ToList());

        if (!PricesMatch(booking, currentPrice))
        {
            return await AutoRejectAsync(booking, BookingStatusReasons.PriceMismatch, transaction);
        }

        var slotTaken = await OverlappingOnSameHall(booking, BookingStatus.Confirmed)
            .AnyAsync();

        if (slotTaken)
        {
            return await AutoRejectAsync(booking, BookingStatusReasons.TimeSlotOccupied, transaction);
        }

        booking.Status = BookingStatus.Confirmed;
        booking.StatusReason = null;

        var overlappingPending = await OverlappingOnSameHall(booking, BookingStatus.Pending)
            .ToListAsync();

        foreach (var other in overlappingPending)
        {
            other.Status = BookingStatus.Rejected;
            other.StatusReason = BookingStatusReasons.TimeSlotOccupied;
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        var message = overlappingPending.Count == 0
            ? "Booking confirmed."
            : $"Booking confirmed. Rejected {overlappingPending.Count} overlapping pending booking(s).";

        var dto = MapToDto(booking);
        dto.Message = message;
        return new StatusUpdateResult(200, dto, null);
    }

    private async Task<StatusUpdateResult> AutoRejectAsync(
        Booking booking,
        string reason,
        IDbContextTransaction transaction)
    {
        booking.Status = BookingStatus.Rejected;
        booking.StatusReason = reason;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        var dto = MapToDto(booking);
        dto.Message = reason;
        return new StatusUpdateResult(200, dto, null);
    }

    private async Task<Booking?> LoadBookingAsync(int id)
    {
        return await context.Bookings
            .Include(b => b.ConferenceHall)
                .ThenInclude(h => h.AvailableServices)
            .Include(b => b.Services)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    private IQueryable<Booking> OverlappingOnSameHall(Booking booking, BookingStatus status) =>
        context.Bookings
            .Where(b =>
                b.Id != booking.Id
                && b.ConferenceHallId == booking.ConferenceHallId
                && b.Status == status)
            .WhereOverlaps(booking.StartAt, booking.EndAt);

    private static bool PricesMatch(Booking booking, BookingPriceResult current)
    {
        return booking.HallHourlyRateSnapshot == current.HallHourlyRate
            && booking.HallRentalPrice == current.HallRentalPrice
            && booking.ServicesPrice == current.ServicesPrice
            && booking.TotalPrice == current.TotalPrice;
    }

    private static BookingDto MapToDto(Booking booking)
    {
        var localStart = BookingSchedule.ToUserLocal(booking.StartAt);
        var localEnd = BookingSchedule.ToUserLocal(booking.EndAt);

        return new BookingDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            ConferenceHallId = booking.ConferenceHallId,
            ConferenceHallName = booking.ConferenceHall.Name,
            StartAt = localStart,
            EndAt = localEnd,
            Date = DateOnly.FromDateTime(localStart.DateTime),
            StartHour = localStart.Hour,
            EndHour = localEnd.Hour,
            Status = booking.Status,
            Message = booking.StatusReason,
            HallHourlyRateSnapshot = booking.HallHourlyRateSnapshot,
            HallRentalPrice = booking.HallRentalPrice,
            ServicesPrice = booking.ServicesPrice,
            TotalPrice = booking.TotalPrice,
            Services = booking.Services
                .Select(s => new HallServiceDto
                {
                    Id = s.HallServiceId,
                    Name = s.Name,
                    Price = s.Price
                })
                .ToList()
        };
    }
}
