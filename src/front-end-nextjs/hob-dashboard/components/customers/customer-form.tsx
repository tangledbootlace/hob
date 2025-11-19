"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { FormState } from "@/actions/customers";
import { AlertCircle, CheckCircle2 } from "lucide-react";

interface CustomerFormProps {
  action: (prevState: FormState | null, formData: FormData) => Promise<FormState>;
  defaultValues?: {
    name: string;
    email: string;
    phone?: string;
  };
  isEdit?: boolean;
}

function SubmitButton({ isEdit }: { isEdit?: boolean }) {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending}>
      {pending ? "Saving..." : isEdit ? "Update Customer" : "Create Customer"}
    </Button>
  );
}

export function CustomerForm({ action, defaultValues, isEdit }: CustomerFormProps) {
  const [state, formAction] = useActionState(action, null);
  const router = useRouter();

  useEffect(() => {
    if (state?.success && state?.redirectTo) {
      router.push(state.redirectTo);
    }
  }, [state, router]);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{isEdit ? "Edit Customer" : "Create New Customer"}</CardTitle>
      </CardHeader>
      <CardContent>
        <form action={formAction} className="space-y-4">
          {state?.message && (
            <Alert variant={state.success ? "default" : "destructive"}>
              {state.success ? (
                <CheckCircle2 className="h-4 w-4" />
              ) : (
                <AlertCircle className="h-4 w-4" />
              )}
              <AlertDescription>{state.message}</AlertDescription>
            </Alert>
          )}

          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input
              id="name"
              name="name"
              type="text"
              required
              defaultValue={defaultValues?.name}
              placeholder="John Doe"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email *</Label>
            <Input
              id="email"
              name="email"
              type="email"
              required
              defaultValue={defaultValues?.email}
              placeholder="john@example.com"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="phone">Phone</Label>
            <Input
              id="phone"
              name="phone"
              type="tel"
              defaultValue={defaultValues?.phone}
              placeholder="+1 (555) 123-4567"
            />
          </div>

          <div className="flex gap-2">
            <SubmitButton isEdit={isEdit} />
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
