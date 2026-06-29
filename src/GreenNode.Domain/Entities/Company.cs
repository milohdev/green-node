using GreenNode.Domain.Common;

namespace GreenNode.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string Sector { get; private set; } = null!;
    public Guid UserId { get; private set; }

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

    protected Company() { }
}
