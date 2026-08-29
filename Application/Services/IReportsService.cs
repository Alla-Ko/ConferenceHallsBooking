using ConferenceHalls.Application.DTOs.Reports;

namespace ConferenceHalls.Application.Services;

public interface IReportsService
{
    Task<HallRevenueReportDto> GetHallRevenueAsync(DateOnly from, DateOnly to);

    Task<ServiceRevenueReportDto> GetServiceRevenueAsync(DateOnly from, DateOnly to);

    Task<HallRentalHoursReportDto> GetRentalHoursAsync(DateOnly from, DateOnly to);

    Task<HallUtilizationReportDto> GetUtilizationAsync(DateOnly from, DateOnly to);

    Task<UserRevenueReportDto> GetUserRevenueAsync(DateOnly from, DateOnly to);
}
