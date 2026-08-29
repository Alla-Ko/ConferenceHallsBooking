namespace ConferenceHalls.Application.DTOs.Reports;

public class ServiceRevenueRowDto
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TimesBooked { get; set; }

    public decimal Revenue { get; set; }
}

public class ServiceRevenueReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<ServiceRevenueRowDto> Services { get; set; } = [];

    public decimal TotalRevenue { get; set; }
}
