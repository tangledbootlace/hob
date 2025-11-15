using HOB.Common.Library.Messages;
using HOB.Data;
using HOB.Worker.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HOB.Worker.Consumers;

public class GenerateReportConsumer : IConsumer<GenerateReportCommand>
{
    private readonly HobDbContext _dbContext;
    private readonly ICsvReportGenerator _reportGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GenerateReportConsumer> _logger;

    public GenerateReportConsumer(
        HobDbContext dbContext,
        ICsvReportGenerator reportGenerator,
        IConfiguration configuration,
        ILogger<GenerateReportConsumer> logger)
    {
        _dbContext = dbContext;
        _reportGenerator = reportGenerator;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateReportCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation("Received GenerateReportCommand with CorrelationId {CorrelationId}", command.CorrelationId);

        var startDate = command.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = command.EndDate ?? DateTime.UtcNow;

        _logger.LogInformation("Querying data for report from {StartDate} to {EndDate}", startDate, endDate);

        // Query data with all relationships
        var reportData = await _dbContext.Sales
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
            .OrderBy(s => s.Order.Customer.Name)
            .ThenBy(s => s.Order.OrderDate)
            .Select(s => new ReportDataRow(
                s.Order.Customer.Name,
                s.Order.Customer.Email,
                s.Order.Customer.Phone,
                s.Order.OrderId,
                s.Order.OrderDate,
                s.Order.TotalAmount,
                s.Order.Status,
                s.ProductName,
                s.Quantity,
                s.UnitPrice,
                s.TotalPrice
            ))
            .ToListAsync();

        _logger.LogInformation("Retrieved {RecordCount} records", reportData.Count);

        // Generate CSV
        _logger.LogInformation("Generating CSV report");
        var csv = _reportGenerator.GenerateSalesReport(reportData);

        // Save to file system
        var outputDir = _configuration["ReportSettings:OutputDirectory"] ?? "/reports";
        var dateFormat = _configuration["ReportSettings:DateFormat"] ?? "yyyyMMdd_HHmmss";
        var timestamp = DateTime.UtcNow.ToString(dateFormat);
        var fileName = $"{timestamp}_sales_report.csv";
        var filePath = Path.Combine(outputDir, fileName);

        // Ensure directory exists
        Directory.CreateDirectory(outputDir);

        await File.WriteAllTextAsync(filePath, csv);

        _logger.LogInformation("Report saved to {FilePath} with {RecordCount} records", filePath, reportData.Count);
        _logger.LogInformation("Message processing completed for CorrelationId {CorrelationId}", command.CorrelationId);
    }
}
