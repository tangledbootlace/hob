export interface DashboardSummary {
  totalCustomers: number;
  totalOrders: number;
  totalSales: number;
  totalProducts: number;
  totalRevenue: number;
  recentOrders: RecentOrderSummary[];
  revenueByStatus: RevenueByStatus;
  ordersLast30Days: DailyOrderStats[];
  lowStockProducts: LowStockProduct[];
}

export interface RecentOrderSummary {
  orderId: string;
  customerName: string;
  orderDate: string;
  totalAmount: number;
  status: string;
}

export interface RevenueByStatus {
  pending: number;
  completed: number;
  cancelled: number;
}

export interface DailyOrderStats {
  date: string;
  count: number;
  revenue: number;
}

export interface LowStockProduct {
  productId: string;
  sku: string;
  name: string;
  stockQuantity: number;
  lowStockThreshold: number;
}
