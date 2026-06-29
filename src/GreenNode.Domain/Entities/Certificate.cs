namespace GreenNode.Domain.Entities;

public class Certificate
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string CompanyName { get; private set; } = default!;
    public string TerritoryName { get; private set; } = default!;
    public int NumberOfTrees { get; private set; }
    public decimal Co2EstimatedKg { get; private set; }
    public string PdfUrl { get; private set; } = default!;
    public string VerificationCode { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Project Project { get; private set; } = default!;

    public Certificate(Guid projectId, string companyName, string territoryName,
        int numberOfTrees, decimal co2EstimatedKg, string pdfUrl, string verificationCode)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        CompanyName = companyName;
        TerritoryName = territoryName;
        NumberOfTrees = numberOfTrees;
        Co2EstimatedKg = co2EstimatedKg;
        PdfUrl = pdfUrl;
        VerificationCode = verificationCode;
        IssuedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    protected Certificate() { }
}
