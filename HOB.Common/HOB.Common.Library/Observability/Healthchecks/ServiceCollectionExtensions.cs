using Microsoft.Extensions.DependencyInjection;

namespace HOB.Common.Library.Observability.HealthChecks;

public static class ServiceCollectionExtensions
{
    public static void AddServiceHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
    }
}