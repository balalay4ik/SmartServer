using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HomeServer.Modules.Devices.Domain.Entities;

namespace HomeServerServer.Modules.Deviecs.Persistence.Configurations;

public class TelemetryConfiguration : IEntityTypeConfiguration<Telemetry>
{
    public void Configure(EntityTypeBuilder<Telemetry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ValueType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Unit)
            .HasMaxLength(20);

        builder.Property(x => x.ConfigurationJson);

        builder.HasOne(x => x.Device)
            .WithMany(x => x.Telemetries)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}