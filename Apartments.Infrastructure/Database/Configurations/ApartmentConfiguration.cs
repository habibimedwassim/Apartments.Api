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

        builder.HasOne(x => x.Owner)
               .WithMany()
               .HasForeignKey(x => x.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RentAmount)
               .HasPrecision(8, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
