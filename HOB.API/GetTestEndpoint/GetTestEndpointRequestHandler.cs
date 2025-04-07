using MediatR;

namespace HOB.API.GetTestEndpoint;

public class GetTestEndpointRequestHandler : IRequestHandler<GetTestEndpointRequest, IResult>
{
    public async Task<IResult> Handle(GetTestEndpointRequest request, CancellationToken cancellationToken)
    {
        return Results.Ok(new GetTestEndpointResponse
        {
            Message = "Hello from HOB.API"
        });
    }
}