using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenNode.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TerritoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.NumberOfTrees)
            .IsRequired();

        builder.Property(c => c.Co2EstimatedKg)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(c => c.PdfUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.VerificationCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.VerificationCode)
            .IsUnique()
            .HasDatabaseName("UQ_Certificates_VerificationCode");

        builder.HasIndex(c => c.ProjectId)
            .IsUnique()
            .HasDatabaseName("UQ_Certificates_ProjectId");

        builder.Property(c => c.IssuedAt)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Relación configurada desde ProjectConfiguration
    }
}
