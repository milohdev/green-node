using GreenNode.Domain.Common;
using GreenNode.Domain.Enums;

namespace GreenNode.Domain.Entities;

public class Project : BaseEntity
{
    public Guid QuoteId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid TerritoryId { get; private set; }
    public string Sector { get; private set; } = null!;
    public int NumberOfTrees { get; private set; }
    public int ValidatedTrees { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public ProjectStatus Status { get; private set; }
    public string? CertificateUrl { get; private set; }

    public Quote Quote { get; private set; } = null!;
    public Company Company { get; private set; } = null!;
    public Territory Territory { get; private set; } = null!;
    public ICollection<Evidence> Evidences { get; private set; } = [];
    public Certificate? Certificate { get; private set; }

    public Project(Guid quoteId, Guid companyId, Guid territoryId,
        string sector, int numberOfTrees)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        QuoteId = quoteId;
        CompanyId = companyId;
        TerritoryId = territoryId;
        Sector = sector;
        NumberOfTrees = numberOfTrees;
        ValidatedTrees = 0;
        ProgressPercentage = 0;
        Status = ProjectStatus.Active;
    }

    protected Project() { }

    public void UpdateProgress(int additionalValidatedTrees)
    {
        ValidatedTrees += additionalValidatedTrees;
        ProgressPercentage = NumberOfTrees > 0
            ? Math.Min((decimal)ValidatedTrees / NumberOfTrees * 100, 100)
            : 0;
        if (ProgressPercentage >= 100)
            Status = ProjectStatus.Completed;
    }

    public void SetCertificateUrl(string url) => CertificateUrl = url;
}
