import Link from "next/link";
import { getOrders } from "@/lib/api/orders";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Plus } from "lucide-react";

export const dynamic = "force-dynamic";

interface PageProps {
  searchParams: Promise<{ page?: string; status?: string; customerId?: string }>;
}

export default async function OrdersPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const page = Number(params.page) || 1;
  const status = params.status || undefined;
  const customerId = params.customerId || undefined;

  const data = await getOrders(page, 20, customerId, status);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Orders</h1>
          <p className="text-[var(--muted-foreground)] mt-2">Manage customer orders and transactions</p>
        </div>
        <Link href="/orders/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Order
          </Button>
        </Link>
      </div>

      <div className="bg-[var(--card)] rounded-lg border border-[var(--border)]">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Customer</TableHead>
              <TableHead>Order Date</TableHead>
              <TableHead>Total Amount</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Items</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center py-8 text-[var(--muted-foreground)]">
                  No orders found
                </TableCell>
              </TableRow>
            ) : (
              data.items.map((order) => (
                <TableRow key={order.orderId}>
                  <TableCell className="font-medium">
                    <Link href={`/orders/${order.orderId}`} className="hover:text-[var(--primary)] hover:underline">
                      {order.customerName}
                    </Link>
                  </TableCell>
                  <TableCell>{new Date(order.orderDate).toLocaleDateString()}</TableCell>
                  <TableCell>${order.totalAmount.toFixed(2)}</TableCell>
                  <TableCell>
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                        order.status === "Completed"
                          ? "bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-400"
                          : order.status === "Pending"
                          ? "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-400"
                          : "bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-400"
                      }`}
                    >
                      {order.status}
                    </span>
                  </TableCell>
                  <TableCell>{order.sales?.length || 0}</TableCell>
                  <TableCell className="text-right">
                    <Link href={`/orders/${order.orderId}`}>
                      <Button variant="outline" size="sm">View</Button>
                    </Link>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="flex justify-center gap-2">
          {page > 1 && (
            <Link href={`/orders?page=${page - 1}`}>
              <Button variant="outline">Previous</Button>
            </Link>
          )}
          <span className="flex items-center px-4 text-sm text-[var(--muted-foreground)]">
            Page {page} of {data.totalPages}
          </span>
          {page < data.totalPages && (
            <Link href={`/orders?page=${page + 1}`}>
              <Button variant="outline">Next</Button>
            </Link>
          )}
        </div>
      )}
    </div>
  );
}
