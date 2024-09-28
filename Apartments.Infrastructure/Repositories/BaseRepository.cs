using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Apartments.Application.Utilities;
using Apartments.Infrastructure.Database;

namespace Apartments.Infrastructure.Repositories;
public class BaseRepository<T>(ApplicationDbContext dbContext) where T : class
{
    public async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }
    public async Task AddListAsync(IEnumerable<T> entities)
    {
        await dbContext.Set<T>().AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }
    public async Task<T?> GetByIdAsync(object id)
    {
        try
        {
            IQueryable<T> baseQuery = dbContext.Set<T>().AsQueryable();

            return typeof(T) switch
            {
                Type t when t == typeof(Apartment) => await GetApartmentById(baseQuery as IQueryable<Apartment>, (int)id) as T,
                Type t when t == typeof(User) => await GetUserById(baseQuery as IQueryable<User>, id) as T,
                Type t when t == typeof(ApartmentPhoto) => await GetApartmentPhotoById(baseQuery as IQueryable<ApartmentPhoto>, (int)id) as T,
                Type t when t == typeof(ApartmentRequest) => await GetApartmentRequestById(baseQuery as IQueryable<ApartmentRequest>, (int)id) as T,
                Type t when t == typeof(RentTransaction) => await GetRentTransactionById(baseQuery as IQueryable<RentTransaction>, (int)id) as T,
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

        return await query.Include(a => a.Owner)
                          .Include(a => a.ApartmentPhotos)
                          .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async Task<User?> GetUserById(IQueryable<User>? query, object id)
    {
        if (query is null) return null;

        return id switch
        {
            string userId => await query.Include(u => u.CurrentApartment)
                                        .FirstOrDefaultAsync(u => u.Id == userId),
            int sysId => await query.Include(u => u.CurrentApartment)
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

    //public async Task<T?> GetByIdAsync(object id)
    //{
    //    try
    //    {
    //        IQueryable<T> baseQuery = dbContext.Set<T>().AsQueryable();
    //        baseQuery = dbContext.ByPassIsDeletedFilter(baseQuery);

    //        if (typeof(T) == typeof(Apartment))
    //        {
    //            var apartmentQuery = baseQuery as IQueryable<Apartment>;
    //            if (apartmentQuery is null) return null;

    //            return await apartmentQuery.Include(a => a.Owner)
    //                                       .Include(a => a.ApartmentPhotos)
    //                                       .FirstOrDefaultAsync(a => a.Id == (int)id) as T;
    //        }

    //        if (typeof(T) == typeof(User))
    //        {
    //            var userQuery = baseQuery as IQueryable<User>;
    //            if (userQuery is null) return null;

    //            if (id is string userId)
    //            {
    //                return await userQuery.Include(u => u.CurrentApartment)
    //                                      .FirstOrDefaultAsync(u => u.Id == userId) as T;
    //            }

    //            return await userQuery.Include(u => u.CurrentApartment)
    //                                  .FirstOrDefaultAsync(u => u.SysId == (int)id) as T;
    //        }

    //        if(typeof(T) == typeof(ApartmentPhoto))
    //        {
    //            var apartmentPhotoQuery = baseQuery as IQueryable<ApartmentPhoto>;
    //            if (apartmentPhotoQuery is null) return null;

    //            return await apartmentPhotoQuery.FirstOrDefaultAsync(x => x.Id == (int)id) as T;
    //        }
    //        return await dbContext.Set<T>().FindAsync(id);
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}
    public async Task UpdateWithChangeLogsAsync(List<T> originalEntity, List<T> updatedEntity, string userEmail, string[]? additionalPropertiesToExclude = null)
    {
        try
        {
            var changeLogs = CoreUtilities.GenerateChangeLogs(originalEntity, updatedEntity, userEmail, additionalPropertiesToExclude);

            if (changeLogs.Any())
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
    public async Task UpdateWithChangeLogsAsync(T originalEntity, T updatedEntity, string userEmail, string entityId, string[]? additionalPropertiesToExclude = null)
    {
        try
        {
            var changeLogs = CoreUtilities.GenerateChangeLogs(originalEntity, updatedEntity, userEmail, entityId, additionalPropertiesToExclude);

            if (changeLogs.Any())
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

    public async Task DeleteRestoreAsync(T entity, bool newValue, string userEmail, string entityId)
    {
        try
        {
            var oldValue = !(newValue);

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
            {
                isDeletedProperty.SetValue(entity, newValue);
            }
            else
            {
                throw new InvalidOperationException($"The entity {typeof(T).Name} does not have a writable 'IsDeleted' property.");
            }

            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}