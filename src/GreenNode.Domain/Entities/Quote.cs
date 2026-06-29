using GreenNode.Domain.Enums;
using GreenNode.Domain.Exceptions;

namespace GreenNode.Domain.Entities;

public class Quote
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid TerritoryId { get; private set; }
    public string Sector { get; private set; } = default!;
    public int NumberOfTrees { get; private set; }
    public QuoteStatus Status { get; private set; }
    public string? AiProposal { get; private set; }
    public string? RejectionReason { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Company Company { get; private set; } = default!;
    public Territory Territory { get; private set; } = default!;

    public Quote(Guid companyId, Guid territoryId, string sector, int numberOfTrees, decimal totalPrice)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        TerritoryId = territoryId;
        Sector = sector;
        NumberOfTrees = numberOfTrees;
        TotalPrice = totalPrice;
        Status = QuoteStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    protected Quote() { }

    public void Approve()
    {
        if (Status != QuoteStatus.Pending)
            throw new DomainException("Solo se puede aprobar una cotización en estado Pending.");
        Status = QuoteStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != QuoteStatus.Pending)
            throw new DomainException("Solo se puede rechazar una cotización en estado Pending.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("El motivo de rechazo es obligatorio.");
        Status = QuoteStatus.Rejected;
        RejectionReason = reason;
    }

    public void SetAiProposal(string proposal)
    {
        AiProposal = proposal;
    }
}
