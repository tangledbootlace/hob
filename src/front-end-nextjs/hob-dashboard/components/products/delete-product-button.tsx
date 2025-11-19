"use client";

import { useActionState, useEffect } from "react";
import { useFormStatus } from "react-dom";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { deleteProductAction, FormState } from "@/actions/products";
import { Trash2, AlertCircle } from "lucide-react";

interface DeleteProductButtonProps {
  productId: string;
  hasSales: boolean;
  salesCount: number;
}

function SubmitButton({ disabled, hasSales, salesCount }: { disabled: boolean; hasSales: boolean; salesCount: number }) {
  const { pending } = useFormStatus();

  return (
    <div>
      <Button
        variant="destructive"
        type="submit"
        disabled={disabled || pending}
        title={hasSales ? `Cannot delete product with ${salesCount} existing sale(s)` : "Delete product"}
      >
        <Trash2 className="h-4 w-4 mr-2" />
        {pending ? "Deleting..." : "Delete"}
      </Button>
      {hasSales && (
        <p className="text-xs text-[var(--muted-foreground)] mt-1">
          Cannot delete: product has {salesCount} sale{salesCount !== 1 ? "s" : ""}. Consider marking it as inactive.
        </p>
      )}
    </div>
  );
}

export function DeleteProductButton({ productId, hasSales, salesCount }: DeleteProductButtonProps) {
  const router = useRouter();
  const [state, formAction] = useActionState(
    deleteProductAction.bind(null, productId),
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
        <SubmitButton disabled={hasSales} hasSales={hasSales} salesCount={salesCount} />
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
