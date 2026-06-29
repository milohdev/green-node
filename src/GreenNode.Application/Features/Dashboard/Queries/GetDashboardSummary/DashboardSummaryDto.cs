namespace GreenNode.Application.Features.Dashboard.Queries.GetDashboardSummary;

public record DashboardSummaryDto(
    int TotalCompanies,
    int PendingQuotes,
    int ApprovedQuotes,
    int RejectedQuotes,
    int ActiveProjects,
    int CompletedProjects,
    int TotalValidatedTrees,
    int TotalEvidences
);
