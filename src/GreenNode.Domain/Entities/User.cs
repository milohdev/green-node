using GreenNode.Domain.Common;
using GreenNode.Domain.Enums;

namespace GreenNode.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    public User(string name, string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    protected User() { }
}