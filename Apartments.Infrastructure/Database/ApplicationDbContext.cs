using Apartments.Domain.Entities;
using Apartments.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Apartments.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserContext userContext)
    : IdentityDbContext<User>(options)
{
    public DbSet<Apartment> Apartments { get; init; }
    public DbSet<ChangeLog> ChangeLogs { get; init; }
    public DbSet<ApartmentRequest> ApartmentRequests { get; init; }
    public DbSet<ApartmentPhoto> ApartmentPhotos { get; init; }
    public DbSet<RentTransaction> RentTransactions { get; init; }
    public DbSet<Notification> Notifications { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public IQueryable<T> ApplyIsDeletedFilter<T>(IQueryable<T> query) where T : class
    {
        return userContext.IsUser() ? query.Where(x => EF.Property<bool>(x, "IsDeleted") == false) : query;
    }
}