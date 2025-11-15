namespace HOB.API.Reports.GenerateReport;

public record GenerateReportResponse(
    Guid CorrelationId,
    string Status,
    string Message,
    DateTime EstimatedCompletionTime
);
