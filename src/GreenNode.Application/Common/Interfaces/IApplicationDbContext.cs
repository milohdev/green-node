using GreenNode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Company> Companies { get; }
    DbSet<Territory> Territories { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<Project> Projects { get; }
    DbSet<Evidence> Evidences { get; }
    DbSet<Certificate> Certificates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
