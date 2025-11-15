"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { createOrder, updateOrder, deleteOrder } from "@/lib/api/orders";
import { CreateOrderRequest, UpdateOrderRequest } from "@/lib/types/order";

export async function createOrderAction(formData: FormData) {
  const customerId = formData.get("customerId") as string;
  const salesData = formData.get("sales") as string;
  const sales = JSON.parse(salesData);

  const data: CreateOrderRequest = {
    customerId,
    orderDate: new Date().toISOString(),
    sales,
  };

  try {
    const order = await createOrder(data);
    revalidatePath("/orders");
    redirect(`/orders/${order.orderId}`);
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to create order");
  }
}

export async function updateOrderAction(orderId: string, formData: FormData) {
  const data: UpdateOrderRequest = {
    status: formData.get("status") as string,
  };

  try {
    await updateOrder(orderId, data);
    revalidatePath(`/orders/${orderId}`);
    revalidatePath("/orders");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to update order");
  }
}

export async function deleteOrderAction(orderId: string) {
  try {
    await deleteOrder(orderId);
    revalidatePath("/orders");
    redirect("/orders");
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : "Failed to delete order");
  }
}
