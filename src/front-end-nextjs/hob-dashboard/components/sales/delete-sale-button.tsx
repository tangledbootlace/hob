"use client";

import { useActionState, useEffect } from "react";
import { useFormStatus } from "react-dom";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { deleteSaleAction, FormState } from "@/actions/sales";
import { Trash2, AlertCircle } from "lucide-react";

interface DeleteSaleButtonProps {
  saleId: string;
  orderStatus: string;
  isLastSale: boolean;
}

function SubmitButton({
  disabled,
  orderStatus,
  isLastSale
}: {
  disabled: boolean;
  orderStatus: string;
  isLastSale: boolean;
}) {
  const { pending } = useFormStatus();

  const getDisabledReason = () => {
    if (orderStatus !== "Pending") {
      return `Cannot delete sale from ${orderStatus} order`;
    }
    if (isLastSale) {
      return "Cannot delete the last sale in an order";
    }
    return "Delete sale";
  };

  const getHelpText = () => {
    if (orderStatus !== "Pending") {
      return `Only sales from Pending orders can be deleted. This order is ${orderStatus}.`;
    }
    if (isLastSale) {
      return "An order must have at least one sale.";
    }
    return null;
  };

  const helpText = getHelpText();

  return (
    <div>
      <Button
        variant="destructive"
        type="submit"
        disabled={disabled || pending}
        title={getDisabledReason()}
      >
        <Trash2 className="h-4 w-4 mr-2" />
        {pending ? "Deleting..." : "Delete Sale"}
      </Button>
      {helpText && (
        <p className="text-xs text-[var(--muted-foreground)] mt-1">
          {helpText}
        </p>
      )}
    </div>
  );
}

export function DeleteSaleButton({ saleId, orderStatus, isLastSale }: DeleteSaleButtonProps) {
  const router = useRouter();
  const [state, formAction] = useActionState(
    deleteSaleAction.bind(null, saleId),
    null
  );

  const cannotDelete = orderStatus !== "Pending" || isLastSale;

  useEffect(() => {
    if (state?.success && state?.redirectTo) {
      router.push(state.redirectTo);
    }
  }, [state, router]);

  return (
    <div className="space-y-2">
      <form action={formAction}>
        <SubmitButton
          disabled={cannotDelete}
          orderStatus={orderStatus}
          isLastSale={isLastSale}
        />
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
