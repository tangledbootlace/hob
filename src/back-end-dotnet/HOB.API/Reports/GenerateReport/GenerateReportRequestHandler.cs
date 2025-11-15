using HOB.Common.Library.Messages;
using MassTransit;
using MediatR;

namespace HOB.API.Reports.GenerateReport;

public class GenerateReportRequestHandler : IRequestHandler<GenerateReportRequest, GenerateReportResponse>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerateReportRequestHandler> _logger;

    public GenerateReportRequestHandler(IPublishEndpoint publishEndpoint, ILogger<GenerateReportRequestHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<GenerateReportResponse> Handle(GenerateReportRequest request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow;

        // Default to current month if no dates provided
        var startDate = request.StartDate ?? new DateTime(requestedAt.Year, requestedAt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = request.EndDate ?? new DateTime(requestedAt.Year, requestedAt.Month, DateTime.DaysInMonth(requestedAt.Year, requestedAt.Month), 23, 59, 59, DateTimeKind.Utc);

        var command = new GenerateReportCommand
        {
            CorrelationId = correlationId,
            RequestedAt = requestedAt,
            StartDate = startDate,
            EndDate = endDate,
            RequestedBy = request.RequestedBy ?? "System"
        };

        await _publishEndpoint.Publish(command, cancellationToken);

        _logger.LogInformation(
            "Published GenerateReportCommand with CorrelationId {CorrelationId} for date range {StartDate} to {EndDate}",
            correlationId, startDate, endDate);

        return new GenerateReportResponse(
            correlationId,
            "Accepted",
            "Report generation request has been queued",
            requestedAt.AddMinutes(5)
        );
    }
}
