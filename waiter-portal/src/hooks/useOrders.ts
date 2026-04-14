import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createOrder,
  getOrder,
  listOrders,
  updateOrderStatus,
} from "@/api/orders";
import type { CreateOrderRequest } from "@/types/order";

export function useOrder(id: string) {
  return useQuery({
    queryKey: ["order", id],
    queryFn: () => getOrder(id),
    enabled: !!id,
  });
}

export function useOrders(params: {
  retailerId?: string;
  status?: string;
  date?: string;
}) {
  return useQuery({
    queryKey: ["orders", params],
    queryFn: () => listOrders(params),
    enabled: !!params.retailerId,
    refetchInterval: 15_000,
  });
}

export function useCreateOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateOrderRequest) => createOrder(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["orders"] }),
  });
}

export function useUpdateOrderStatus(orderId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (status: string) => updateOrderStatus(orderId, status),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["order", orderId] });
      qc.invalidateQueries({ queryKey: ["orders"] });
    },
  });
}
