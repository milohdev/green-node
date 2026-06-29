using GreenNode.Domain.Common;

namespace GreenNode.Domain.Entities;

public class Certificate : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string CompanyName { get; private set; } = null!;
    public string TerritoryName { get; private set; } = null!;
    public int NumberOfTrees { get; private set; }
    public decimal Co2EstimatedKg { get; private set; }
    public string PdfUrl { get; private set; } = null!;
    public string VerificationCode { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    public Project Project { get; private set; } = null!;

    public Certificate(Guid projectId, string companyName, string territoryName,
        int numberOfTrees, decimal co2EstimatedKg, string pdfUrl, string verificationCode)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IssuedAt = DateTime.UtcNow;
        ProjectId = projectId;
        CompanyName = companyName;
        TerritoryName = territoryName;
        NumberOfTrees = numberOfTrees;
        Co2EstimatedKg = co2EstimatedKg;
        PdfUrl = pdfUrl;
        VerificationCode = verificationCode;
    }

    protected Certificate() { }
}