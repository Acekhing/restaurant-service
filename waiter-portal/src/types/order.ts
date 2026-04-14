export interface OrderLine {
  id: string;
  orderId: string;
  inventoryItemId: string;
  itemName: string;
  unitPrice: number;
  quantity: number;
  notes: string | null;
  varietySelection: string | null;
}

export interface Order {
  id: string;
  orderNumber: string;
  retailerId: string;
  waiterName: string | null;
  tableNumber: string | null;
  customerNotes: string | null;
  customerPhone: string | null;
  status: string;
  totalAmount: number;
  displayCurrency: string;
  lines: OrderLine[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderLineRequest {
  inventoryItemId: string;
  itemName: string;
  unitPrice: number;
  quantity: number;
  notes?: string;
  varietySelection?: string;
}

export interface CreateOrderRequest {
  retailerId: string;
  waiterName?: string;
  tableNumber?: string;
  customerNotes?: string;
  customerPhone?: string;
  displayCurrency?: string;
  lines: CreateOrderLineRequest[];
}

export interface CreateOrderResponse {
  id: string;
  orderNumber: string;
}

export type OrderStatus =
  | "Pending"
  | "Confirmed"
  | "Preparing"
  | "Ready"
  | "Served"
  | "Cancelled";
