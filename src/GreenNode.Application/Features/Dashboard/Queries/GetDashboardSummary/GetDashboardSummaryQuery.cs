using MediatR;

namespace GreenNode.Application.Features.Dashboard.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery() : IRequest<DashboardSummaryDto>;
