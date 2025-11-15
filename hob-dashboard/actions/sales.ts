"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createSale, updateSale, deleteSale } from "@/lib/api/sales";
import { CreateSaleRequest, UpdateSaleRequest } from "@/lib/types/sale";

export async function createSaleAction(formData: FormData) {
  const data: CreateSaleRequest = {
    orderId: formData.get("orderId") as string,
    productName: formData.get("productName") as string,
    quantity: Number(formData.get("quantity")),
    unitPrice: Number(formData.get("unitPrice")),
  };

  try {
    const sale = await createSale(data);
    revalidatePath("/sales");
    revalidatePath(`/orders/${data.orderId}`);
    redirect(`/sales/${sale.saleId}`);
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to create sale");
  }
}

export async function updateSaleAction(saleId: string, formData: FormData) {
  const data: UpdateSaleRequest = {
    quantity: Number(formData.get("quantity")),
    unitPrice: Number(formData.get("unitPrice")),
  };

  try {
    await updateSale(saleId, data);
    revalidatePath(`/sales/${saleId}`);
    revalidatePath("/sales");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to update sale");
  }
}

export async function deleteSaleAction(saleId: string) {
  try {
    await deleteSale(saleId);
    revalidatePath("/sales");
    redirect("/sales");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to delete sale");
  }
}
