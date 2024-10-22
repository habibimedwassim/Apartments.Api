using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Database.Configurations;

public class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
{
    public void Configure(EntityTypeBuilder<UserReport> builder)
    {
        // Configure the ReporterId and TargetId properties
        builder.Property(ur => ur.ReporterId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ur => ur.TargetId)
            .HasMaxLength(50);

        // Configure the TargetRole property
        builder.Property(ur => ur.TargetRole)
            .HasMaxLength(10)
            .IsRequired();

        // Configure the Message property
        builder.Property(ur => ur.Message)
            .IsRequired();

        // Configure the Status property
        builder.Property(ur => ur.Status)
            .HasMaxLength(15)
            .IsRequired();

        // Configure the AttachmentUrl property
        builder.Property(ur => ur.AttachmentUrl)
            .HasMaxLength(2048);

        // Configure the Comments property
        builder.Property(ur => ur.Comments)
            .HasMaxLength(500);

        // Configure the relationships with the User entity
        builder.HasOne(ur => ur.Reporter)
            .WithMany()
            .HasForeignKey(ur => ur.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Target)
            .WithMany()
            .HasForeignKey(ur => ur.TargetId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure CreatedDate and ResolvedDate
        builder.Property(ur => ur.CreatedDate)
            .IsRequired();

        builder.Property(ur => ur.ResolvedDate)
            .IsRequired(false);
    }
}
