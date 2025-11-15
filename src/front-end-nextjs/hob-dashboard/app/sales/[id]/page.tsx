import Link from "next/link";
import { getSale } from "@/lib/api/sales";
import { updateSaleAction, deleteSaleAction } from "@/actions/sales";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Trash2 } from "lucide-react";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function SaleDetailPage({ params }: PageProps) {
  const { id } = await params;
  const sale = await getSale(id);

  const updateAction = updateSaleAction.bind(null, id);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/sales">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Sale Details</h1>
            <p className="text-gray-600 mt-2">{sale.productName}</p>
          </div>
        </div>
        <form action={deleteSaleAction.bind(null, id)}>
          <Button variant="destructive" type="submit">
            <Trash2 className="h-4 w-4 mr-2" />
            Delete Sale
          </Button>
        </form>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Edit Sale</CardTitle>
          </CardHeader>
          <CardContent>
            <form action={updateAction} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="productName">Product Name</Label>
                <Input
                  id="productName"
                  type="text"
                  value={sale.productName}
                  disabled
                  className="bg-gray-50"
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
                    defaultValue={sale.quantity}
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
                    defaultValue={sale.unitPrice}
                  />
                </div>
              </div>

              <Button type="submit">Update Sale</Button>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Sale Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Total Price</Label>
              <p className="text-sm text-gray-900 mt-1">${sale.totalPrice.toFixed(2)}</p>
            </div>
            {sale.orderDetails && (
              <>
                <div>
                  <Label>Order Date</Label>
                  <p className="text-sm text-gray-900 mt-1">
                    {new Date(sale.orderDetails.orderDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <Label>Customer</Label>
                  <p className="text-sm text-gray-900 mt-1">{sale.orderDetails.customerName}</p>
                </div>
                <div>
                  <Label>Order Status</Label>
                  <p className="text-sm text-gray-900 mt-1">{sale.orderDetails.status}</p>
                </div>
              </>
            )}
            <div>
              <Label>Created</Label>
              <p className="text-sm text-gray-900 mt-1">{new Date(sale.createdAt).toLocaleString()}</p>
            </div>
            <div>
              <Label>Last Updated</Label>
              <p className="text-sm text-gray-900 mt-1">{new Date(sale.updatedAt).toLocaleString()}</p>
            </div>
            <Link href={`/orders/${sale.orderId}`}>
              <Button variant="outline" className="w-full">View Order</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
