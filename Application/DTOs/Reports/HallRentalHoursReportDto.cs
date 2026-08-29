namespace ConferenceHalls.Application.DTOs.Reports;

public class HourBandDto
{
    public int Hours { get; set; }

    public decimal Revenue { get; set; }
}

public class HallRentalHoursRowDto
{
    public int HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public HourBandDto Morning { get; set; } = new();

    public HourBandDto Peak { get; set; } = new();

    public HourBandDto Evening { get; set; } = new();

    public HourBandDto Standard { get; set; } = new();

    public decimal TotalRentalRevenue { get; set; }
}

public class HallRentalHoursReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public List<HallRentalHoursRowDto> Halls { get; set; } = [];
}
