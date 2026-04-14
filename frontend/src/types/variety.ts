export interface VarietyData {
  name: string;
  price: number;
}

export interface VarietyItemRef {
  id: string;
  name: string;
}

export interface Variety {
  id: string;
  name: string;
  inventoryItemIds: string[];
  inventoryItems: VarietyItemRef[];
  varieties: VarietyData[];
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateVarietyRequest {
  name: string;
  inventoryItemIds: string[];
  varieties: VarietyData[];
  ownerId: string;
}

export interface UpdateVarietyRequest {
  name?: string;
  inventoryItemIds?: string[];
  varieties?: VarietyData[];
}
