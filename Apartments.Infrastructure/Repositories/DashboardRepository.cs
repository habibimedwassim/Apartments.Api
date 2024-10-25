using Apartments.Application.Dtos.DashboardDtos;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
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
    public async Task<AdminDashboardDetails> GetAdminDashboardDetailsAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var past30Days = DateTime.UtcNow.AddDays(-30);

        // Get the total number of non-admin users
        var totalUsers = await dbContext.Users
            .CountAsync(u => u.Role != UserRoles.Admin && !u.IsDeleted);

        // Get the total number of owners
        var totalOwners = await dbContext.Users
            .CountAsync(u => u.Role == UserRoles.Owner && !u.IsDeleted);

        // Get the total number of tenants
        var totalTenants = await dbContext.Apartments
            .CountAsync(a => a.IsOccupied && a.TenantId != null && !a.IsDeleted);

        // Get the total number of active users in the last 30 days (excluding admins)
        var activeUsersLast30Days = await dbContext.Users
            .CountAsync(u => u.LastLoginDate >= past30Days && u.Role != UserRoles.Admin && !u.IsDeleted);

        // Get the recent 5 user reports
        var recentReports = await dbContext.UserReports
            .Where(x => x.TargetRole == UserRoles.Admin)
            .OrderByDescending(r => r.CreatedDate)
            .Take(5)
            .ToListAsync();

        // Get the recent 5 change logs
        var recentChangeLogs = await dbContext.ChangeLogs
            .OrderByDescending(cl => cl.ChangedAt)
            .Take(5)
            .ToListAsync();

        // Generate a list for reports by month for the current year
        var reportsByMonth = Enumerable.Range(1, 12).Select(month => new ReportsByMonth
        {
            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
            Reports = dbContext.UserReports
                .Count(rt => rt.CreatedDate.Year == currentYear &&
                             rt.TargetRole == UserRoles.Admin &&
                             rt.CreatedDate.Month == month)
        }).ToList();

        // Create and return the AdminDashboardDetails object
        return new AdminDashboardDetails
        {
            TotalUsers = totalUsers,
            TotalOwners = totalOwners,
            TotalTenants = totalTenants,
            ActiveUsersLast30Days = activeUsersLast30Days,
            ReportsByMonth = reportsByMonth,
            RecentReports = recentReports,
            RecentChangeLogs = recentChangeLogs
        };
    }
}
