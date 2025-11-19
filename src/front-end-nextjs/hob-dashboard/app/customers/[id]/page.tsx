import Link from "next/link";
import { getCustomer } from "@/lib/api/customers";
import { updateCustomerAction, FormState } from "@/actions/customers";
import { CustomerForm } from "@/components/customers/customer-form";
import { DeleteCustomerButton } from "@/components/customers/delete-customer-button";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { ArrowLeft } from "lucide-react";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function CustomerDetailPage({ params }: PageProps) {
  const { id } = await params;
  const customer = await getCustomer(id);

  const updateAction = async (prevState: FormState | null, formData: FormData) => {
    "use server";
    return updateCustomerAction(id, prevState, formData);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex items-center gap-4">
          <Link href="/customers">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold tracking-tight break-words">{customer.name}</h1>
            <p className="text-[var(--muted-foreground)] mt-2 text-sm sm:text-base break-all">{customer.email}</p>
          </div>
        </div>
        <div className="sm:mt-0">
          <DeleteCustomerButton
            customerId={id}
            hasOrders={customer.orders.length > 0}
            orderCount={customer.orders.length}
          />
        </div>
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
              <p className="text-[var(--muted-foreground)] text-center py-8">No orders yet</p>
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
