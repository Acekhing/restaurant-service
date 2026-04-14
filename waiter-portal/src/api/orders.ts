import client from "./client";
import type {
  Order,
  CreateOrderRequest,
  CreateOrderResponse,
} from "@/types/order";

export async function createOrder(
  body: CreateOrderRequest
): Promise<CreateOrderResponse> {
  const { data } = await client.post<CreateOrderResponse>("/orders", body);
  return data;
}

export async function getOrder(id: string): Promise<Order> {
  const { data } = await client.get<Order>(`/orders/${id}`);
  return data;
}

export async function listOrders(params: {
  retailerId?: string;
  status?: string;
  date?: string;
}): Promise<Order[]> {
  const { data } = await client.get<Order[]>("/orders", { params });
  return data;
}

export async function updateOrderStatus(
  id: string,
  status: string
): Promise<void> {
  await client.patch(`/orders/${id}/status`, { status });
}
