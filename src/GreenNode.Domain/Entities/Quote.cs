using GreenNode.Domain.Common;
using GreenNode.Domain.Enums;

namespace GreenNode.Domain.Entities;

public class Quote : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid TerritoryId { get; private set; }
    public string Sector { get; private set; } = null!;
    public int NumberOfTrees { get; private set; }
    public QuoteStatus Status { get; private set; }
    public string? AiProposal { get; private set; }
    public string? RejectionReason { get; private set; }
    public decimal TotalPrice { get; private set; }

    public Company Company { get; private set; } = null!;
    public Territory Territory { get; private set; } = null!;

    public Quote(Guid companyId, Guid territoryId, string sector,
        int numberOfTrees, decimal totalPrice)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        CompanyId = companyId;
        TerritoryId = territoryId;
        Sector = sector;
        NumberOfTrees = numberOfTrees;
        TotalPrice = totalPrice;
        Status = QuoteStatus.Pending;
    }

    protected Quote() { }

    public void Approve() => Status = QuoteStatus.Approved;

    public void Reject(string reason)
    {
        Status = QuoteStatus.Rejected;
        RejectionReason = reason;
    }

    public void SetAiProposal(string proposal) => AiProposal = proposal;
}
