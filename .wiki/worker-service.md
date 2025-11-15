# Worker Service

## Overview

HOB.Worker is a .NET console application designed to run as an Azure Container App Job. It processes report generation messages from RabbitMQ and implements a queue drain observer for graceful shutdown.

## Architecture

### Execution Model

**Azure Container App Job**:
- Triggered by cron schedule (e.g., every hour, daily)
- Starts up, processes available messages
- Shuts down gracefully when queue is empty

**Lifecycle**:
```
Cron Trigger → Start Worker → Connect to RabbitMQ → Process Messages
                                      ↓
              Graceful Exit ← 30s No Messages ← Monitor Queue
```

## Project Structure

```
HOB.Worker/
├── Program.cs                          # Host configuration and startup
├── Consumers/
│   └── GenerateReportConsumer.cs      # MassTransit consumer
├── Observers/
│   └── QueueDrainObserver.cs          # Queue monitoring and shutdown
├── Services/
│   ├── ICsvReportGenerator.cs         # Report generation interface
│   └── CsvReportGenerator.cs          # CSV formatting implementation
├── Dockerfile                          # Container image definition
└── appsettings.json                    # Configuration
```

## Queue Drain Observer

### Purpose

Monitor the RabbitMQ queue and trigger graceful shutdown when no messages are received for 30 seconds.

### Implementation Details

**File**: `HOB.Worker/Observers/QueueDrainObserver.cs`

**Interface**: `IReceiveObserver` (MassTransit)

**Key Methods**:
- `PreReceive(ReceiveContext)`: Called before receiving a message
- `PostReceive(ReceiveContext)`: Called after receiving a message
- `PostConsume(ConsumeContext, TimeSpan, string)`: Called after consuming a message
- `ConsumeFault(ConsumeContext, TimeSpan, string, Exception)`: Called on consume failure

**State Management**:
```csharp
private Timer? _idleTimer;
private int _activeMessages = 0;
private readonly object _lock = new();
```

**Timer Behavior**:

1. **Initialization**:
   ```csharp
   _idleTimer = new Timer(
       callback: OnIdleTimeout,
       state: null,
       dueTime: TimeSpan.FromSeconds(30),
       period: Timeout.InfiniteTimeSpan
   );
   ```

2. **On Message Received** (PreReceive):
   ```csharp
   lock (_lock)
   {
       _activeMessages++;
       _idleTimer?.Change(Timeout.Infinite, Timeout.Infinite); // Pause
   }
   ```

3. **On Message Completed** (PostConsume):
   ```csharp
   lock (_lock)
   {
       _activeMessages--;
       if (_activeMessages == 0)
       {
           _idleTimer?.Change(TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
       }
   }
   ```

4. **On Timeout**:
   ```csharp
   private void OnIdleTimeout(object? state)
   {
       _logger.LogInformation("No messages for 30 seconds, shutting down...");
       _appLifetime.StopApplication();
   }
   ```

### Thread Safety

- Uses `lock` statement to protect shared state
- Atomic operations for counter increments/decrements
- Timer callback runs on thread pool thread

### Edge Cases

**Multiple messages arriving simultaneously**:
- Counter increments for each message
- Timer only resets when ALL messages complete

**Message processing takes > 30 seconds**:
- Timer paused during processing
- No timeout while work is in progress

**Consumer throws exception**:
- `ConsumeFault` decrements counter
- Timer still resets after failure
- Failed message handled by retry policy

## Report Generation

### CSV Report Generator

**Interface**: `ICsvReportGenerator`

```csharp
public interface ICsvReportGenerator
{
    string GenerateSalesReport(IEnumerable<ReportDataRow> data);
}
```

**Implementation**: `CsvReportGenerator`

### Report Format

**File Name**: `{yyyyMMdd}_{HHmmss}_sales_report.csv`

**Columns**:
```
Customer Name, Customer Email, Customer Phone, Order ID, Order Date, Order Total, Order Status, Product Name, Quantity, Unit Price, Line Total
```

**Example**:
```csv
Customer Name,Customer Email,Customer Phone,Order ID,Order Date,Order Total,Order Status,Product Name,Quantity,Unit Price,Line Total
John Doe,john.doe@example.com,555-0100,guid-10,2025-01-15,150.00,Completed,Widget A,2,50.00,100.00
John Doe,john.doe@example.com,555-0100,guid-10,2025-01-15,150.00,Completed,Widget B,1,50.00,50.00
Jane Smith,jane.smith@example.com,555-0200,guid-11,2025-01-16,75.00,Pending,Widget C,1,75.00,75.00
```

### Data Query

**Query Structure**:
```csharp
var reportData = await _dbContext.Sales
    .Include(s => s.Order)
        .ThenInclude(o => o.Customer)
    .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
    .OrderBy(s => s.Order.Customer.Name)
    .ThenBy(s => s.Order.OrderDate)
    .Select(s => new ReportDataRow
    {
        CustomerName = s.Order.Customer.Name,
        CustomerEmail = s.Order.Customer.Email,
        CustomerPhone = s.Order.Customer.Phone,
        OrderId = s.Order.OrderId,
        OrderDate = s.Order.OrderDate,
        OrderTotal = s.Order.TotalAmount,
        OrderStatus = s.Order.Status,
        ProductName = s.ProductName,
        Quantity = s.Quantity,
        UnitPrice = s.UnitPrice,
        LineTotal = s.TotalPrice
    })
    .ToListAsync();
```

### File Storage

**Directory**: `/reports/` (mounted volume in Docker)

**Permissions**: Write access required

