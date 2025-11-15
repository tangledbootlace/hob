using MediatR;

namespace HOB.API.Dashboard.GetDashboardSummary;

public record GetDashboardSummaryRequest() : IRequest<GetDashboardSummaryResponse>;
