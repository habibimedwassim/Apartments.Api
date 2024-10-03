using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apartments.Infrastructure.Database.Configurations;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.HasMany(x => x.ApartmentPhotos)
            .WithOne(x => x.Apartment)
            .HasForeignKey(x => x.ApartmentId);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.Title)
            .HasMaxLength(100);

        builder.Property(x => x.RentAmount)
            .HasPrecision(8, 2);

        builder.Property(a => a.OwnerId)
            .HasMaxLength(50);

        builder.Property(a => a.Street)
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .HasMaxLength(50);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20);

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasDefaultValue("");
    }
}