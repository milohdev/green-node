using GreenNode.Domain.Common;

namespace GreenNode.Domain.Entities;

public class Territory : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Department { get; private set; } = null!;
    public string Municipality { get; private set; } = null!;
    public decimal AvailableHectares { get; private set; }
    public decimal PricePerTree { get; private set; }

    public Territory(string name, string department, string municipality,
        decimal availableHectares, decimal pricePerTree)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Name = name;
        Department = department;
        Municipality = municipality;
        AvailableHectares = availableHectares;
        PricePerTree = pricePerTree;
    }

    protected Territory() { }
}
