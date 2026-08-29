namespace ConferenceHalls.Application.DTOs.Reports;

public class HallRevenueRowDto
{
    public int HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public decimal RentalRevenue { get; set; }

    public decimal ServicesRevenue { get; set; }

    public decimal TotalRevenue { get; set; }
}

public class HallRevenueReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<HallRevenueRowDto> Halls { get; set; } = [];

    public decimal TotalRentalRevenue { get; set; }

    public decimal TotalServicesRevenue { get; set; }

    public decimal TotalRevenue { get; set; }
}
