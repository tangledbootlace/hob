import { OrderForm } from "@/components/orders/order-form";
import { createOrderAction } from "@/actions/orders";
import { getCustomers } from "@/lib/api/customers";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";

export default async function NewOrderPage() {
  const customersData = await getCustomers(1, 100);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/orders">
          <Button variant="outline" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Order</h1>
          <p className="text-gray-600 mt-2">Add a new order with sales items</p>
        </div>
      </div>

      <div className="max-w-4xl">
        <OrderForm action={createOrderAction} customers={customersData.items} />
      </div>
    </div>
  );
}
