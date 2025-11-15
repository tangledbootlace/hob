using HOB.Common.Library.Shared;
using HOB.Common.Library.Observability.HealthChecks;
using HOB.Common.Library.Observability.Metrics;
using HOB.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(x =>
{
    x.AddServerHeader = false;
    x.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("HOB_SERVICES_ContainerPort")!));
});

builder.Services.AddCustomProblemDetails();
builder.Services.AddSwaggerDashboard();

builder.Services.AddServiceHealthChecks();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

//TODO: add DI services

builder.Services.AddPrometheusMetrics();

var app = builder.Build();

app.UseCustomExceptionHandling();

app.UsePrometheusMetrics();

app.UseSwaggerDashboard();

app.UseTestApi();

app.UseHealthCheckRouting();

app.Run();
