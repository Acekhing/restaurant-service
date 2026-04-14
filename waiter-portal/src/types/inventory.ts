export interface InventoryItem {
  id: string;
  name: string;
  shortName: string | null;
  itemType: string;
  displayPrice: number;
  displayCurrency: string;
  inventoryItemCode: string | null;
  isAvailable: boolean;
  outOfStock: boolean;
  image: string | null;
  retailerId: string;
}

export interface MenuInventoryItem {
  id: string;
  name: string;
  displayPrice: number;
  sortOrder: number;
  isRequired: boolean;
}

export interface Menu {
  id: string;
  description: string | null;
  ownerId: string;
  ownerName: string | null;
  categoryName: string | null;
  menuItemCode: string | null;
  isActive: boolean;
  displayCurrency: string;
  inventoryItems: MenuInventoryItem[] | null;
}

export interface Retailer {
  id: string;
  retailerType: string;
  businessName: string | null;
}
