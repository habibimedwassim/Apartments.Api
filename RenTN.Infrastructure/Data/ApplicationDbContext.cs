using RenTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RenTN.Infrastructure.Data;

internal class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    internal DbSet<Apartment> Apartments { get; set; }
    internal DbSet<ApartmentPhoto> ApartmentPhotos { get; set; }
    internal DbSet<Tenant> Tenants { get; set; }
    internal DbSet<RentHistory> RentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Apartment>()
                    .OwnsOne(x => x.Location);

        modelBuilder.Entity<Apartment>()
                    .HasMany(x => x.ApartmentPhotos)
                    .WithOne(x => x.Apartment)
                    .HasForeignKey(x => x.ApartmentID);

        modelBuilder.Entity<User>()
                    .HasMany(x => x.OwnedApartments)
                    .WithOne(x => x.Owner)
                    .HasForeignKey(x => x.OwnerID);

        modelBuilder.Entity<Tenant>()
                    .HasOne(t => t.Apartment)
                    .WithMany()
                    .HasForeignKey(t => t.ApartmentID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tenant>()
                    .HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentHistory>()
                    .HasOne(rh => rh.Tenant)
                    .WithMany(t => t.RentHistory)
                    .HasForeignKey(rh => rh.TenantID)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentHistory>()
                    .HasOne(rh => rh.Apartment)
                    .WithMany()
                    .HasForeignKey(rh => rh.ApartmentID)
                    .OnDelete(DeleteBehavior.Restrict);
    }
}
