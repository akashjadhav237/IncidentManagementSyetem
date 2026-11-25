using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class ApplicationServiceConfiguration : IEntityTypeConfiguration<ApplicationService>
{
    public void Configure(EntityTypeBuilder<ApplicationService> builder)
    {
        builder.ToTable("application_services");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.OwnerTeam).HasMaxLength(100);
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasIndex(s => s.Name).IsUnique();
    }
}
