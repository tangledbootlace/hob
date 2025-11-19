"use client";

import { useFormStatus } from "react-dom";
import { Product } from "@/lib/types/product";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface ProductFormProps {
  product?: Product;
  action: (formData: FormData) => Promise<void>;
}

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending}>
      {pending ? "Saving..." : "Save Product"}
    </Button>
  );
}

export function ProductForm({ product, action }: ProductFormProps) {
  return (
    <form action={action}>
      <Card>
        <CardHeader>
          <CardTitle>{product ? "Edit Product" : "New Product"}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="sku">SKU *</Label>
              <Input
                id="sku"
                name="sku"
                defaultValue={product?.sku}
                required
                placeholder="e.g., WGT-A-001"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="name">Product Name *</Label>
              <Input
                id="name"
                name="name"
                defaultValue={product?.name}
                required
                placeholder="e.g., Widget A"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <textarea
              id="description"
              name="description"
              defaultValue={product?.description || ""}
              className="w-full rounded-md border border-gray-300 px-3 py-2"
              rows={3}
              placeholder="Product description..."
            />
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label htmlFor="unitPrice">Unit Price *</Label>
              <Input
                id="unitPrice"
                name="unitPrice"
                type="number"
                step="0.01"
                min="0"
                defaultValue={product?.unitPrice}
                required
                placeholder="0.00"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="stockQuantity">Stock Quantity *</Label>
              <Input
                id="stockQuantity"
                name="stockQuantity"
                type="number"
                min="0"
                defaultValue={product?.stockQuantity}
                required
                placeholder="0"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="lowStockThreshold">Low Stock Threshold *</Label>
              <Input
                id="lowStockThreshold"
                name="lowStockThreshold"
                type="number"
                min="0"
                defaultValue={product?.lowStockThreshold}
                required
                placeholder="10"
              />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="category">Category</Label>
              <Input
                id="category"
                name="category"
                defaultValue={product?.category || ""}
                placeholder="e.g., Widgets, Gadgets"
              />
            </div>

            {product && (
              <div className="space-y-2">
                <Label htmlFor="isActive">Status *</Label>
                <select
                  id="isActive"
                  name="isActive"
                  defaultValue={product.isActive.toString()}
                  className="flex h-10 w-full rounded-md border border-[var(--border)] bg-[var(--background)] text-[var(--foreground)] px-3 py-2 text-sm ring-offset-[var(--background)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--ring)] focus-visible:ring-offset-2"
                >
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </select>
              </div>
            )}
          </div>

          <div className="flex gap-2">
            <SubmitButton />
          </div>
        </CardContent>
      </Card>
    </form>
  );
}
