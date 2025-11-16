using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Dashboard.GetDashboardSummary;

public class GetDashboardSummaryRequestHandler : IRequestHandler<GetDashboardSummaryRequest, GetDashboardSummaryResponse>
{
    private readonly HobDbContext _dbContext;

    public GetDashboardSummaryRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetDashboardSummaryResponse> Handle(GetDashboardSummaryRequest request, CancellationToken cancellationToken)
    {
        // Get total counts
        var totalCustomers = await _dbContext.Customers.CountAsync(cancellationToken);
        var totalOrders = await _dbContext.Orders.CountAsync(cancellationToken);
        var totalSales = await _dbContext.Sales.CountAsync(cancellationToken);
        var totalProducts = await _dbContext.Products.Where(p => p.IsActive).CountAsync(cancellationToken);

        // Get total revenue
        var totalRevenue = await _dbContext.Orders.SumAsync(o => o.TotalAmount, cancellationToken);

        // Get recent orders (last 10)
        var recentOrders = await _dbContext.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderSummary(
                o.OrderId,
                o.Customer.Name,
                o.OrderDate,
                o.TotalAmount,
                o.Status
            ))
            .ToListAsync(cancellationToken);

        // Get revenue by status
        var revenueByStatus = await _dbContext.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
            .ToListAsync(cancellationToken);

        var revenueStatus = new RevenueByStatus(
            Pending: revenueByStatus.FirstOrDefault(r => r.Status == "Pending")?.Revenue ?? 0,
            Completed: revenueByStatus.FirstOrDefault(r => r.Status == "Completed")?.Revenue ?? 0,
            Cancelled: revenueByStatus.FirstOrDefault(r => r.Status == "Cancelled")?.Revenue ?? 0
        );

        // Get orders for last 30 days grouped by date
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).Date;
        var ordersLast30Days = await _dbContext.Orders
            .Where(o => o.OrderDate >= thirtyDaysAgo)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailyOrderStats(
                g.Key,
                g.Count(),
                g.Sum(o => o.TotalAmount)
            ))
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        // Get low stock products
        var lowStockProducts = await _dbContext.Products
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockProduct(
                p.ProductId,
                p.SKU,
                p.Name,
                p.StockQuantity,
                p.LowStockThreshold
            ))
            .ToListAsync(cancellationToken);

        return new GetDashboardSummaryResponse(
            totalCustomers,
            totalOrders,
            totalSales,
            totalProducts,
            totalRevenue,
            recentOrders,
            revenueStatus,
            ordersLast30Days,
            lowStockProducts
        );
    }
}
