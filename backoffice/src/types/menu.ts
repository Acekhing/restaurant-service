export interface MenuItemVarietyOption {
  name: string;
  price: number;
}

export interface MenuItemVariety {
  id: string;
  name: string;
  options: MenuItemVarietyOption[];
}

export interface MenuInventoryItem {
  id: string;
  name: string;
  displayPrice: number;
  sortOrder: number;
  isRequired: boolean;
  variety: MenuItemVariety | null;
}

export interface MenuInventoryInput {
  inventoryItemId: string;
  isRequired: boolean;
}

export interface Menu {
  id: string;
  description: string | null;
  ownerId: string;
  ownerName: string | null;
  ownerImage: string | null;
  image: string | null;
  isActive: boolean;
  displayCurrency: string;

  categoryId: string | null;
  categoryName: string | null;
  menuItemCode: string | null;
  isPublished: boolean;
  isScheduled: boolean;
  publishedAt: string | null;

  inventoryItems: MenuInventoryItem[] | null;
  priceRange: string | null;

  createdAt: string;
  updatedAt: string;
}

export interface MenuSearchResult {
  items: Menu[];
  total: number;
  page: number;
  size: number;
}

export interface CreateMenuRequest {
  description?: string;
  ownerId: string;
  image?: string;
  displayCurrency?: string;
  categoryId: string;
  isPublished?: boolean;
  isScheduled?: boolean;
  publishedAt?: string;
  inventoryItems?: MenuInventoryInput[];
}

export interface UpdateMenuRequest {
  description?: string;
  image?: string;
  isActive?: boolean;
  categoryId?: string;
  isPublished?: boolean;
  isScheduled?: boolean;
  publishedAt?: string;
  inventoryItems?: MenuInventoryInput[];
}

export interface AddMenuItemsRequest {
  items: MenuInventoryInput[];
}

export interface SortMenuItemsEntry {
  id: string;
  sortOrder: number;
}

export interface MenuHistory {
  id: string;
  menuId: string;
  ownerId: string;
  status: string;
  date: string;
}

export interface UpdateMenuItemCodeRequest {
  menuItemCode: string | null;
}
