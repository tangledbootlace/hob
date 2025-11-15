using MediatR;

namespace HOB.API.Reports.GenerateReport;

public record GenerateReportRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? RequestedBy = null
) : IRequest<GenerateReportResponse>;
