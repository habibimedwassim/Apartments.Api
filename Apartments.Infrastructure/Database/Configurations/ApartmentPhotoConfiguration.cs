using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apartments.Infrastructure.Database.Configurations;

public class ApartmentPhotoConfiguration : IEntityTypeConfiguration<ApartmentPhoto>
{
    public void Configure(EntityTypeBuilder<ApartmentPhoto> builder)
    {
        builder.Property(a => a.Url)
            .HasMaxLength(2048);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}