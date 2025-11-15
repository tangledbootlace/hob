using HOB.Common.Library.Shared;
using HOB.Common.Library.Observability.HealthChecks;
using HOB.Common.Library.Observability.Telemetry;
using HOB.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(x =>
{
    x.AddServerHeader = false;
    x.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("HOB_SERVICES_ContainerPort")!));
});

builder.Services.AddCustomProblemDetails();
builder.Services.AddSwaggerDashboard();

builder.Services.AddServiceHealthChecks(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddOpenTelemetryTracing(builder.Configuration);

//TODO: add service metrics middleware

var app = builder.Build();

app.UseCustomExceptionHandling();

//TODO: use metrics middleware

app.UseSwaggerDashboard();

app.UseTestApi();

app.UseHealthCheckRouting();

app.Run();
