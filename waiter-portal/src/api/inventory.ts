import client from "./client";
import type { InventoryItem, Menu, Retailer } from "@/types/inventory";

export async function getItemByCode(code: string): Promise<InventoryItem> {
  const { data } = await client.get<InventoryItem>(
    `/inventoryitems/by-code/${encodeURIComponent(code)}`
  );
  return data;
}

export async function getMenuByCode(code: string): Promise<Menu> {
  const { data } = await client.get<Menu>(
    `/menus/by-code/${encodeURIComponent(code)}`
  );
  return data;
}

export async function listRetailers(): Promise<Retailer[]> {
  const { data } = await client.get<Retailer[]>("/retailers");
  return data;
}
