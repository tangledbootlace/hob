export interface Sale {
  saleId: string;
  orderId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  createdAt: string;
  updatedAt: string;
  orderDetails?: {
    orderDate: string;
    customerName: string;
    status: string;
  };
}

export interface CreateSaleRequest {
  orderId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface UpdateSaleRequest {
  quantity: number;
  unitPrice: number;
}
