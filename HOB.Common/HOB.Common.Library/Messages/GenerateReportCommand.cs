namespace HOB.Common.Library.Messages;

public record GenerateReportCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string RequestedBy { get; init; } = "System";
}
