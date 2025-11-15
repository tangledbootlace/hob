import { createSaleAction } from "@/actions/sales";
import { getOrders } from "@/lib/api/orders";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft } from "lucide-react";

export default async function NewSalePage() {
  const ordersData = await getOrders(1, 100);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/sales">
          <Button variant="outline" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Sale</h1>
          <p className="text-gray-600 mt-2">Add a new sale item to an existing order</p>
        </div>
      </div>

      <div className="max-w-2xl">
        <Card>
          <CardHeader>
            <CardTitle>New Sale Item</CardTitle>
          </CardHeader>
          <CardContent>
            <form action={createSaleAction} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="orderId">Order *</Label>
                <select
                  id="orderId"
                  name="orderId"
                  required
                  className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
                >
                  <option value="">Select an order</option>
                  {ordersData.items.map((order) => (
                    <option key={order.orderId} value={order.orderId}>
                      {order.customerName} - {new Date(order.orderDate).toLocaleDateString()} (${order.totalAmount.toFixed(2)})
                    </option>
                  ))}
                </select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="productName">Product Name *</Label>
                <Input
                  id="productName"
                  name="productName"
                  type="text"
                  required
                  placeholder="Product name"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="quantity">Quantity *</Label>
                  <Input
                    id="quantity"
                    name="quantity"
                    type="number"
                    required
                    min="1"
                    defaultValue="1"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="unitPrice">Unit Price *</Label>
                  <Input
                    id="unitPrice"
                    name="unitPrice"
                    type="number"
                    required
                    min="0"
                    step="0.01"
                    defaultValue="0"
                  />
                </div>
              </div>

              <Button type="submit">Create Sale</Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
