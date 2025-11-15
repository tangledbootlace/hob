import { apiClient } from "./client";
import { Customer, CreateCustomerRequest, UpdateCustomerRequest } from "../types/customer";
import { PaginatedResponse } from "../types/api";

export async function getCustomers(
  page = 1,
  pageSize = 20,
  search?: string
): Promise<PaginatedResponse<Customer>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (search) {
    params.append("search", search);
  }
  return apiClient.get<PaginatedResponse<Customer>>(`/api/customers?${params.toString()}`, {
    cache: "no-store",
  });
}

export async function getCustomer(customerId: string): Promise<Customer> {
  return apiClient.get<Customer>(`/api/customers/${customerId}`, {
    cache: "no-store",
  });
}

export async function createCustomer(data: CreateCustomerRequest): Promise<Customer> {
  return apiClient.post<Customer>("/api/customers", data);
}

export async function updateCustomer(
  customerId: string,
  data: UpdateCustomerRequest
): Promise<Customer> {
  return apiClient.put<Customer>(`/api/customers/${customerId}`, data);
}

export async function deleteCustomer(customerId: string): Promise<void> {
  return apiClient.delete(`/api/customers/${customerId}`);
}
