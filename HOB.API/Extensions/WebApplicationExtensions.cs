using HOB.API.GetTestEndpoint;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HOB.API.Extensions;

public static class WebApplicationExtensions
{
    public static void UseTestApi(this WebApplication app)
    {
        app.MapGet(pattern: "/v1",
            async ([FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new GetTestEndpointRequest());
                return result;
            })
            .Produces<GetTestEndpointResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithName("v1-TestEndpoint")
            .WithTags("Reader")
            .WithOpenApi();
    }
}