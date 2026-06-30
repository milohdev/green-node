using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenNode.Infrastructure.Persistence.Configurations;

public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable("Evidences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PhotoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.AiValidated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.AiConfidence)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.AiObservations)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Relación configurada desde ProjectConfiguration (Cascade)
    }
}
