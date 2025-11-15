using System.Text;

namespace HOB.Worker.Services;

public class CsvReportGenerator : ICsvReportGenerator
{
    public string GenerateSalesReport(IEnumerable<ReportDataRow> data)
    {
        var sb = new StringBuilder();

        // Write header
        sb.AppendLine("Customer Name,Customer Email,Customer Phone,Order ID,Order Date,Order Total,Order Status,Product Name,Quantity,Unit Price,Line Total");

        // Write data rows
        foreach (var row in data)
        {
            sb.AppendLine($"{EscapeCsv(row.CustomerName)},{EscapeCsv(row.CustomerEmail)},{EscapeCsv(row.CustomerPhone ?? "")},{row.OrderId},{row.OrderDate:yyyy-MM-dd HH:mm:ss},{row.OrderTotal:F2},{EscapeCsv(row.OrderStatus)},{EscapeCsv(row.ProductName)},{row.Quantity},{row.UnitPrice:F2},{row.LineTotal:F2}");
        }

        return sb.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // If the value contains comma, newline, or double quote, wrap it in quotes
        if (value.Contains(',') || value.Contains('\n') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
