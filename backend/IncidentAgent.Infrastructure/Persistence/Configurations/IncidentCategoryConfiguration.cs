using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class IncidentCategoryConfiguration : IEntityTypeConfiguration<IncidentCategory>
{
    public void Configure(EntityTypeBuilder<IncidentCategory> builder)
    {
        builder.ToTable("incident_categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
