using HOB.Common.Library.Shared;
using HOB.Common.Library.Observability.HealthChecks;
using HOB.Common.Library.Observability.Telemetry;
using HOB.Common.Library.Observability.Metrics;
using HOB.API.Extensions;
using HOB.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(x =>
{
    x.AddServerHeader = false;
    x.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("HOB_SERVICES_ContainerPort")!));
});

builder.Services.AddCustomProblemDetails();
builder.Services.AddSwaggerDashboard();

// Configure JSON serialization to use camelCase for property names
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddServiceHealthChecks(builder.Configuration);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddOpenTelemetryTracing(builder.Configuration);

builder.Services.AddPrometheusMetrics();

var app = builder.Build();

// Initialize database in Development environment
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HobDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.UseCustomExceptionHandling();

app.UsePrometheusMetrics();

app.UseSwaggerDashboard();

app.UseCustomerApi();

app.UseOrderApi();

app.UseSaleApi();

app.UseProductApi();

app.UseReportApi();

app.UseDashboardApi();

app.UseHealthCheckRouting();

app.Run();
