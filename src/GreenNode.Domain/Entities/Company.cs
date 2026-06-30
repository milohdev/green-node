using GreenNode.Domain.Common;

namespace GreenNode.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Sector { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Company(string name, string email, string phone, string sector, Guid userId)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Name = name;
        Email = email;
        Phone = phone;
        Sector = sector;
        UserId = userId;
    }

    // Implementado por Juan como fallback temporal — Cami debe revisar antes de merge a develop
    public void Update(string name, string phone, string sector)
    {
        Name = name;
        Phone = phone;
        Sector = sector;
    }

    protected Company() { }
}