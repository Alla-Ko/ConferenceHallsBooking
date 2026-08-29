using ConferenceHalls.Application.DTOs.Reports;
using ConferenceHalls.Domain.Entities;
using ConferenceHalls.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHalls.Application.Services;

public class ReportsService(
    AppDbContext context,
    IBookingPriceCalculator priceCalculator) : IReportsService
{
    public async Task<HallRevenueReportDto> GetHallRevenueAsync(DateOnly from, DateOnly to)
    {
        var bookings = await GetConfirmedInPeriodAsync(from, to);
        var halls = await context.ConferenceHalls.AsNoTracking().ToListAsync();

        var rows = halls
            .Select(hall =>
            {
                var hallBookings = bookings.Where(b => b.ConferenceHallId == hall.Id).ToList();
                var rental = hallBookings.Sum(b => b.HallRentalPrice);
                var services = hallBookings.Sum(b => b.ServicesPrice);
                return new HallRevenueRowDto
                {
                    HallId = hall.Id,
                    HallName = hall.Name,
                    RentalRevenue = rental,
                    ServicesRevenue = services,
                    TotalRevenue = rental + services
                };
            })
            .OrderBy(r => r.HallName)
            .ToList();

        return new HallRevenueReportDto
        {
            From = from,
            To = to,
            Halls = rows,
            TotalRentalRevenue = rows.Sum(r => r.RentalRevenue),
            TotalServicesRevenue = rows.Sum(r => r.ServicesRevenue),
            TotalRevenue = rows.Sum(r => r.TotalRevenue)
        };
    }

    public async Task<ServiceRevenueReportDto> GetServiceRevenueAsync(DateOnly from, DateOnly to)
    {
        var bookings = await GetConfirmedInPeriodAsync(from, to);

        var rows = bookings
            .SelectMany(b => b.Services)
            .GroupBy(s => s.HallServiceId)
            .Select(g => new ServiceRevenueRowDto
            {
                ServiceId = g.Key,
                Name = g.First().Name,
                TimesBooked = g.Count(),
                Revenue = g.Sum(s => s.Price)
            })
            .OrderByDescending(r => r.Revenue)
            .ToList();

        return new ServiceRevenueReportDto
        {
            From = from,
            To = to,
            Services = rows,
            TotalRevenue = rows.Sum(r => r.Revenue)
        };
    }

    public async Task<HallRentalHoursReportDto> GetRentalHoursAsync(DateOnly from, DateOnly to)
    {
        var bookings = await GetConfirmedInPeriodAsync(from, to);
        var halls = await context.ConferenceHalls.AsNoTracking().ToListAsync();

        var rows = halls.Select(hall =>
        {
            var morning = new HourBandDto();
            var peak = new HourBandDto();
            var evening = new HourBandDto();
            var standard = new HourBandDto();

            foreach (var booking in bookings.Where(b => b.ConferenceHallId == hall.Id))
            {
                var split = priceCalculator.SplitRentalByHourBand(
                    booking.HallHourlyRateSnapshot,
                    booking.StartAt,
                    booking.EndAt);

                morning.Hours += split.MorningHours;
                morning.Revenue += split.MorningRevenue;
                peak.Hours += split.PeakHours;
                peak.Revenue += split.PeakRevenue;
                evening.Hours += split.EveningHours;
                evening.Revenue += split.EveningRevenue;
                standard.Hours += split.StandardHours;
                standard.Revenue += split.StandardRevenue;
            }

            return new HallRentalHoursRowDto
            {
                HallId = hall.Id,
                HallName = hall.Name,
                Morning = morning,
                Peak = peak,
                Evening = evening,
                Standard = standard,
                TotalRentalRevenue =
                    morning.Revenue + peak.Revenue + evening.Revenue + standard.Revenue
            };
        }).OrderBy(r => r.HallName).ToList();

        return new HallRentalHoursReportDto
        {
            From = from,
            To = to,
            Halls = rows
        };
    }

    public async Task<HallUtilizationReportDto> GetUtilizationAsync(DateOnly from, DateOnly to)
    {
        var bookings = await GetConfirmedInPeriodAsync(from, to);
        var halls = await context.ConferenceHalls.AsNoTracking().ToListAsync();
        var daysInPeriod = to.DayNumber - from.DayNumber + 1;
        var availableHours = daysInPeriod * BookingSchedule.BookableHoursPerDay;

        var rows = halls.Select(hall =>
        {
            var confirmedHours = bookings
                .Where(b => b.ConferenceHallId == hall.Id)
                .Sum(b => BookingSchedule.HoursCount(b.StartAt, b.EndAt));

            var occupancy = availableHours == 0
                ? 0
                : Math.Round(confirmedHours * 100m / availableHours, 2, MidpointRounding.AwayFromZero);

            return new HallUtilizationRowDto
            {
                HallId = hall.Id,
                HallName = hall.Name,
                ConfirmedHours = confirmedHours,
                AvailableHours = availableHours,
                OccupancyPercent = occupancy
            };
        }).OrderBy(r => r.HallName).ToList();

        return new HallUtilizationReportDto
        {
            From = from,
            To = to,
            DaysInPeriod = daysInPeriod,
            BookableHoursPerDay = BookingSchedule.BookableHoursPerDay,
            Halls = rows
        };
    }

    public async Task<UserRevenueReportDto> GetUserRevenueAsync(DateOnly from, DateOnly to)
    {
        var bookings = await GetConfirmedInPeriodAsync(from, to);

        var rows = bookings
            .GroupBy(b => b.UserId)
            .Select(g =>
            {
                var user = g.First().User;
                return new UserRevenueRowDto
                {
                    UserId = g.Key,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    BookingsCount = g.Count(),
                    ConfirmedHours = g.Sum(b => BookingSchedule.HoursCount(b.StartAt, b.EndAt)),
                    RentalRevenue = g.Sum(b => b.HallRentalPrice),
                    ServicesRevenue = g.Sum(b => b.ServicesPrice),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                };
            })
            .OrderByDescending(r => r.TotalRevenue)
            .ThenByDescending(r => r.BookingsCount)
            .ToList();

        return new UserRevenueReportDto
        {
            From = from,
            To = to,
            Users = rows
        };
    }

    private async Task<List<Booking>> GetConfirmedInPeriodAsync(DateOnly from, DateOnly to)
    {
        var (startUtc, endExclusiveUtc) = BookingSchedule.ToUtcPeriod(from, to);

        var confirmed = await context.Bookings
            .AsNoTracking()
            .Include(b => b.ConferenceHall)
            .Include(b => b.Services)
            .Include(b => b.User)
            .Where(b => b.Status == BookingStatus.Confirmed)
            .ToListAsync();

        return confirmed
            .Where(b => b.StartAt >= startUtc && b.StartAt < endExclusiveUtc)
            .ToList();
    }
}
