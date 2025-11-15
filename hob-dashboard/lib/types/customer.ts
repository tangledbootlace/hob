export interface Customer {
  customerId: string;
  name: string;
  email: string;
  phone?: string;
  createdAt: string;
  updatedAt: string;
  orders: OrderSummary[];
}

export interface OrderSummary {
  orderId: string;
  orderDate: string;
  totalAmount: number;
  status: string;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  phone?: string;
}

export interface UpdateCustomerRequest {
  name: string;
  email: string;
  phone?: string;
}
