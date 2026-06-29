using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenNode.Infrastructure.Persistence.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Sector)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.NumberOfTrees)
            .IsRequired();

        builder.Property(q => q.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(q => q.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.AiProposal)
            .HasMaxLength(5000);

        builder.Property(q => q.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        builder.HasOne(q => q.Company)
            .WithMany()
            .HasForeignKey(q => q.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Territory)
            .WithMany()
            .HasForeignKey(q => q.TerritoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
