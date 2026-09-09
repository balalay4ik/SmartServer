using HomeServer.Modules.Devices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServerServer.Modules.Deviecs.Persistence.Configurations;

public class DeviceInfoConfiguration : IEntityTypeConfiguration<DeviceInfo>
{
    public void Configure(EntityTypeBuilder<DeviceInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConfigurationJson);
    }
}