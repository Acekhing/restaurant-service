import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  listRetailers,
  getRetailer,
  createRetailer,
  updateRetailer,
} from "@/api/retailer";
import type {
  RetailerType,
  CreateRetailerRequest,
  UpdateRetailerRequest,
} from "@/types/retailer";

export function useRetailers(type?: RetailerType) {
  return useQuery({
    queryKey: ["retailers", type],
    queryFn: () => listRetailers(type),
  });
}

export function useRetailer(id: string) {
  return useQuery({
    queryKey: ["retailers", id],
    queryFn: () => getRetailer(id),
    enabled: !!id,
  });
}

export function useCreateRetailer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateRetailerRequest) => createRetailer(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["retailers"] }),
  });
}

export function useUpdateRetailer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateRetailerRequest }) =>
      updateRetailer(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["retailers"] }),
  });
}
