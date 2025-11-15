using Microsoft.Extensions.DependencyInjection;

namespace HOB.Common.Library.Observability.Metrics;

public static class ServiceCollectionExtensions
{
    public static void AddPrometheusMetrics(this IServiceCollection services)
    {
        // Prometheus metrics are automatically registered by the prometheus-net.AspNetCore package
        // No additional service registration is required
    }
}
