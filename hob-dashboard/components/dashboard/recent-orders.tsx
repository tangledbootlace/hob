import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RecentOrderSummary } from "@/lib/types/dashboard";

interface RecentOrdersProps {
  orders: RecentOrderSummary[];
}

export function RecentOrders({ orders }: RecentOrdersProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Recent Orders</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {orders.map((order) => (
            <div key={order.orderId} className="flex items-center justify-between border-b pb-4 last:border-0">
              <div className="space-y-1">
                <Link
                  href={`/orders/${order.orderId}`}
                  className="font-medium hover:text-blue-600 hover:underline"
                >
                  {order.customerName}
                </Link>
                <p className="text-sm text-gray-600">
                  {new Date(order.orderDate).toLocaleDateString()}
                </p>
              </div>
              <div className="text-right">
                <p className="font-semibold">${order.totalAmount.toFixed(2)}</p>
                <StatusBadge status={order.status} />
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function StatusBadge({ status }: { status: string }) {
  const colors = {
    Pending: "bg-yellow-100 text-yellow-800",
    Completed: "bg-green-100 text-green-800",
    Cancelled: "bg-red-100 text-red-800",
  };

  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
        colors[status as keyof typeof colors] || "bg-gray-100 text-gray-800"
      }`}
    >
      {status}
    </span>
  );
}
