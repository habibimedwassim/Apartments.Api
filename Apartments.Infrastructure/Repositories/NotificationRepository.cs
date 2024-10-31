using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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
}
