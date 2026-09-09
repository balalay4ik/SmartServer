using HomeServer.Modules.Devices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServerServer.Modules.Deviecs.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ExternalId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Transport)
            .IsRequired();

        builder.Property(x => x.Adapter)
            .IsRequired();

        builder.HasIndex(x => x.ExternalId)
            .IsUnique();

        builder.HasMany(x => x.Commands)
            .WithOne(x => x.Device)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Telemetries)
            .WithOne(x => x.Device)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Info)
            .WithOne(x => x.Device)
            .HasForeignKey<DeviceInfo>(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}