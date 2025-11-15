import { apiClient } from "./client";
import { Order, CreateOrderRequest, UpdateOrderRequest } from "../types/order";
import { PaginatedResponse } from "../types/api";

export async function getOrders(
  page = 1,
  pageSize = 20,
  customerId?: string,
  status?: string,
  startDate?: string,
  endDate?: string
): Promise<PaginatedResponse<Order>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (customerId) params.append("customerId", customerId);
  if (status) params.append("status", status);
  if (startDate) params.append("startDate", startDate);
  if (endDate) params.append("endDate", endDate);

  return apiClient.get<PaginatedResponse<Order>>(`/api/orders?${params.toString()}`, {
    cache: "no-store",
  });
}

export async function getOrder(orderId: string): Promise<Order> {
  return apiClient.get<Order>(`/api/orders/${orderId}`, {
    cache: "no-store",
  });
}

export async function createOrder(data: CreateOrderRequest): Promise<Order> {
  return apiClient.post<Order>("/api/orders", data);
}

export async function updateOrder(orderId: string, data: UpdateOrderRequest): Promise<Order> {
  return apiClient.put<Order>(`/api/orders/${orderId}`, data);
}

export async function deleteOrder(orderId: string): Promise<void> {
  return apiClient.delete(`/api/orders/${orderId}`);
}
