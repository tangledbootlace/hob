using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace HOB.Common.Library.Shared;

public static class WebApplicationExtensions
{
    public static void UseSwaggerDashboard(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    public static void UseCustomExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context
                =>
            {
                var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandler?.Error;

                if (exception != null)
                {
                    System.Diagnostics.Activity.Current?.RecordException(exception);
                }
                
                await Results.Problem()
                    .ExecuteAsync(context);
            }));
    }
}