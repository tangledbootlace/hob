"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createProduct, updateProduct, deleteProduct } from "@/lib/api/products";
import { CreateProductRequest, UpdateProductRequest } from "@/lib/types/product";

export async function createProductAction(formData: FormData) {
  const data: CreateProductRequest = {
    sku: formData.get("sku") as string,
    name: formData.get("name") as string,
    description: (formData.get("description") as string) || undefined,
    unitPrice: parseFloat(formData.get("unitPrice") as string),
    stockQuantity: parseInt(formData.get("stockQuantity") as string, 10),
    lowStockThreshold: parseInt(formData.get("lowStockThreshold") as string, 10),
    category: (formData.get("category") as string) || undefined,
  };

  try {
    const product = await createProduct(data);
    revalidatePath("/products");
    redirect(`/products/${product.productId}`);
  } catch (error) {
    console.error("Failed to create product:", error);
    throw error;
  }
}

export async function updateProductAction(productId: string, formData: FormData) {
  const data: UpdateProductRequest = {
    sku: formData.get("sku") as string,
    name: formData.get("name") as string,
    description: (formData.get("description") as string) || undefined,
    unitPrice: parseFloat(formData.get("unitPrice") as string),
    stockQuantity: parseInt(formData.get("stockQuantity") as string, 10),
    lowStockThreshold: parseInt(formData.get("lowStockThreshold") as string, 10),
    category: (formData.get("category") as string) || undefined,
    isActive: formData.get("isActive") === "true",
  };

  try {
    await updateProduct(productId, data);
    revalidatePath("/products");
    revalidatePath(`/products/${productId}`);
    redirect(`/products/${productId}`);
  } catch (error) {
    console.error("Failed to update product:", error);
    throw error;
  }
}

export async function deleteProductAction(productId: string) {
  try {
    await deleteProduct(productId);
    revalidatePath("/products");
    redirect("/products");
  } catch (error) {
    console.error("Failed to delete product:", error);
    throw error;
  }
}
