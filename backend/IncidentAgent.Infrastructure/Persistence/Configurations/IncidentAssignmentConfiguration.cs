using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class IncidentAssignmentConfiguration : IEntityTypeConfiguration<IncidentAssignment>
{
    public void Configure(EntityTypeBuilder<IncidentAssignment> builder)
    {
        builder.ToTable("incident_assignments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssignedAtUtc).IsRequired();

        builder.HasOne(a => a.Incident)
            .WithMany(i => i.Assignments)
            .HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Assignee)
            .WithMany()
            .HasForeignKey(a => a.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedBy)
            .WithMany()
            .HasForeignKey(a => a.AssignedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
