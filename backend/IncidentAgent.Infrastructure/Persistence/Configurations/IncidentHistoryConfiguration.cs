using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class IncidentHistoryConfiguration : IEntityTypeConfiguration<IncidentHistory>
{
    public void Configure(EntityTypeBuilder<IncidentHistory> builder)
    {
        builder.ToTable("incident_history");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(h => h.OldValue).HasMaxLength(500);
        builder.Property(h => h.NewValue).HasMaxLength(500);
        builder.Property(h => h.Note).HasMaxLength(1000);
        builder.Property(h => h.ChangedAtUtc).IsRequired();

        builder.HasIndex(h => new { h.IncidentId, h.ChangedAtUtc });

        builder.HasOne(h => h.Incident)
            .WithMany(i => i.History)
            .HasForeignKey(h => h.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ActorUser)
            .WithMany()
            .HasForeignKey(h => h.ActorUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
