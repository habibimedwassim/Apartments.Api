using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class NotificationRepository(ApplicationDbContext dbContext)
    : BaseRepository<Notification>(dbContext), INotificationRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
        return await AddAsync(notification);
    }
    public async Task AddNotificationListAsync(List<Notification> notifications)
    {
        await _dbContext.Notifications.AddRangeAsync(notifications);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddOrUpdateDeviceTokenAsync(string userId, string deviceToken)
    {
        var existingToken = await _dbContext.UserDeviceTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceToken == deviceToken);

        if (existingToken == null)
        {
            _dbContext.UserDeviceTokens.Add(new UserDeviceToken
            {
                UserId = userId,
                DeviceToken = deviceToken
            });
        }
        else
        {
            existingToken.LastUpdated = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetAllUnreadNotificationsAsync(string id)
    {
        return await _dbContext.Notifications
                               .Where(x => x.UserId == id && x.IsRead == false)
                               .OrderByDescending(x => x.CreatedAt)
                               .ToListAsync();
    }
    public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string userId, string type)
    {
        return await _dbContext.Notifications
                               .Where(x => x.UserId == userId && x.Type == type)
                               .ToListAsync();
    }
    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.IsRead == false);

        if (notification != null)
        {
            notification.IsRead = true;

            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task MarkAsReadAsync(string userId, string type)
    {
        var notifications = await _dbContext.Notifications
            .Where(x => x.UserId == userId && x.IsRead == false && x.Type == type)
            .ToListAsync();

        if (notifications.Any())
        {
            notifications.ForEach(x => x.IsRead = true);

            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task<List<string>> GetDeviceTokensByUserIdAsync(string userId)
    {
        return await _dbContext.UserDeviceTokens
            .Where(x => x.UserId == userId)
            .Select(x => x.DeviceToken)
            .ToListAsync();
    }

    public async Task<PagedModel<Notification>> GetNotificationsPagedAsync(int pageNumber, string id)
    {
        // Start with the base query
        var baseQuery = _dbContext.Notifications
                                .Where(x => x.UserId == id)
                                .OrderBy(x => x.IsRead)
                                .ThenByDescending(x => x.CreatedAt)
                                .AsQueryable();

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply pagination
        var apartments = await baseQuery
            .Skip(AppConstants.PageSize * (pageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<Notification> { Data = apartments, DataCount = totalCount };
    }

    public async Task<int> GetUnreadNotificationsCountAsync(string id)
    {
        return await _dbContext.Notifications.CountAsync(x => x.UserId == id && x.IsRead == false);
    }
}
