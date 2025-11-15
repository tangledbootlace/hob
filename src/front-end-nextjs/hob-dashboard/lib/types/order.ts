export interface Order {
  orderId: string;
  customerId: string;
  customerName: string;
  orderDate: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  updatedAt: string;
  sales: SaleSummary[];
}

export interface SaleSummary {
  saleId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreateOrderRequest {
  customerId: string;
  orderDate?: string;
  sales: CreateSaleItem[];
}

export interface CreateSaleItem {
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface UpdateOrderRequest {
  status: string;
}
