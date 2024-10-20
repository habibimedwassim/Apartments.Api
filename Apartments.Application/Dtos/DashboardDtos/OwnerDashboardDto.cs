using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.RentTransactionDtos;

namespace Apartments.Application.Dtos.DashboardDtos;

public class OwnerDashboardDto
{
    public int TotalOwnedApartments { get; set; }
    public int OccupiedApartments { get; set; }
    public int AvailableApartments { get; set; }
    public int TotalTenants { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RentTransactionDto> RecentTransactions { get; set; } = [];
    public List<ApartmentRequestDto> RecentRentRequests { get; set; } = [];
    public List<ApartmentRequestDto> RecentLeaveRequests { get; set; } = [];
    public List<ApartmentRequestDto> RecentDismissRequests { get; set; } = [];
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = [];
}

public class RevenueByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}
