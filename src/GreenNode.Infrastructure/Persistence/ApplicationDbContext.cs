using GreenNode.Application.Common.Interfaces;
using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Evidence> Evidences => Set<Evidence>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
