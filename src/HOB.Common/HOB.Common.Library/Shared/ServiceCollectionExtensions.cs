using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace HOB.Common.Library.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
                ctx.ProblemDetails.Extensions.Add("problemId", Activity.Current?.Id));
    }

    public static void AddSwaggerDashboard(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}