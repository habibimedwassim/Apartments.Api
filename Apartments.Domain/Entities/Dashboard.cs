namespace Apartments.Domain.Entities;

public class OwnerDashboardDetails
{
    public int TotalOwnedApartments { get; set; }
    public int OccupiedApartments { get; set; }
    public int AvailableApartments { get; set; }
    public int TotalTenants { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RentTransaction> RecentTransactions { get; set; } = [];
    public List<ApartmentRequest> RecentRentRequests { get; set; } = [];
    public List<ApartmentRequest> RecentLeaveRequests { get; set; } = [];
    public List<ApartmentRequest> RecentDismissRequests { get; set; } = [];
    public List<(string Month, decimal Revenue)> RevenueByMonth { get; set; } = [];
}

public class AdminDashboardDetails
{
    public int TotalUsers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public List<ReportsByMonth> ReportsByMonth { get; set; } = [];
    public List<UserReport> RecentReports { get; set; } = [];
    public List<ChangeLog> RecentChangeLogs { get; set; } = [];
}

public class ReportsByMonth
{
    public string Month { get; set; } = string.Empty;
    public int Reports { get; set; }
}