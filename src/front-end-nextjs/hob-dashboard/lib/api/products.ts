import { ApiClient } from "./client";
import { Product, CreateProductRequest, UpdateProductRequest } from "../types/product";
import { PaginatedResponse } from "../types/api";

const client = new ApiClient();

export async function getProducts(
  page: number = 1,
  pageSize: number = 20,
  search?: string,
  category?: string,
  lowStock?: boolean,
  activeOnly?: boolean
): Promise<PaginatedResponse<Product>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });

  if (search) params.append("search", search);
  if (category) params.append("category", category);
  if (lowStock !== undefined) params.append("lowStock", lowStock.toString());
  if (activeOnly !== undefined) params.append("activeOnly", activeOnly.toString());

  return client.get<PaginatedResponse<Product>>(
    `/api/products?${params.toString()}`,
    { cache: "no-store" }
  );
}

export async function getProduct(productId: string): Promise<Product> {
  return client.get<Product>(`/api/products/${productId}`, { cache: "no-store" });
}

export async function createProduct(data: CreateProductRequest): Promise<Product> {
  return client.post<Product>("/api/products", data);
}

export async function updateProduct(
  productId: string,
  data: UpdateProductRequest
): Promise<Product> {
  return client.put<Product>(`/api/products/${productId}`, data);
}

export async function deleteProduct(productId: string): Promise<void> {
  return client.delete(`/api/products/${productId}`);
}
