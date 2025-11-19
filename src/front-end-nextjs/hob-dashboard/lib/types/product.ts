export interface Product {
  productId: string;
  sku: string;
  name: string;
  description?: string;
  unitPrice: number;
  stockQuantity: number;
  lowStockThreshold: number;
  category?: string;
  isActive: boolean;
  isLowStock: boolean;
  createdAt: string;
  updatedAt: string;
  salesCount?: number;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  unitPrice: number;
  stockQuantity: number;
  lowStockThreshold: number;
  category?: string;
}

export interface UpdateProductRequest {
  sku: string;
  name: string;
  description?: string;
  unitPrice: number;
  stockQuantity: number;
  lowStockThreshold: number;
  category?: string;
  isActive: boolean;
}
