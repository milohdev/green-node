using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenNode.Infrastructure.Persistence.Configurations;

public class TerritoryConfiguration : IEntityTypeConfiguration<Territory>
{
    public void Configure(EntityTypeBuilder<Territory> builder)
    {
        builder.ToTable("Territories");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Territories_Name");

        builder.Property(t => t.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Municipality)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.AvailableHectares)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(t => t.PricePerTree)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(t => t.CreatedAt)
            .IsRequired();
    }
}
