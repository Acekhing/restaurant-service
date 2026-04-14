import client from "./client";
import type {
  Retailer,
  RetailerType,
  CreateRetailerRequest,
  UpdateRetailerRequest,
} from "@/types/retailer";

export async function listRetailers(type?: RetailerType): Promise<Retailer[]> {
  const { data } = await client.get<Retailer[]>("/retailers", {
    params: type ? { type } : undefined,
  });
  return data;
}

export async function getRetailer(id: string): Promise<Retailer> {
  const { data } = await client.get<Retailer>(`/retailers/${id}`);
  return data;
}

export async function createRetailer(
  body: CreateRetailerRequest
): Promise<Retailer> {
  const { data } = await client.post<Retailer>("/retailers", body);
  return data;
}

export async function updateRetailer(
  id: string,
  body: UpdateRetailerRequest
): Promise<void> {
  await client.put(`/retailers/${id}`, body);
}
