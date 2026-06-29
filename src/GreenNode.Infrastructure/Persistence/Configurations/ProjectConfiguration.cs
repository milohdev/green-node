using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenNode.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sector)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.NumberOfTrees)
            .IsRequired();

        builder.Property(p => p.ValidatedTrees)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ProgressPercentage)
            .IsRequired()
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0m);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.CertificateUrl)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.Quote)
            .WithOne()
            .HasForeignKey<Project>(p => p.QuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.QuoteId)
            .IsUnique()
            .HasDatabaseName("UQ_Projects_QuoteId");

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Territory)
            .WithMany()
            .HasForeignKey(p => p.TerritoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Evidences)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Certificate)
            .WithOne(c => c.Project)
            .HasForeignKey<Certificate>(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
