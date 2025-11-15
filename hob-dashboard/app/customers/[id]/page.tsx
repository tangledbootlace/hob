import Link from "next/link";
import { getCustomer } from "@/lib/api/customers";
import { updateCustomerAction, deleteCustomerAction } from "@/actions/customers";
import { CustomerForm } from "@/components/customers/customer-form";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { ArrowLeft, Trash2 } from "lucide-react";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function CustomerDetailPage({ params }: PageProps) {
  const { id } = await params;
  const customer = await getCustomer(id);

  const updateAction = updateCustomerAction.bind(null, id);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/customers">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">{customer.name}</h1>
            <p className="text-gray-600 mt-2">{customer.email}</p>
          </div>
        </div>
        <form action={deleteCustomerAction.bind(null, id)}>
          <Button variant="destructive" type="submit">
            <Trash2 className="h-4 w-4 mr-2" />
            Delete Customer
          </Button>
        </form>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <CustomerForm
          action={updateAction}
          defaultValues={{
            name: customer.name,
            email: customer.email,
            phone: customer.phone,
          }}
          isEdit
        />

        <Card>
          <CardHeader>
            <CardTitle>Customer Orders</CardTitle>
          </CardHeader>
          <CardContent>
            {customer.orders?.length === 0 ? (
              <p className="text-gray-500 text-center py-8">No orders yet</p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {customer.orders?.map((order) => (
                    <TableRow key={order.orderId}>
                      <TableCell>{new Date(order.orderDate).toLocaleDateString()}</TableCell>
                      <TableCell>${order.totalAmount.toFixed(2)}</TableCell>
                      <TableCell>{order.status}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
