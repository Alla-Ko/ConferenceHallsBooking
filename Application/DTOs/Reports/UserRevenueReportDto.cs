namespace ConferenceHalls.Application.DTOs.Reports;

public class UserRevenueRowDto
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int BookingsCount { get; set; }

    public int ConfirmedHours { get; set; }

    public decimal RentalRevenue { get; set; }

    public decimal ServicesRevenue { get; set; }

    public decimal TotalRevenue { get; set; }
}

public class UserRevenueReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<UserRevenueRowDto> Users { get; set; } = [];
}
