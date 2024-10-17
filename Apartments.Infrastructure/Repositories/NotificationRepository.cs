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

    public async Task<IEnumerable<Notification>> GetNotificationsAsync(string id)
    {
        return await _dbContext.Notifications
                               .Where(x => x.UserId == id && x.IsRead == false)
                               .OrderByDescending(x => x.CreatedAt)
                               .ToListAsync();
    }

    public async Task MarkAsReadAsync(string userId, string type)
    {
        var notifications = await _dbContext.Notifications
            .Where(x => x.UserId == userId && x.IsRead == false && x.Type.ToLower() == type.ToLower())
            .ToListAsync();

        if (notifications.Any())
        {
            notifications.ForEach(x => x.IsRead = true);

            await _dbContext.SaveChangesAsync();
        }
    }
}
