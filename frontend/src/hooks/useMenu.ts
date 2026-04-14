import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  searchMenus,
  getMenuFromES,
  createMenu,
  updateMenu,
  deleteMenu,
  addMenuItems,
  removeMenuItem,
  sortMenuItems,
  listMenus,
  getMenuHistory,
  updateMenuItemCode,
  generateMenuCodes,
} from "@/api/menu";
import type {
  CreateMenuRequest,
  UpdateMenuRequest,
  AddMenuItemsRequest,
  SortMenuItemsEntry,
  UpdateMenuItemCodeRequest,
} from "@/types/menu";

export function useSearchMenus(params: {
  q?: string;
  ownerId?: string;
  active?: boolean;
  page?: number;
  size?: number;
}) {
  return useQuery({
    queryKey: ["menus", params],
    queryFn: () => searchMenus(params),
  });
}

export function useMenu(id: string) {
  return useQuery({
    queryKey: ["menu", id],
    queryFn: () => getMenuFromES(id),
    enabled: !!id,
  });
}

export function useCreateMenu() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateMenuRequest) => createMenu(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["menus"] }),
  });
}

export function useUpdateMenu(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateMenuRequest) => updateMenu(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menu", id] });
      qc.invalidateQueries({ queryKey: ["menus"] });
      qc.invalidateQueries({ queryKey: ["menu", id, "history"] });
    },
  });
}

export function useDeleteMenu(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => deleteMenu(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menus"] });
    },
  });
}

export function useAddMenuItems(menuId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: AddMenuItemsRequest) => addMenuItems(menuId, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menu", menuId] });
      qc.invalidateQueries({ queryKey: ["menus"] });
      qc.invalidateQueries({ queryKey: ["menu", menuId, "history"] });
    },
  });
}

export function useRemoveMenuItem(menuId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (inventoryItemId: string) =>
      removeMenuItem(menuId, inventoryItemId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menu", menuId] });
      qc.invalidateQueries({ queryKey: ["menus"] });
      qc.invalidateQueries({ queryKey: ["menu", menuId, "history"] });
    },
  });
}

export function useSortMenuItems(menuId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (items: SortMenuItemsEntry[]) =>
      sortMenuItems(menuId, items),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menu", menuId] });
      qc.invalidateQueries({ queryKey: ["menus"] });
    },
  });
}

export function useUpdateMenuItemCode(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: UpdateMenuItemCodeRequest) =>
      updateMenuItemCode(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menu", id] });
      qc.invalidateQueries({ queryKey: ["menus"] });
    },
  });
}

export function useMenusByOwner(ownerId: string) {
  return useQuery({
    queryKey: ["menus", "byOwner", ownerId],
    queryFn: () => listMenus({ ownerId }),
    enabled: ownerId.trim().length > 0,
  });
}

export function useMenuHistory(menuId: string) {
  return useQuery({
    queryKey: ["menu", menuId, "history"],
    queryFn: () => getMenuHistory(menuId),
    enabled: !!menuId,
  });
}

export function useGenerateMenuCodes() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => generateMenuCodes(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["menus"] });
    },
  });
}
