import client from "./client";
import type { Branch, CreateBranchRequest } from "@/types/retailer";

export async function listBranches(retailerId: string): Promise<Branch[]> {
  const { data } = await client.get<Branch[]>("/branches", {
    params: { retailerId },
  });
  return data;
}

export async function createBranch(
  body: CreateBranchRequest
): Promise<Branch> {
  const { data } = await client.post<Branch>("/branches", body);
  return data;
}
