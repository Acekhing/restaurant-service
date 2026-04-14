import client from "./client";
import type { Variety, CreateVarietyRequest, UpdateVarietyRequest } from "@/types/variety";

export async function createVariety(
  body: CreateVarietyRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>("/varieties", body);
  return data;
}

export async function updateVariety(
  id: string,
  body: UpdateVarietyRequest
): Promise<void> {
  await client.put(`/varieties/${id}`, body);
}

export async function getVariety(id: string): Promise<Variety> {
  const { data } = await client.get<Variety>(`/varieties/${id}`);
  return data;
}

export async function deleteVariety(id: string): Promise<void> {
  await client.delete(`/varieties/${id}`);
}

export async function listVarieties(params?: {
  ownerId?: string;
}): Promise<Variety[]> {
  const { data } = await client.get<Variety[]>("/varieties", { params });
  return data;
}
