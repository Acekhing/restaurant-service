export interface Category {
  id: string;
  name: string;
  ownerId: string;
}

export interface CreateCategoryRequest {
  name: string;
  ownerId: string;
}

export interface UpdateCategoryRequest {
  name: string;
}