**Retention**: Handled externally (not by worker)

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "MassTransit": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=db;Database=HOB;User Id=sa;Password=Password123;TrustServerCertificate=True;",
    "RabbitMQ": "amqp://local:local@rabbitmq:5672"
  },
  "ReportSettings": {
    "OutputDirectory": "/reports",
    "DateFormat": "yyyyMMdd_HHmmss"
  }
}
```

### Environment Variables

- `DOTNET_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: SQL Server connection
- `ConnectionStrings__RabbitMQ`: RabbitMQ connection
- `ReportSettings__OutputDirectory`: Report output path

## Logging

### Log Events

**Startup**:
```
[Information] Worker starting up...
[Information] Connecting to RabbitMQ: amqp://local:local@rabbitmq:5672
[Information] Connected to database: HOB
[Information] Queue drain observer attached
```

**Message Processing**:
```
[Information] Received GenerateReportCommand {CorrelationId}
[Information] Querying data for report from {StartDate} to {EndDate}
[Information] Retrieved {RecordCount} records
[Information] Generating CSV report
[Information] Report saved: /reports/20250115_143000_sales_report.csv
[Information] Message processing completed in {Duration}ms
```

**Queue Drain**:
```
[Information] Active messages: 0, starting idle timer
[Information] No messages received for 30 seconds
[Information] Initiating graceful shutdown
```

**Shutdown**:
```
[Information] Shutdown signal received
[Information] Waiting for active messages to complete...
[Information] All messages processed
[Information] Disconnecting from RabbitMQ
[Information] Worker stopped
```

### Error Logging

```
[Error] Failed to process message {CorrelationId}: {ExceptionMessage}
[Error] Database query failed: {ExceptionMessage}
[Error] Failed to save report: {ExceptionMessage}
```

## Docker Configuration

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["HOB.Worker/HOB.Worker.csproj", "HOB.Worker/"]
COPY ["HOB.Data/HOB.Data.csproj", "HOB.Data/"]
COPY ["HOB.Common/HOB.Common.Library/HOB.Common.Library.csproj", "HOB.Common/HOB.Common.Library/"]
RUN dotnet restore "HOB.Worker/HOB.Worker.csproj"

COPY . .
WORKDIR "/src/HOB.Worker"
RUN dotnet build "HOB.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HOB.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create reports directory
RUN mkdir -p /reports && chmod 777 /reports

ENTRYPOINT ["dotnet", "HOB.Worker.dll"]
```

### docker-compose.service.yml

```yaml
worker:
  build:
    context: .
    dockerfile: HOB.Worker/Dockerfile
  image: hob-worker:latest
  container_name: hob-worker
  depends_on:
    db:
      condition: service_healthy
    rabbitmq:
      condition: service_healthy
  environment:
    - DOTNET_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Server=db;Database=HOB;User Id=sa;Password=Password123;TrustServerCertificate=True;
    - ConnectionStrings__RabbitMQ=amqp://local:local@rabbitmq:5672
  volumes:
    - ./reports:/reports
  networks:
    - hob-network
```

## Azure Container App Job

### Configuration

**Trigger**: Cron schedule

**Example YAML**:
```yaml
properties:
  configuration:
    triggerType: Schedule
    scheduleTriggerConfig:
      cronExpression: "0 */1 * * *"  # Every hour
      parallelism: 1
      replicaCompletionCount: 1
  template:
    containers:
      - name: hob-worker
        image: myregistry.azurecr.io/hob-worker:latest
        env:
          - name: ConnectionStrings__DefaultConnection
            secretRef: sql-connection
          - name: ConnectionStrings__RabbitMQ
            secretRef: rabbitmq-connection
        volumeMounts:
          - volumeName: reports
            mountPath: /reports
```

### Scaling

- **Parallelism**: 1 (single instance)
- **Replica Completion**: 1 (completes when one instance exits)
- **Timeout**: 30 minutes (safety timeout)

### Monitoring

- **Logs**: Azure Monitor / Application Insights
- **Metrics**: Container metrics (CPU, memory)
- **Alerts**: Job failures, long run times

## Testing

### Local Testing

1. **Start Infrastructure**:
   ```bash
   docker-compose -f docker-compose.infrastructure.yml up -d
   ```

2. **Run Worker Locally**:
   ```bash
   cd HOB.Worker
   dotnet run
   ```

3. **Publish Test Message**:
   ```bash
   curl -X POST http://localhost:8080/api/reports/generate
   ```

4. **Verify Report**:
   ```bash
   ls -la /reports/
   cat /reports/20250115_143000_sales_report.csv
   ```

### Unit Testing

**Test Queue Drain Observer**:
```csharp
[Fact]
public async Task QueueDrainObserver_TriggersShutdown_After30Seconds()
{
    // Arrange
    var appLifetime = new Mock<IHostApplicationLifetime>();
    var observer = new QueueDrainObserver(appLifetime.Object);

    // Act
    await Task.Delay(TimeSpan.FromSeconds(31));

    // Assert
    appLifetime.Verify(x => x.StopApplication(), Times.Once);
}
```

## Performance Considerations

- **Message Prefetch**: Set to 1 to avoid overloading
- **Database Connections**: Use connection pooling
- **Memory**: Batch large datasets in chunks
- **CSV Generation**: Stream large files instead of loading all into memory

## Security

- **Connection Strings**: Use Azure Key Vault
- **File Permissions**: Restrict report directory access
- **Input Validation**: Validate date ranges in messages
- **SQL Injection**: Use parameterized queries (EF Core handles this)

## Future Enhancements

- Upload reports to Azure Blob Storage
- Send email notifications when report is ready
- Support different report formats (Excel, PDF)
- Implement report templates
- Add report history tracking
- Support incremental reports (delta since last run)
