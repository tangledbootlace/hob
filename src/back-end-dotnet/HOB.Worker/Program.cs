using HOB.Data;
using HOB.Worker.Consumers;
using HOB.Worker.Observers;
using HOB.Worker.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add DbContext
builder.Services.AddDbContext<HobDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add CSV Report Generator
builder.Services.AddSingleton<ICsvReportGenerator, CsvReportGenerator>();

// Add MassTransit
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

        // Add queue drain observer
        var appLifetime = context.GetRequiredService<IHostApplicationLifetime>();
        var logger = context.GetRequiredService<ILogger<QueueDrainObserver>>();
        cfg.ConnectReceiveObserver(new QueueDrainObserver(appLifetime, logger));
    });
});

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Worker starting up...");

await host.RunAsync();

logger.LogInformation("Worker stopped");
