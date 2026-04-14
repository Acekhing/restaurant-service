import client from "./client";
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@/types/category";

export async function listCategories(params?: {
  ownerId?: string;
}): Promise<Category[]> {
  const { data } = await client.get<Category[]>("/categories", { params });
  return data;
}

export async function createCategory(
  body: CreateCategoryRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>("/categories", body);
  return data;
}

export async function updateCategory(
  id: string,
  body: UpdateCategoryRequest
): Promise<void> {
  await client.put(`/categories/${id}`, body);
}

export async function deleteCategory(id: string): Promise<void> {
  await client.delete(`/categories/${id}`);
}
