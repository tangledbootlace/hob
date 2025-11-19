"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createProduct, updateProduct, deleteProduct } from "@/lib/api/products";
import { CreateProductRequest, UpdateProductRequest } from "@/lib/types/product";

export type FormState = {
  success: boolean;
  message?: string;
  errors?: Record<string, string[]>;
  redirectTo?: string;
};

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

export async function deleteProductAction(
  productId: string,
  prevState: FormState | null,
  formData: FormData
): Promise<FormState> {
  try {
    await deleteProduct(productId);
    revalidatePath("/products");
    return {
      success: true,
      message: "Product deleted successfully",
      redirectTo: "/products",
    };
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "Failed to delete product",
    };
  }
}
