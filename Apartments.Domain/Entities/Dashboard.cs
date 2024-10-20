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
