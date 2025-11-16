namespace HOB.API.Dashboard.GetDashboardSummary;

public record GetDashboardSummaryResponse(
    int TotalCustomers,
    int TotalOrders,
    int TotalSales,
    int TotalProducts,
    decimal TotalRevenue,
    List<RecentOrderSummary> RecentOrders,
    RevenueByStatus RevenueByStatus,
    List<DailyOrderStats> OrdersLast30Days,
    List<LowStockProduct> LowStockProducts
);

public record RecentOrderSummary(
    Guid OrderId,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status
);

public record RevenueByStatus(
    decimal Pending,
    decimal Completed,
    decimal Cancelled
);

public record DailyOrderStats(
    DateTime Date,
    int Count,
    decimal Revenue
);

public record LowStockProduct(
    Guid ProductId,
    string SKU,
    string Name,
    int StockQuantity,
    int LowStockThreshold
);
