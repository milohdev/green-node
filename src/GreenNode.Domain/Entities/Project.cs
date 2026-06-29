using GreenNode.Domain.Enums;
using GreenNode.Domain.Exceptions;

namespace GreenNode.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid QuoteId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid TerritoryId { get; private set; }
    public string Sector { get; private set; } = default!;
    public int NumberOfTrees { get; private set; }
    public int ValidatedTrees { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public ProjectStatus Status { get; private set; }
    public string? CertificateUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Quote Quote { get; private set; } = default!;
    public Company Company { get; private set; } = default!;
    public Territory Territory { get; private set; } = default!;
    public ICollection<Evidence> Evidences { get; private set; } = [];
    public Certificate? Certificate { get; private set; }

    public Project(Guid quoteId, Guid companyId, Guid territoryId, string sector, int numberOfTrees)
    {
        Id = Guid.NewGuid();
        QuoteId = quoteId;
        CompanyId = companyId;
        TerritoryId = territoryId;
        Sector = sector;
        NumberOfTrees = numberOfTrees;
        ValidatedTrees = 0;
        ProgressPercentage = 0;
        Status = ProjectStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    protected Project() { }

    public void UpdateProgress(int additionalValidatedTrees)
    {
        if (additionalValidatedTrees < 0)
            throw new DomainException("Los árboles validados no pueden ser negativos.");
        ValidatedTrees += additionalValidatedTrees;
        ProgressPercentage = NumberOfTrees > 0
            ? Math.Min(100m, (decimal)ValidatedTrees / NumberOfTrees * 100m)
            : 0m;
        if (ProgressPercentage >= 100m)
            Status = ProjectStatus.Completed;
    }

    public void SetCertificateUrl(string url)
    {
        CertificateUrl = url;
    }
}
