using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IncidentNumber).HasMaxLength(20).IsRequired();
        builder.Property(i => i.Title).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(4000).IsRequired();
        builder.Property(i => i.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.UpdatedAtUtc).IsRequired();

        builder.HasIndex(i => i.IncidentNumber).IsUnique();
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Priority);
        builder.HasIndex(i => i.ServiceId);
        builder.HasIndex(i => i.CreatedAtUtc);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Incidents)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Service)
            .WithMany(s => s.Incidents)
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Reporter)
            .WithMany(u => u.ReportedIncidents)
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AssignedTo)
            .WithMany(u => u.AssignedIncidents)
            .HasForeignKey(i => i.AssignedToId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
