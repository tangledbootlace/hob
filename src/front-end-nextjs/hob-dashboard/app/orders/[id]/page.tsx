import Link from "next/link";
import { getOrder } from "@/lib/api/orders";
import { updateOrderAction, deleteOrderAction } from "@/actions/orders";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Trash2 } from "lucide-react";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function OrderDetailPage({ params }: PageProps) {
  const { id } = await params;
  const order = await getOrder(id);

  const updateAction = updateOrderAction.bind(null, id);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/orders">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Order Details</h1>
            <p className="text-[var(--muted-foreground)] mt-2">
              {order.customerName} - {new Date(order.orderDate).toLocaleDateString()}
            </p>
          </div>
        </div>
        <form action={deleteOrderAction.bind(null, id)}>
          <Button variant="destructive" type="submit">
            <Trash2 className="h-4 w-4 mr-2" />
            Delete Order
          </Button>
        </form>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Order Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Customer</Label>
              <p className="text-sm text-[var(--foreground)] mt-1">
                <Link href={`/customers/${order.customerId}`} className="hover:text-[var(--primary)] hover:underline">
                  {order.customerName}
                </Link>
              </p>
            </div>
            <div>
              <Label>Order Date</Label>
              <p className="text-sm text-[var(--foreground)] mt-1">{new Date(order.orderDate).toLocaleDateString()}</p>
            </div>
            <div>
              <Label>Total Amount</Label>
              <p className="text-sm text-[var(--foreground)] mt-1">${order.totalAmount.toFixed(2)}</p>
            </div>
            <form action={updateAction} className="space-y-2">
              <Label htmlFor="status">Status</Label>
              <div className="flex gap-2">
                <select
                  id="status"
                  name="status"
                  defaultValue={order.status}
                  className="flex h-10 w-full rounded-md border border-[var(--border)] bg-[var(--background)] text-[var(--foreground)] px-3 py-2 text-sm ring-offset-[var(--background)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--ring)] focus-visible:ring-offset-2"
                >
                  <option value="Pending">Pending</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
                <Button type="submit">Update</Button>
              </div>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Sales Items</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Product</TableHead>
                  <TableHead>Qty</TableHead>
                  <TableHead>Unit Price</TableHead>
                  <TableHead>Total</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {order.sales.map((sale) => (
                  <TableRow key={sale.saleId}>
                    <TableCell>{sale.productName}</TableCell>
                    <TableCell>{sale.quantity}</TableCell>
                    <TableCell>${sale.unitPrice.toFixed(2)}</TableCell>
                    <TableCell>${sale.totalPrice.toFixed(2)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
