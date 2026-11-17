using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Persistence.Configurations;

public class HotelSettingConfiguration : IEntityTypeConfiguration<HotelSetting>
{
    public void Configure(EntityTypeBuilder<HotelSetting> builder)
    {
        builder.ToTable("hotel_settings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Group)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
