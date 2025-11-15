"use client";

import { useFormStatus } from "react-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface CustomerFormProps {
  action: (formData: FormData) => void;
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
  return (
    <Card>
      <CardHeader>
        <CardTitle>{isEdit ? "Edit Customer" : "Create New Customer"}</CardTitle>
      </CardHeader>
      <CardContent>
        <form action={action} className="space-y-4">
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
