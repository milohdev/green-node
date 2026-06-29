namespace GreenNode.Domain.Entities;

public class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string Sector { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = default!;

    public Company(string name, string email, string phone, string sector, Guid userId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Sector = sector;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    protected Company() { }
}
