import { CustomerForm } from "@/components/customers/customer-form";
import { createCustomerAction } from "@/actions/customers";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";

export default function NewCustomerPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/customers">
          <Button variant="outline" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Customer</h1>
          <p className="text-gray-600 mt-2">Add a new customer to your database</p>
        </div>
      </div>

      <div className="max-w-2xl">
        <CustomerForm action={createCustomerAction} />
      </div>
    </div>
  );
}
