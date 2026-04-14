export interface InventoryOpeningHours {
  id: string;
  day: string;
  openingTime: string;
  closingTime: string;
  isAvailable: boolean;
}

export interface InventoryItem {
  id: string;
  name: string;
  shortName: string | null;
  itemType: string;
  tags: string | null;
  notes: string | null;
  image: string | null;
  rawImageUrl: string | null;
  isOriginalImage: boolean;
  displayPrice: number;
  supplierPrice: number | null;
  oldSellingPrice: number | null;
  deliveryFee: number;
  priceRange: string | null;
  hasDeals: boolean;
  displayCurrency: string;
  averagePreparationTime: number | null;
  inventoryItemCode: string | null;
  isAvailable: boolean;
  outOfStock: boolean;
  openingDayHours: InventoryOpeningHours[] | null;
  displayTimes: InventoryOpeningHours[] | null;
  retailerId: string;
  retailerType: string;
  hasVariety: boolean;
  variety: string | null;
  stationId: string | null;
  zoneId: string | null;
}

export interface SearchResult {
  items: InventoryItem[];
  total: number;
  page: number;
  size: number;
}

export interface Promotion {
  id: string;
  ownerId: string;
  discountInPercentage: number;
  currency: string;
  effectiveFrom: string;
  effectiveTo: string;
  isActive: boolean;
  inventoryItemIds: string[] | null;
  menuIds: string[] | null;
  isAppliedToMenu: boolean;
  isAppliedToItems: boolean;
  isFreeDelivery: boolean;
  createdAt: string;
}

export interface CreateItemRequest {
  name: string;
  ownerId: string;
  supplierPrice: number;
  deliveryFee: number;
  itemType: string;
  averagePreparationTime?: number;
  openingDayHours?: InventoryOpeningHours[];
}

export interface CreatePromotionRequest {
  ownerId: string;
  discountInPercentage: number;
  currency?: string;
  effectiveFrom: string;
  effectiveTo: string;
  inventoryItemIds?: string[];
  menuIds?: string[];
  isAppliedToMenu: boolean;
  isAppliedToItems: boolean;
  isFreeDelivery: boolean;
}

export interface UpdatePromotionRequest {
  discountInPercentage: number;
  currency?: string;
  effectiveFrom: string;
  effectiveTo: string;
  inventoryItemIds?: string[];
  menuIds?: string[];
  isAppliedToMenu: boolean;
  isAppliedToItems: boolean;
  isFreeDelivery: boolean;
}

export interface UpdatePriceRequest {
  displayPrice: number;
}

export interface UpdateAvailabilityRequest {
  isAvailable: boolean;
  outOfStock?: boolean;
  isHidden?: boolean;
}

export interface UpdateInventoryItemCodeRequest {
  inventoryItemCode: string | null;
}
