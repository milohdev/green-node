namespace GreenNode.Domain.Entities;

public class Territory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Department { get; private set; } = default!;
    public string Municipality { get; private set; } = default!;
    public decimal AvailableHectares { get; private set; }
    public decimal PricePerTree { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Territory(string name, string department, string municipality,
        decimal availableHectares, decimal pricePerTree)
    {
        Id = Guid.NewGuid();
        Name = name;
        Department = department;
        Municipality = municipality;
        AvailableHectares = availableHectares;
        PricePerTree = pricePerTree;
        CreatedAt = DateTime.UtcNow;
    }

    protected Territory() { }
}
