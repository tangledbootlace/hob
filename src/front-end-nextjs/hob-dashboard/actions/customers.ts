"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createCustomer, updateCustomer, deleteCustomer } from "@/lib/api/customers";
import { CreateCustomerRequest, UpdateCustomerRequest } from "@/lib/types/customer";

export type FormState = {
  success: boolean;
  message?: string;
  errors?: Record<string, string[]>;
  redirectTo?: string;
};

export async function createCustomerAction(
  prevState: FormState | null,
  formData: FormData
): Promise<FormState> {
  const data: CreateCustomerRequest = {
    name: formData.get("name") as string,
    email: formData.get("email") as string,
    phone: (formData.get("phone") as string) || undefined,
  };

  try {
    const customer = await createCustomer(data);
    revalidatePath("/customers");
    return {
      success: true,
      message: "Customer created successfully",
      redirectTo: `/customers/${customer.customerId}`,
    };
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "Failed to create customer",
    };
  }
}

export async function updateCustomerAction(
  customerId: string,
  prevState: FormState | null,
  formData: FormData
): Promise<FormState> {
  const data: UpdateCustomerRequest = {
    name: formData.get("name") as string,
    email: formData.get("email") as string,
    phone: (formData.get("phone") as string) || undefined,
  };

  try {
    await updateCustomer(customerId, data);
    revalidatePath(`/customers/${customerId}`);
    revalidatePath("/customers");
    return {
      success: true,
      message: "Customer updated successfully",
    };
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "Failed to update customer",
    };
  }
}

export async function deleteCustomerAction(
  customerId: string,
  prevState: FormState | null,
  formData: FormData
): Promise<FormState> {
  try {
    await deleteCustomer(customerId);
    revalidatePath("/customers");
    return {
      success: true,
      message: "Customer deleted successfully",
      redirectTo: "/customers",
    };
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "Failed to delete customer",
    };
  }
}
