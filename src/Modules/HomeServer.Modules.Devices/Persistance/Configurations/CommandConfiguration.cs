using HomeServer.Modules.Devices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServerServer.Modules.Deviecs.Persistence.Configurations;

public class CommandConfiguration : IEntityTypeConfiguration<Command>
{
    public void Configure(EntityTypeBuilder<Command> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ControlType)
            .IsRequired();

        builder.Property(x => x.ValueType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ConfigurationJson);

        builder.HasOne(x => x.Device)
            .WithMany(x => x.Commands)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}