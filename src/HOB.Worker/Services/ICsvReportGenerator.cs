namespace HOB.Worker.Services;

public interface ICsvReportGenerator
{
    string GenerateSalesReport(IEnumerable<ReportDataRow> data);
}

public record ReportDataRow(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    Guid OrderId,
    DateTime OrderDate,
    decimal OrderTotal,
    string OrderStatus,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
