using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.EntityName);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.CreatedAt);

        builder.Property(x => x.OldValues)
            .HasMaxLength(4000);

        builder.Property(x => x.NewValues)
            .HasMaxLength(4000);

        builder.Property(x => x.ChangedColumns)
            .HasMaxLength(1000);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(250);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}
