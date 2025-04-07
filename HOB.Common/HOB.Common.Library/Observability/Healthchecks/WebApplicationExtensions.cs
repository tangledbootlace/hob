using Microsoft.AspNetCore.Builder;

namespace HOB.Common.Library.Observability.HealthChecks;

public static class WebApplicationExtenions
{
    public static void UseHealthCheckRouting(this WebApplication app)
    {
        app.MapHealthChecks("/healthz");
    }
}