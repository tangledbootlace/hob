"use client";

import { useState } from "react";
import { useFormStatus } from "react-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Plus, Trash2 } from "lucide-react";
import { Customer } from "@/lib/types/customer";

interface OrderFormProps {
  action: (formData: FormData) => void;
  customers: Customer[];
}

interface SaleItem {
  productName: string;
  quantity: number;
  unitPrice: number;
}

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending}>
      {pending ? "Creating..." : "Create Order"}
    </Button>
  );
}

export function OrderForm({ action, customers }: OrderFormProps) {
  const [sales, setSales] = useState<SaleItem[]>([
    { productName: "", quantity: 1, unitPrice: 0 },
  ]);

  const addSaleItem = () => {
    setSales([...sales, { productName: "", quantity: 1, unitPrice: 0 }]);
  };

  const removeSaleItem = (index: number) => {
    setSales(sales.filter((_, i) => i !== index));
  };

  const updateSaleItem = (index: number, field: keyof SaleItem, value: string | number) => {
    const newSales = [...sales];
    newSales[index] = { ...newSales[index], [field]: value };
    setSales(newSales);
  };

  const handleSubmit = (formData: FormData) => {
    formData.set("sales", JSON.stringify(sales));
    action(formData);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Create New Order</CardTitle>
      </CardHeader>
      <CardContent>
        <form action={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="customerId">Customer *</Label>
            <select
              id="customerId"
              name="customerId"
              required
              className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm"
            >
              <option value="">Select a customer</option>
              {customers.map((customer) => (
                <option key={customer.customerId} value={customer.customerId}>
                  {customer.name} ({customer.email})
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label>Sales Items *</Label>
              <Button type="button" variant="outline" size="sm" onClick={addSaleItem}>
                <Plus className="h-4 w-4 mr-2" />
                Add Item
              </Button>
            </div>

            {sales.map((sale, index) => (
              <div key={index} className="grid grid-cols-12 gap-2 items-end">
                <div className="col-span-5 space-y-2">
                  <Label htmlFor={`product-${index}`}>Product Name</Label>
                  <Input
                    id={`product-${index}`}
                    type="text"
                    required
                    value={sale.productName}
                    onChange={(e) => updateSaleItem(index, "productName", e.target.value)}
                    placeholder="Product name"
                  />
                </div>
                <div className="col-span-2 space-y-2">
                  <Label htmlFor={`qty-${index}`}>Quantity</Label>
                  <Input
                    id={`qty-${index}`}
                    type="number"
                    required
                    min="1"
                    value={sale.quantity}
                    onChange={(e) => updateSaleItem(index, "quantity", Number(e.target.value))}
                  />
                </div>
                <div className="col-span-3 space-y-2">
                  <Label htmlFor={`price-${index}`}>Unit Price</Label>
                  <Input
                    id={`price-${index}`}
                    type="number"
                    required
                    min="0"
                    step="0.01"
                    value={sale.unitPrice}
                    onChange={(e) => updateSaleItem(index, "unitPrice", Number(e.target.value))}
                  />
                </div>
                <div className="col-span-2">
                  {sales.length > 1 && (
                    <Button
                      type="button"
                      variant="destructive"
                      size="icon"
                      onClick={() => removeSaleItem(index)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </div>
              </div>
            ))}
          </div>

          <div className="flex gap-2">
            <SubmitButton />
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
