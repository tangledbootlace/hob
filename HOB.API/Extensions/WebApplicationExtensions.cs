using HOB.API.GetTestEndpoint;
using HOB.API.Customers.CreateCustomer;
using HOB.API.Customers.GetCustomer;
using HOB.API.Customers.ListCustomers;
using HOB.API.Customers.UpdateCustomer;
using HOB.API.Customers.DeleteCustomer;
using HOB.API.Orders.CreateOrder;
using HOB.API.Orders.GetOrder;
using HOB.API.Orders.ListOrders;
using HOB.API.Orders.UpdateOrder;
using HOB.API.Orders.DeleteOrder;
using HOB.API.Sales.CreateSale;
using HOB.API.Sales.GetSale;
using HOB.API.Sales.ListSales;
using HOB.API.Sales.UpdateSale;
using HOB.API.Sales.DeleteSale;
using HOB.API.Reports.GenerateReport;
using HOB.API.Dashboard.GetDashboardSummary;
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

    public static void UseCustomerApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapPost("/", async (CreateCustomerRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(request);
                    return Results.Created($"/api/customers/{response.CustomerId}", response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("CreateCustomer");

        group.MapGet("/{customerId:guid}", async (Guid customerId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(new GetCustomerRequest(customerId));
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("GetCustomer");

        group.MapGet("/", async (int page, int pageSize, string? search, [FromServices] IMediator mediator) =>
            {
                var response = await mediator.Send(new ListCustomersRequest(page, pageSize, search));
                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("ListCustomers");

        group.MapPut("/{customerId:guid}", async (Guid customerId, UpdateCustomerRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var updatedRequest = request with { CustomerId = customerId };
                    var response = await mediator.Send(updatedRequest);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("UpdateCustomer");

        group.MapDelete("/{customerId:guid}", async (Guid customerId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(new DeleteCustomerRequest(customerId));
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("DeleteCustomer");
    }

    public static void UseOrderApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("/", async (CreateOrderRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(request);
                    return Results.Created($"/api/orders/{response.OrderId}", response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("CreateOrder");

        group.MapGet("/{orderId:guid}", async (Guid orderId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(new GetOrderRequest(orderId));
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("GetOrder");

        group.MapGet("/", async (
            int page,
            int pageSize,
            Guid? customerId,
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            [FromServices] IMediator mediator) =>
            {
                var response = await mediator.Send(new ListOrdersRequest(
                    page,
                    pageSize,
                    customerId,
                    status,
                    startDate,
                    endDate
                ));
                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("ListOrders");

        group.MapPut("/{orderId:guid}", async (Guid orderId, UpdateOrderRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var updatedRequest = request with { OrderId = orderId };
                    var response = await mediator.Send(updatedRequest);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("UpdateOrder");

        group.MapDelete("/{orderId:guid}", async (Guid orderId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(new DeleteOrderRequest(orderId));
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("DeleteOrder");
    }

    public static void UseSaleApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/sales").WithTags("Sales");

        group.MapPost("/", async (CreateSaleRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(request);
                    return Results.Created($"/api/sales/{response.SaleId}", response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("CreateSale");

        group.MapGet("/{saleId:guid}", async (Guid saleId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(new GetSaleRequest(saleId));
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("GetSale");

        group.MapGet("/", async (
            int page,
            int pageSize,
            Guid? orderId,
            string? productNameSearch,
            [FromServices] IMediator mediator) =>
            {
                var response = await mediator.Send(new ListSalesRequest(
                    page,
                    pageSize,
                    orderId,
                    productNameSearch
                ));
                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("ListSales");

        group.MapPut("/{saleId:guid}", async (Guid saleId, UpdateSaleRequest request, [FromServices] IMediator mediator) =>
            {
                try
                {
                    var updatedRequest = request with { SaleId = saleId };
                    var response = await mediator.Send(updatedRequest);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("UpdateSale");

        group.MapDelete("/{saleId:guid}", async (Guid saleId, [FromServices] IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(new DeleteSaleRequest(saleId));
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithOpenApi()
            .WithName("DeleteSale");
    }

    public static void UseReportApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapPost("/generate", async (GenerateReportRequest request, [FromServices] IMediator mediator) =>
            {
                var response = await mediator.Send(request);
                return Results.Accepted($"/api/reports/{response.CorrelationId}/status", response);
            })
            .WithOpenApi()
            .WithName("GenerateReport");
    }

    public static void UseDashboardApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/summary", async ([FromServices] IMediator mediator) =>
            {
                var response = await mediator.Send(new GetDashboardSummaryRequest());
                return Results.Ok(response);
            })
            .WithOpenApi()
            .WithName("GetDashboardSummary");
    }
}