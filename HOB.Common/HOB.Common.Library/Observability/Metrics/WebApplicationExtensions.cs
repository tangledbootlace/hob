using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace HOB.Common.Library.Observability.Metrics;

public static class WebApplicationExtensions
{
    public static void UsePrometheusMetrics(this WebApplication app)
    {
        // Enable HTTP metrics collection
        app.UseHttpMetrics();

        // Map the /metrics endpoint for Prometheus scraping
        app.MapMetrics();
    }
}
