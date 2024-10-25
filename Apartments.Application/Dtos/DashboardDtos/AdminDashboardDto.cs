using Apartments.Domain.Entities;

namespace Apartments.Application.Dtos.DashboardDtos;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public List<ReportsByMonthDto> ReportsByMonth { get; set; } = [];
    public List<RecentReportDto> RecentReports { get; set; } = [];
    public List<ChangeLog> RecentChangeLogs { get; set; } = [];
}

public class RecentReportDto
{
    public int Id { get; set; }
    public string Message { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
}

public class ReportsByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public int Reports { get; set; }
}