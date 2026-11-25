using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("sla_policies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ResponseTimeMinutes).IsRequired();
        builder.Property(p => p.ResolutionTimeMinutes).IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        builder.HasIndex(p => p.Priority).IsUnique();
    }
}
