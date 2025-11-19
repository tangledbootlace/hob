"use client";

import { useActionState, useEffect } from "react";
import { useFormStatus } from "react-dom";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { deleteCustomerAction, FormState } from "@/actions/customers";
import { Trash2, AlertCircle } from "lucide-react";

interface DeleteCustomerButtonProps {
  customerId: string;
  hasOrders: boolean;
  orderCount: number;
}

function SubmitButton({ disabled, hasOrders, orderCount }: { disabled: boolean; hasOrders: boolean; orderCount: number }) {
  const { pending } = useFormStatus();

  return (
    <div>
      <Button
        variant="destructive"
        type="submit"
        disabled={disabled || pending}
        title={hasOrders ? `Cannot delete customer with ${orderCount} existing order(s)` : "Delete customer"}
      >
        <Trash2 className="h-4 w-4 mr-2" />
        {pending ? "Deleting..." : "Delete Customer"}
      </Button>
      {hasOrders && (
        <p className="text-xs text-[var(--muted-foreground)] mt-1">
          Cannot delete: customer has {orderCount} order{orderCount !== 1 ? "s" : ""}
        </p>
      )}
    </div>
  );
}

export function DeleteCustomerButton({ customerId, hasOrders, orderCount }: DeleteCustomerButtonProps) {
  const router = useRouter();
  const [state, formAction] = useActionState(
    deleteCustomerAction.bind(null, customerId),
    null
  );

  useEffect(() => {
    if (state?.success && state?.redirectTo) {
      router.push(state.redirectTo);
    }
  }, [state, router]);

  return (
    <div className="space-y-2">
      <form action={formAction}>
        <SubmitButton disabled={hasOrders} hasOrders={hasOrders} orderCount={orderCount} />
      </form>
      {state?.message && !state.success && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{state.message}</AlertDescription>
        </Alert>
      )}
    </div>
  );
}
