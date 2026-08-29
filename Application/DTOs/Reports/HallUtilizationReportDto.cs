namespace ConferenceHalls.Application.DTOs.Reports;

public class HallUtilizationRowDto
{
    public int HallId { get; set; }

    public string HallName { get; set; } = string.Empty;

    public int ConfirmedHours { get; set; }

    public int AvailableHours { get; set; }

    public decimal OccupancyPercent { get; set; }
}

public class HallUtilizationReportDto
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public int DaysInPeriod { get; set; }

    public int BookableHoursPerDay { get; set; }

    public List<HallUtilizationRowDto> Halls { get; set; } = [];
}
