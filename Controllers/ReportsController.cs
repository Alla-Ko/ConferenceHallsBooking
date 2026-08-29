using ConferenceHalls.Application.DTOs.Reports;
using ConferenceHalls.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHalls.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class ReportsController(IReportsService reportsService) : ControllerBase
{
    [HttpGet("revenue/halls")]
    public Task<ActionResult<HallRevenueReportDto>> GetHallRevenue(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to) =>
        GetReportAsync(from, to, reportsService.GetHallRevenueAsync);

    [HttpGet("revenue/services")]
    public Task<ActionResult<ServiceRevenueReportDto>> GetServiceRevenue(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to) =>
        GetReportAsync(from, to, reportsService.GetServiceRevenueAsync);

    [HttpGet("revenue/hours")]
    public Task<ActionResult<HallRentalHoursReportDto>> GetRentalHours(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to) =>
        GetReportAsync(from, to, reportsService.GetRentalHoursAsync);

    [HttpGet("utilization")]
    public Task<ActionResult<HallUtilizationReportDto>> GetUtilization(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to) =>
        GetReportAsync(from, to, reportsService.GetUtilizationAsync);

    [HttpGet("users")]
    public Task<ActionResult<UserRevenueReportDto>> GetUserRevenue(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to) =>
        GetReportAsync(from, to, reportsService.GetUserRevenueAsync);

    private async Task<ActionResult<T>> GetReportAsync<T>(
        DateOnly from,
        DateOnly to,
        Func<DateOnly, DateOnly, Task<T>> getReport)
    {
        try
        {
            return Ok(await getReport(from, to));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
