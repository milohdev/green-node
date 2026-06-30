using GreenNode.Application.Common.Interfaces;
using GreenNode.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GreenNode.Application.Features.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request, CancellationToken ct)
    {
        int totalCompanies    = await _context.Companies.CountAsync(ct);
        int pendingQuotes     = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Pending, ct);
        int approvedQuotes    = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Approved, ct);
        int rejectedQuotes    = await _context.Quotes.CountAsync(q => q.Status == QuoteStatus.Rejected, ct);
        int activeProjects    = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Active, ct);
        int completedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed, ct);
        int validatedTrees    = await _context.Projects.SumAsync(p => p.ValidatedTrees, ct);
        int totalEvidences    = await _context.Evidences.CountAsync(ct);

        return new DashboardSummaryDto(
            totalCompanies, pendingQuotes, approvedQuotes,
            rejectedQuotes, activeProjects, completedProjects,
            validatedTrees, totalEvidences);
    }
}
