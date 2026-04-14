import client from "./client";
import type {
  Menu,
  MenuSearchResult,
  MenuHistory,
  CreateMenuRequest,
  UpdateMenuRequest,
  AddMenuItemsRequest,
  SortMenuItemsEntry,
  UpdateMenuItemCodeRequest,
} from "@/types/menu";

export async function searchMenus(params: {
  q?: string;
  ownerId?: string;
  active?: boolean;
  page?: number;
  size?: number;
}): Promise<MenuSearchResult> {
  const { data } = await client.get<MenuSearchResult>("/menus/search", {
    params,
  });
  return data;
}

export async function getMenuFromES(id: string): Promise<Menu> {
  const { data } = await client.get<Menu>(`/menus/search/${id}`);
  return data;
}

export async function createMenu(
  body: CreateMenuRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>("/menus", body);
  return data;
}

export async function updateMenu(
  id: string,
  body: UpdateMenuRequest
): Promise<void> {
  await client.patch(`/menus/${id}`, body);
}

export async function deleteMenu(id: string): Promise<void> {
  await client.delete(`/menus/${id}`);
}

export async function addMenuItems(
  id: string,
  body: AddMenuItemsRequest
): Promise<{ added: number }> {
  const { data } = await client.post<{ added: number }>(
    `/menus/${id}/items`,
    body
  );
  return data;
}

export async function removeMenuItem(
  menuId: string,
  inventoryItemId: string
): Promise<void> {
  await client.delete(`/menus/${menuId}/items/${inventoryItemId}`);
}

export async function sortMenuItems(
  menuId: string,
  items: SortMenuItemsEntry[]
): Promise<void> {
  await client.patch(`/menus/search/${menuId}/sort-items`, items);
}

export async function listMenus(params?: {
  ownerId?: string;
}): Promise<Menu[]> {
  const { data } = await client.get<Menu[]>("/menus", { params });
  return data;
}

export async function updateMenuItemCode(
  id: string,
  body: UpdateMenuItemCodeRequest
): Promise<void> {
  await client.patch(`/menus/${id}/menu-item-code`, body);
}

export async function getMenuHistory(
  menuId: string
): Promise<MenuHistory[]> {
  const { data } = await client.get<MenuHistory[]>(
    `/menus/${menuId}/history`
  );
  return data;
}

export async function generateMenuCodes(): Promise<{ generated: number }> {
  const { data } = await client.post<{ generated: number }>(
    "/menus/generate-codes"
  );
  return data;
}
