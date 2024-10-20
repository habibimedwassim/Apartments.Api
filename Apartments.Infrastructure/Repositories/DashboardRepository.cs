using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class DashboardRepository(ApplicationDbContext dbContext) : IDashboardRepository
{
    public async Task<OwnerDashboardDetails> GetOwnerDashboardDetailsAsync(string ownerId)
    {
        // Count the total number of owned apartments
        var totalOwnedApartments = await dbContext.Apartments
            .CountAsync(a => a.OwnerId == ownerId);

        // Count the number of occupied apartments
        var occupiedApartments = await dbContext.Apartments
            .CountAsync(a => a.OwnerId == ownerId && a.IsOccupied);

        // Calculate available apartments
        var availableApartments = totalOwnedApartments - occupiedApartments;

        // Count distinct tenants who have rented from the owner
        var totalTenants = await dbContext.Apartments
            .CountAsync(x => x.OwnerId == ownerId && x.TenantId != null);

        // Calculate the total revenue from transactions that are Paid or Late
        var totalRevenue = await dbContext.RentTransactions
            .Where(rt => rt.OwnerId == ownerId &&
                         (rt.Status == RequestStatus.Paid || rt.Status == RequestStatus.Late))
            .SumAsync(rt => rt.RentAmount);

        // Get the top 5 recent rent requests
        var recentRentRequests = await dbContext.ApartmentRequests
                .Where(ar => ar.OwnerId == ownerId && ar.RequestType == ApartmentRequestType.Rent.ToString())
                .OrderByDescending(ar => ar.CreatedDate)
                .Take(5)
                .ToListAsync();

        // Get the top 5 recent leave requests
        var recentLeaveRequests = await dbContext.ApartmentRequests
            .Where(ar => ar.OwnerId == ownerId && ar.RequestType == ApartmentRequestType.Leave.ToString())
            .OrderByDescending(ar => ar.CreatedDate)
            .Take(5)
            .ToListAsync();

        // Get the top 5 recent dismiss requests
        var recentDismissRequests = await dbContext.ApartmentRequests
            .Where(ar => ar.OwnerId == ownerId && ar.RequestType == ApartmentRequestType.Dismiss.ToString())
            .OrderByDescending(ar => ar.CreatedDate)
            .Take(5)
            .ToListAsync();

        // Get the top 5 recent transactions with status Paid or Late
        var recentTransactions = await dbContext.RentTransactions
            .Where(rt => rt.OwnerId == ownerId)
            .OrderByDescending(rt => rt.CreatedDate)
            .Take(5)
            .ToListAsync();

        // Get the revenue by month for this year
        var currentYear = DateTime.Now.Year;
        var revenueByMonth = Enumerable.Range(1, 12).Select(month => new
        {
            Month = $"{currentYear}-{month:00}",
            Revenue = dbContext.RentTransactions
                .Where(rt => rt.OwnerId == ownerId &&
                             (rt.Status == RequestStatus.Paid || rt.Status == RequestStatus.Late) &&
                             rt.DateFrom.Year == currentYear && rt.DateFrom.Month == month)
                .Sum(rt => rt.RentAmount)
        }).ToList();

        // Create and return the OwnerDashboardDetails object
        return new OwnerDashboardDetails
        {
            TotalOwnedApartments = totalOwnedApartments,
            OccupiedApartments = occupiedApartments,
            AvailableApartments = availableApartments,
            TotalTenants = totalTenants,
            TotalRevenue = totalRevenue,
            RecentRentRequests = recentRentRequests,
            RecentLeaveRequests = recentLeaveRequests,
            RecentDismissRequests = recentDismissRequests,
            RecentTransactions = recentTransactions,
            RevenueByMonth = revenueByMonth.Select(r => (r.Month, r.Revenue)).ToList()
        };
    }


}
