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

        // Calculate the total revenue from paid transactions
        var totalRevenue = await dbContext.RentTransactions
            .Where(rt => rt.OwnerId == ownerId && rt.Status == RequestStatus.Paid)
            .SumAsync(rt => rt.RentAmount);

        // Get the top 5 recent transactions
        var recentTransactions = await dbContext.RentTransactions
            .Where(rt => rt.OwnerId == ownerId)
            .OrderByDescending(rt => rt.CreatedDate)
            .Take(5)
            .ToListAsync();

        // Get the top 5 recent apartment requests (rent and leave)
        var recentRequests = await dbContext.ApartmentRequests
            .Where(ar => ar.OwnerId == ownerId)
            .OrderByDescending(ar => ar.RequestDate)
            .Take(5)
            .ToListAsync();

        // Get the revenue by month
        var revenueByMonth = await dbContext.RentTransactions
            .Where(rt => rt.OwnerId == ownerId && rt.Status == RequestStatus.Paid)
            .GroupBy(rt => new { rt.DateFrom.Year, rt.DateFrom.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month:00}",
                Revenue = g.Sum(rt => rt.RentAmount)
            })
            .ToListAsync();

        // Create and return the OwnerDashboardDetails object
        return new OwnerDashboardDetails
        {
            TotalOwnedApartments = totalOwnedApartments,
            OccupiedApartments = occupiedApartments,
            AvailableApartments = availableApartments,
            TotalTenants = totalTenants,
            TotalRevenue = totalRevenue,
            RecentTransactions = recentTransactions,
            RecentRequests = recentRequests,
            RevenueByMonth = revenueByMonth.Select(r => (r.Month, r.Revenue)).ToList()
        };
    }

}
