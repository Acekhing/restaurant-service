import client from "./client";
import type {
  SearchResult,
  InventoryItem,
  Promotion,
  CreateItemRequest,
  CreatePromotionRequest,
  UpdatePromotionRequest,
  UpdatePriceRequest,
  UpdateAvailabilityRequest,
  UpdateInventoryItemCodeRequest,
} from "@/types/inventory";

export async function searchItems(params: {
  q?: string;
  itemType?: string;
  retailerType?: string;
  ownerId?: string;
  available?: boolean;
  page?: number;
  size?: number;
}): Promise<SearchResult> {
  const { data } = await client.get<SearchResult>("/inventory/search", {
    params,
  });
  return data;
}

export async function getItemFromES(id: string): Promise<InventoryItem> {
  const { data } = await client.get<InventoryItem>(
    `/inventory/search/${id}`
  );
  return data;
}

export async function createItem(
  body: CreateItemRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>(
    "/inventoryitems",
    body
  );
  return data;
}

export async function updatePrice(
  id: string,
  body: UpdatePriceRequest
): Promise<void> {
  await client.patch(`/inventoryitems/${id}/price`, body);
}

export async function updateAvailability(
  id: string,
  body: UpdateAvailabilityRequest
): Promise<void> {
  await client.patch(`/inventoryitems/${id}/availability`, body);
}

export async function getPromotions(ownerId: string): Promise<Promotion[]> {
  const { data } = await client.get<Promotion[]>("/promotions", {
    params: { ownerId },
  });
  return data;
}

export async function deleteItem(id: string): Promise<void> {
  await client.delete(`/inventoryitems/${id}`);
}

export async function createPromotion(
  body: CreatePromotionRequest
): Promise<{ promotionId: string }> {
  const { data } = await client.post<{ promotionId: string }>(
    "/promotions",
    body
  );
  return data;
}

export async function updatePromotion(
  id: string,
  body: UpdatePromotionRequest
): Promise<void> {
  await client.put(`/promotions/${id}`, body);
}

export async function deletePromotion(id: string): Promise<void> {
  await client.delete(`/promotions/${id}`);
}

export async function deactivatePromotion(id: string): Promise<void> {
  await client.patch(`/promotions/${id}/deactivate`);
}

export async function updateInventoryItemCode(
  id: string,
  body: UpdateInventoryItemCodeRequest
): Promise<void> {
  await client.patch(`/inventoryitems/${id}/inventory-item-code`, body);
}

export async function listItems(params?: {
  ownerId?: string;
  itemType?: string;
}): Promise<InventoryItem[]> {
  const { data } = await client.get<InventoryItem[]>("/inventoryitems", {
    params,
  });
  return data;
}

export async function generateInventoryItemCodes(): Promise<{ generated: number }> {
  const { data } = await client.post<{ generated: number }>(
    "/inventoryitems/generate-codes"
  );
  return data;
}
