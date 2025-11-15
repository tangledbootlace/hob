"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createCustomer, updateCustomer, deleteCustomer } from "@/lib/api/customers";
import { CreateCustomerRequest, UpdateCustomerRequest } from "@/lib/types/customer";

export async function createCustomerAction(formData: FormData) {
  const data: CreateCustomerRequest = {
    name: formData.get("name") as string,
    email: formData.get("email") as string,
    phone: (formData.get("phone") as string) || undefined,
  };

  try {
    const customer = await createCustomer(data);
    revalidatePath("/customers");
    redirect(`/customers/${customer.customerId}`);
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to create customer");
  }
}

export async function updateCustomerAction(customerId: string, formData: FormData) {
  const data: UpdateCustomerRequest = {
    name: formData.get("name") as string,
    email: formData.get("email") as string,
    phone: (formData.get("phone") as string) || undefined,
  };

  try {
    await updateCustomer(customerId, data);
    revalidatePath(`/customers/${customerId}`);
    revalidatePath("/customers");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to update customer");
  }
}

export async function deleteCustomerAction(customerId: string) {
  try {
    await deleteCustomer(customerId);
    revalidatePath("/customers");
    redirect("/customers");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to delete customer");
  }
}
