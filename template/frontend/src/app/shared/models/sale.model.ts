export interface SaleItem {
  id?: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount?: number;
  totalAmount?: number;
  isCancelled?: boolean;
}

export interface Sale {
  id: string;
  saleNumber: number;
  saleDate: string;
  customerId: string;
  customerName: string;
  branchId: string;
  branchName: string;
  totalAmount: number;
  isCancelled: boolean;
  createdAt?: string;
  updatedAt?: string;
  items: SaleItem[];
}

export interface CreateSaleRequest {
  saleDate: string;
  customerId: string;
  customerName: string;
  branchId: string;
  branchName: string;
  items: Omit<SaleItem, 'id' | 'discount' | 'totalAmount' | 'isCancelled'>[];
}

export interface UpdateSaleRequest {
  saleDate: string;
  customerId: string;
  customerName: string;
  branchId: string;
  branchName: string;
  items: (Omit<SaleItem, 'discount' | 'totalAmount' | 'isCancelled'> & { id?: string })[];
}

export interface SalesListParams {
  page: number;
  size: number;
  order?: string;
  customerName?: string;
  branchName?: string;
  isCancelled?: boolean;
  saleNumber?: number;
}
