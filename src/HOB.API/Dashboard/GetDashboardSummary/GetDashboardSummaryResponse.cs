namespace HOB.API.Dashboard.GetDashboardSummary;

public record GetDashboardSummaryResponse(
    int TotalCustomers,
    int TotalOrders,
    int TotalSales,
    decimal TotalRevenue,
    List<RecentOrderSummary> RecentOrders,
    RevenueByStatus RevenueByStatus,
    List<DailyOrderStats> OrdersLast30Days
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
