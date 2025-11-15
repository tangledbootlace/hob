import { apiClient } from "./client";
import { Sale, CreateSaleRequest, UpdateSaleRequest } from "../types/sale";
import { PaginatedResponse } from "../types/api";

export async function getSales(
  page = 1,
  pageSize = 20,
  orderId?: string,
  productNameSearch?: string
): Promise<PaginatedResponse<Sale>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (orderId) params.append("orderId", orderId);
  if (productNameSearch) params.append("productNameSearch", productNameSearch);

  return apiClient.get<PaginatedResponse<Sale>>(`/api/sales?${params.toString()}`, {
    cache: "no-store",
  });
}

export async function getSale(saleId: string): Promise<Sale> {
  return apiClient.get<Sale>(`/api/sales/${saleId}`, {
    cache: "no-store",
  });
}

export async function createSale(data: CreateSaleRequest): Promise<Sale> {
  return apiClient.post<Sale>("/api/sales", data);
}

export async function updateSale(saleId: string, data: UpdateSaleRequest): Promise<Sale> {
  return apiClient.put<Sale>(`/api/sales/${saleId}`, data);
}

export async function deleteSale(saleId: string): Promise<void> {
  return apiClient.delete(`/api/sales/${saleId}`);
}
