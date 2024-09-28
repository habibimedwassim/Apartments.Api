using Apartments.Domain.Entities;
using Apartments.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Apartments.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserContext userContext)
    : IdentityDbContext<User>(options)
{
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<ChangeLog> ChangeLogs { get; set; }
    public DbSet<ApartmentRequest> ApartmentRequests { get; set; }
    public DbSet<ApartmentPhoto> ApartmentPhotos { get; set; }
    public DbSet<RentTransaction> RentTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    public IQueryable<T> ApplyIsDeletedFilter<T>(IQueryable<T> query) where T : class
    {
        if (userContext.IsUser())
        {
            return query.Where(x => EF.Property<bool>(x, "IsDeleted") == false);
        }
        return query;
    }
}