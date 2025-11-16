import Link from "next/link";
import { getDashboardSummary } from "@/lib/api/dashboard";
import { StatsCard } from "@/components/dashboard/stats-card";
import { RecentOrders } from "@/components/dashboard/recent-orders";
import { Users, ShoppingCart, Package, DollarSign, AlertTriangle, Box } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const summary = await getDashboardSummary();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-gray-600 mt-2">
          Overview of your business metrics and recent activity
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
        <StatsCard
          title="Total Customers"
          value={summary.totalCustomers}
          icon={Users}
          description="Active customers"
        />
        <StatsCard
          title="Total Products"
          value={summary.totalProducts}
          icon={Box}
          description="Active products"
        />
        <StatsCard
          title="Total Orders"
          value={summary.totalOrders}
          icon={ShoppingCart}
          description="All time orders"
        />
        <StatsCard
          title="Total Sales"
          value={summary.totalSales}
          icon={Package}
          description="Individual sale items"
        />
        <StatsCard
          title="Total Revenue"
          value={`$${summary.totalRevenue.toFixed(2)}`}
          icon={DollarSign}
          description="All time revenue"
        />
      </div>

      {/* Revenue by Status */}
      <div className="grid gap-4 md:grid-cols-3">
        <StatsCard
          title="Pending Revenue"
          value={`$${summary.revenueByStatus.pending.toFixed(2)}`}
          icon={DollarSign}
        />
        <StatsCard
          title="Completed Revenue"
          value={`$${summary.revenueByStatus.completed.toFixed(2)}`}
          icon={DollarSign}
        />
        <StatsCard
          title="Cancelled Revenue"
          value={`$${summary.revenueByStatus.cancelled.toFixed(2)}`}
          icon={DollarSign}
        />
      </div>

      {/* Low Stock Alerts & Recent Orders */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Low Stock Alerts */}
        {summary.lowStockProducts.length > 0 && (
          <Card className="border-yellow-200 bg-yellow-50">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-yellow-800">
                <AlertTriangle className="h-5 w-5" />
                Low Stock Alerts ({summary.lowStockProducts.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {summary.lowStockProducts.slice(0, 5).map((product) => (
                  <div
                    key={product.productId}
                    className="flex items-center justify-between rounded-lg bg-white p-3 shadow-sm"
                  >
                    <div>
                      <p className="font-medium text-gray-900">{product.name}</p>
                      <p className="text-sm text-gray-600">SKU: {product.sku}</p>
                    </div>
                    <div className="text-right">
                      <p className="font-semibold text-yellow-700">
                        {product.stockQuantity} units
                      </p>
                      <p className="text-xs text-gray-500">
                        Threshold: {product.lowStockThreshold}
                      </p>
                    </div>
                  </div>
                ))}
                <Link href="/products?lowStock=true">
                  <Button variant="outline" className="w-full mt-2">
                    View All Low Stock Products
                  </Button>
                </Link>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Recent Orders - takes full width if no low stock alerts */}
        <div className={summary.lowStockProducts.length === 0 ? "lg:col-span-2" : ""}>
          <RecentOrders orders={summary.recentOrders} />
        </div>
      </div>
    </div>
  );
}
