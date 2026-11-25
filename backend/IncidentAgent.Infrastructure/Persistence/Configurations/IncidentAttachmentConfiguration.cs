using IncidentAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentAgent.Infrastructure.Persistence.Configurations;

public class IncidentAttachmentConfiguration : IEntityTypeConfiguration<IncidentAttachment>
{
    public void Configure(EntityTypeBuilder<IncidentAttachment> builder)
    {
        builder.ToTable("incident_attachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.SizeBytes).IsRequired();
        builder.Property(a => a.UploadedAtUtc).IsRequired();

        builder.HasOne(a => a.Incident)
            .WithMany(i => i.Attachments)
            .HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
