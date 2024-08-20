using RenTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RenTN.Infrastructure.Data;

internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    internal DbSet<Apartment> Apartments { get; set; }
    internal DbSet<ChangeLog> ChangeLogs { get; set; }
    internal DbSet<RentHistory> RentHistories { get; set; }
    internal DbSet<RentalRequest> RentalRequests { get; set; }
    internal DbSet<ApartmentPhoto> ApartmentPhotos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Apartment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RentHistory>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RentalRequest>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<ApartmentPhoto>().HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<User>()
                    .Property(u => u.SysID)
                    .ValueGeneratedOnAdd()
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        modelBuilder.Entity<User>()
                    .HasIndex(u => u.SysID)
                    .IsUnique();

        modelBuilder.Entity<Apartment>()
                    .HasMany(x => x.ApartmentPhotos)
                    .WithOne(x => x.Apartment)
                    .HasForeignKey(x => x.ApartmentID);

        modelBuilder.Entity<Apartment>()
                    .HasOne(x => x.Owner)
                    .WithMany()
                    .HasForeignKey(x => x.OwnerID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalRequest>()
                    .HasOne(x => x.Tenant)
                    .WithMany()
                    .HasForeignKey(x => x.TenantID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalRequest>()
                    .HasOne(x => x.Apartment)
                    .WithMany()
                    .HasForeignKey(x => x.ApartmentID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentHistory>()
                    .HasOne(x => x.Tenant)
                    .WithMany()
                    .HasForeignKey(x => x.TenantID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentHistory>()
                    .HasOne(x => x.Apartment)
                    .WithMany()
                    .HasForeignKey(x => x.ApartmentID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Apartment>()
                    .Property(x => x.Price)
                    .HasPrecision(8, 2);

        modelBuilder.Entity<RentHistory>()
                    .Property(x => x.Amount)
                    .HasPrecision(8, 2);
    }
}
