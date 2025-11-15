import { getDashboardSummary } from "@/lib/api/dashboard";
import { StatsCard } from "@/components/dashboard/stats-card";
import { RecentOrders } from "@/components/dashboard/recent-orders";
import { Users, ShoppingCart, Package, DollarSign } from "lucide-react";

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
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatsCard
          title="Total Customers"
          value={summary.totalCustomers}
          icon={Users}
          description="Active customers"
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

      {/* Recent Orders */}
      <RecentOrders orders={summary.recentOrders} />
    </div>
  );
}
