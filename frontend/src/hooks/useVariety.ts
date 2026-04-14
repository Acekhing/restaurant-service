import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { createVariety, updateVariety, deleteVariety, getVariety, listVarieties } from "@/api/variety";
import { listItems } from "@/api/inventory";
import type { CreateVarietyRequest, UpdateVarietyRequest } from "@/types/variety";

export function useVarieties(params?: { ownerId?: string }) {
  return useQuery({
    queryKey: ["varieties", params],
    queryFn: () => listVarieties(params),
  });
}

export function useVariety(id: string) {
  return useQuery({
    queryKey: ["variety", id],
    queryFn: () => getVariety(id),
    enabled: !!id,
  });
}

export function useCreateVariety() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateVarietyRequest) => createVariety(body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["varieties"] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useUpdateVariety() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateVarietyRequest }) =>
      updateVariety(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["varieties"] });
      qc.invalidateQueries({ queryKey: ["variety"] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useDeleteVariety() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteVariety(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["varieties"] });
      qc.invalidateQueries({ queryKey: ["variety"] });
      qc.invalidateQueries({ queryKey: ["items"] });
    },
  });
}

export function useInventoryItemsByOwner(ownerId: string) {
  return useQuery({
    queryKey: ["items", "byOwner", ownerId],
    queryFn: () => listItems({ ownerId }),
    enabled: ownerId.trim().length > 0,
  });
}
