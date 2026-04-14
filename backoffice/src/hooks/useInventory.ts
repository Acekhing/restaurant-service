import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  searchItems,
  getItemFromES,
  createItem,
  deleteItem,
  updatePrice,
  updateAvailability,
  updateInventoryItemCode,
  getPromotions,
  createPromotion,
  updatePromotion,
  deletePromotion,
  deactivatePromotion,
  listItems,
  generateInventoryItemCodes,
} from "@/api/inventory";
import type {
  CreateItemRequest,
  CreatePromotionRequest,
  UpdatePromotionRequest,
  UpdatePriceRequest,
  UpdateAvailabilityRequest,
  UpdateInventoryItemCodeRequest,
} from "@/types/inventory";

export function useSearchItems(params: {
  q?: string;
  itemType?: string;
  retailerType?: string;
  ownerId?: string;
  available?: boolean;
  page?: number;
  size?: number;
}) {
  return useQuery({
    queryKey: ["items", params],
    queryFn: () => searchItems(params),
  });
}

export function useItem(id: string) {
  return useQuery({
    queryKey: ["item", id],
    queryFn: () => getItemFromES(id),
    enabled: !!id,
  });
}

export function usePromotions(ownerId: string) {
  return useQuery({
    queryKey: ["promotions", ownerId],
    queryFn: () => getPromotions(ownerId),
    enabled: !!ownerId,
  });
}

export function useCreateItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateItemRequest) => createItem(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["items"] }),
  });
}

export function useDeleteItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteItem(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["items"] }),
  });
}

export function useUpdatePrice(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdatePriceRequest) => updatePrice(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["item", id] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useUpdateAvailability(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateAvailabilityRequest) =>
      updateAvailability(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["item", id] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useUpdateInventoryItemCode(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateInventoryItemCodeRequest) =>
      updateInventoryItemCode(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["item", id] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useCreatePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreatePromotionRequest) => createPromotion(body),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ["promotions", variables.ownerId] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useUpdatePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdatePromotionRequest }) =>
      updatePromotion(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["promotions"] });
    },
  });
}

export function useDeletePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deletePromotion(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["promotions"] });
    },
  });
}

export function useDeactivatePromotion() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deactivatePromotion(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["promotions"] });
    },
  });
}

export function useListItems(ownerId: string) {
  return useQuery({
    queryKey: ["items-list", ownerId],
    queryFn: () => listItems({ ownerId }),
    enabled: ownerId.trim().length > 0,
  });
}

export function useGenerateInventoryItemCodes() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => generateInventoryItemCodes(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["items-list"] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}
