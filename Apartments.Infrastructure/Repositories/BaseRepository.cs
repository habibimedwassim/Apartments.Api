using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Apartments.Application.Utilities;
using Apartments.Infrastructure.Database;

namespace Apartments.Infrastructure.Repositories;

public class BaseRepository<T>(ApplicationDbContext dbContext) where T : class
{
    protected async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    protected async Task AddListAsync(IEnumerable<T> entities)
    {
        await dbContext.Set<T>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    protected async Task<T?> GetByIdAsync(object id)
    {
        try
        {
            var baseQuery = dbContext.Set<T>().AsQueryable();

            return typeof(T) switch
            {
                { } t when t == typeof(Apartment) => await GetApartmentById(baseQuery as IQueryable<Apartment>,
                    (int)id) as T,
                { } t when t == typeof(User) => await GetUserById(baseQuery as IQueryable<User>, id) as T,
                { } t when t == typeof(ApartmentPhoto) => await GetApartmentPhotoById(
                    baseQuery as IQueryable<ApartmentPhoto>, (int)id) as T,
                { } t when t == typeof(ApartmentRequest) => await GetApartmentRequestById(
                    baseQuery as IQueryable<ApartmentRequest>, (int)id) as T,
                { } t when t == typeof(RentTransaction) => await GetRentTransactionById(
                    baseQuery as IQueryable<RentTransaction>, (int)id) as T,
                _ => await dbContext.Set<T>().FindAsync(id)
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<Apartment?> GetApartmentById(IQueryable<Apartment>? query, int id)
    {
        if (query is null) return null;

        return await query
            .Include(a => a.Owner)
            .Include(a => a.Tenant)
            .Include(a => a.ApartmentPhotos)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async Task<User?> GetUserById(IQueryable<User>? query, object id)
    {
        if (query is null) return null;

        return id switch
        {
            string userId => await query
                .FirstOrDefaultAsync(u => u.Id == userId),
            int sysId => await query
                .FirstOrDefaultAsync(u => u.SysId == sysId),
            _ => null
        };
    }

    private async Task<ApartmentPhoto?> GetApartmentPhotoById(IQueryable<ApartmentPhoto>? query, int id)
    {
        if (query is null) return null;

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    private async Task<ApartmentRequest?> GetApartmentRequestById(IQueryable<ApartmentRequest>? query, int id)
    {
        if (query is null) return null;

        return await query.Include(x => x.Apartment)
            .Include(x => x.Tenant)
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    private async Task<RentTransaction?> GetRentTransactionById(IQueryable<RentTransaction>? query, int id)
    {
        if (query is null) return null;

        return await query.Include(x => x.Apartment)
            .Include(x => x.Tenant)
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    protected async Task UpdateWithChangeLogsAsync(List<T> originalEntity, List<T> updatedEntity, string userEmail,
        string[]? additionalPropertiesToExclude = null)
    {
        try
        {
            var changeLogs = CoreUtilities.GenerateChangeLogs(originalEntity, updatedEntity, userEmail,
                additionalPropertiesToExclude);

            if (changeLogs.Count > 0)
            {
                await dbContext.ChangeLogs.AddRangeAsync(changeLogs);
                dbContext.Update(updatedEntity);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected async Task UpdateWithChangeLogsAsync(T originalEntity, T updatedEntity, string userEmail, string entityId,
        string[]? additionalPropertiesToExclude = null)
    {
        try
        {
            var changeLogs = CoreUtilities.GenerateChangeLogs(originalEntity, updatedEntity, userEmail, entityId,
                additionalPropertiesToExclude);

            if (changeLogs.Count > 0)
            {
                await dbContext.ChangeLogs.AddRangeAsync(changeLogs);
                dbContext.Update(updatedEntity);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected async Task DeleteRestoreAsync(T entity, bool newValue, string userEmail, string entityId)
    {
        try
        {
            var oldValue = !newValue;

            var changeLog = new ChangeLog
            {
                EntityType = typeof(T).Name,
                PropertyId = entityId,
                PropertyName = "IsDeleted",
                OldValue = oldValue.ToString(),
                NewValue = newValue.ToString(),
                ChangedBy = userEmail,
                ChangedAt = DateTime.UtcNow
            };

            await dbContext.ChangeLogs.AddAsync(changeLog);

            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");

            if (isDeletedProperty != null && isDeletedProperty.CanWrite)
                isDeletedProperty.SetValue(entity, newValue);
            else
                throw new InvalidOperationException(
                    $"The entity {typeof(T).Name} does not have a writable 'IsDeleted' property.");

            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}