import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  listCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "@/api/category";
import type {
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@/types/category";

export function useCategories(ownerId: string) {
  return useQuery({
    queryKey: ["categories", ownerId],
    queryFn: () => listCategories({ ownerId }),
    enabled: ownerId.trim().length > 0,
  });
}

export function useCreateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateCategoryRequest) => createCategory(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["categories"] }),
  });
}

export function useUpdateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateCategoryRequest }) =>
      updateCategory(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["categories"] }),
  });
}

export function useDeleteCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteCategory(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["categories"] }),
  });
}
