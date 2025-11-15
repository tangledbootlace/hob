# MassTransit Integration

## Overview

MassTransit provides a message-based architecture for asynchronous, event-driven communication between the API and Worker services.

## Architecture

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│  HOB.API    │         │  RabbitMQ   │         │ HOB.Worker  │
│             │         │             │         │             │
│  Publisher  │────────>│   Queue     │────────>│  Consumer   │
│             │ Publish │             │ Consume │             │
└─────────────┘         └─────────────┘         └─────────────┘
```

## Message Contracts

### GenerateReportCommand

Published by API when a manager requests a report.

**Location**: `HOB.Common.Library/Messages/GenerateReportCommand.cs`

**Properties**:
```csharp
public record GenerateReportCommand
{
    public Guid CorrelationId { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string RequestedBy { get; init; } = "System";
}
```

**Usage**:
- Sent when: Manager calls `POST /api/reports/generate`
- Queue: `hob-report-generation`
- Expected response: 202 Accepted with CorrelationId

### ReportGeneratedEvent (Future)

Published by Worker when report is complete.

**Properties**:
```csharp
public record ReportGeneratedEvent
{
    public Guid CorrelationId { get; init; }
    public string FilePath { get; init; }
    public DateTime GeneratedAt { get; init; }
    public int TotalRecords { get; init; }
}
```

## Configuration

### API Configuration

**File**: `HOB.API/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddMassTransitWithRabbitMq(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqConfig = configuration.GetConnectionString("RabbitMQ");
            cfg.Host(rabbitMqConfig);

            cfg.ConfigureEndpoints(context);
        });
    });

    return services;
}
```

**Registration in Program.cs**:
```csharp
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);
```

### Worker Configuration

**File**: `HOB.Worker/Program.cs`

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GenerateReportConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetConnectionString("RabbitMQ");
        cfg.Host(rabbitMqConfig);

        cfg.ReceiveEndpoint("hob-report-generation", e =>
        {
            e.ConfigureConsumer<GenerateReportConsumer>(context);

            // Configure retry policy
            e.UseMessageRetry(r => r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(30),
                intervalDelta: TimeSpan.FromSeconds(5)
            ));

            // Prefetch count for queue
            e.PrefetchCount = 1;
        });

        cfg.ConnectReceiveObserver(new QueueDrainObserver(
            context.GetRequiredService<IHostApplicationLifetime>()
        ));
    });
});

var host = builder.Build();
await host.RunAsync();
```

## Consumer Implementation

### GenerateReportConsumer

**File**: `HOB.Worker/Consumers/GenerateReportConsumer.cs`

**Responsibilities**:
1. Receive `GenerateReportCommand` message
2. Query database for Orders, Customers, Sales
3. Generate CSV report
4. Save report to filesystem
5. Log completion (optionally publish ReportGeneratedEvent)

**Implementation Pattern**:
```csharp
public class GenerateReportConsumer : IConsumer<GenerateReportCommand>
{
    private readonly HobDbContext _dbContext;
    private readonly ICsvReportGenerator _reportGenerator;
    private readonly ILogger<GenerateReportConsumer> _logger;

    public async Task Consume(ConsumeContext<GenerateReportCommand> context)
    {
        var command = context.Message;

        // Query data
        var reportData = await GetReportDataAsync(command);

        // Generate CSV
        var csv = _reportGenerator.GenerateSalesReport(reportData);

        // Save to filesystem
        var filePath = await SaveReportAsync(csv, command.CorrelationId);

        _logger.LogInformation(
            "Report generated: {FilePath} for correlation {CorrelationId}",
            filePath, command.CorrelationId);
    }
}
```

## Queue Drain Observer

### Purpose

Monitor the message queue and trigger graceful shutdown when:
- No messages received for 30 seconds
- All in-flight messages have been processed

### Implementation

**File**: `HOB.Worker/Observers/QueueDrainObserver.cs`

**Key Features**:
- Implements `IReceiveObserver` interface
- Maintains idle timer (30 seconds)
- Pauses timer during message processing
- Resets timer after each message
- Calls `IHostApplicationLifetime.StopApplication()` on timeout

**Lifecycle**:
1. Worker starts, observer attached to MassTransit
2. Timer starts (30s countdown)
3. Message received → pause timer
4. Message processing begins
5. Message completed → reset timer to 30s
6. No message for 30s → trigger shutdown
7. MassTransit stops gracefully
8. Worker exits

## Error Handling

### Retry Policy

- **Strategy**: Exponential backoff
- **Retry Limit**: 3 attempts
- **Min Interval**: 1 second
- **Max Interval**: 30 seconds
- **Interval Delta**: 5 seconds

**Timeline**:
- Attempt 1: Immediate
- Attempt 2: Wait 1s
- Attempt 3: Wait 6s
- Attempt 4: Wait 11s
- Failed: Move to error queue

### Error Queue

- **Name**: `hob-report-generation_error`
- **Purpose**: Store failed messages for manual intervention
- **Retention**: 14 days

### Fault Consumer (Future)

Monitor error queue and send alerts.

## Health Checks

### API Health Check

```csharp
builder.Services.AddHealthChecks()
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ"),
        name: "rabbitmq",
        tags: new[] { "messaging" }
    );
```

### Worker Health Check

Worker doesn't expose health endpoints (console app), but logs heartbeat.

## Observability

### Distributed Tracing

MassTransit integrates with OpenTelemetry:
- Each message creates a new Activity span
- CorrelationId propagated via headers
- Consumer spans nested under publisher spans

**Trace Example**:
```
POST /api/reports/generate
  └─ Publish GenerateReportCommand
       └─ Consume GenerateReportCommand
            ├─ Database Query
            ├─ Generate CSV
            └─ Save File
```

### Metrics

**Publisher Metrics**:
- `masstransit_publish_total`: Total messages published
- `masstransit_publish_duration_seconds`: Publish latency

**Consumer Metrics**:
- `masstransit_consume_total`: Total messages consumed
- `masstransit_consume_duration_seconds`: Processing time
- `masstransit_consume_fault_total`: Failed messages

### Logging

**API Logs**:
```
[Information] Publishing GenerateReportCommand with CorrelationId {CorrelationId}
```

**Worker Logs**:
```
[Information] Received GenerateReportCommand with CorrelationId {CorrelationId}
[Information] Generated report with 150 records
[Information] Report saved to /reports/20250115_143000_report.csv
[Information] No messages received for 30s, initiating graceful shutdown
```

## Testing

### Local Testing

1. Start infrastructure:
   ```bash
   docker-compose up -d
   ```

2. Publish message via API:
   ```bash
   curl -X POST http://hob.api.localhost/api/reports/generate
   ```

3. Check Worker logs:
   ```bash
   docker logs -f hob-worker
   ```

4. Verify report created:
   ```bash
   ls -la /reports/
   ```

### RabbitMQ Management UI

- **URL**: http://web.rabbitmq.hob.localhost
- **Credentials**: local/local
- **Check**: Queues, message rates, consumers

## Security Considerations

- **Connection String**: Use Azure Key Vault in production
- **Message Encryption**: Enable SSL/TLS for RabbitMQ
- **Authentication**: Use certificate-based auth for production
- **Message Validation**: Validate all incoming messages
- **Rate Limiting**: Prevent message flooding

## Future Enhancements

- Add Saga pattern for complex workflows
- Implement message scheduling (delayed delivery)
- Add request/response pattern for synchronous scenarios
- Implement event sourcing for audit trail
- Add message deduplication
- Implement circuit breaker pattern
