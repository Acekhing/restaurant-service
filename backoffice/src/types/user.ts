export interface User {
  id: string;
  email: string;
  fullName: string | null;
  role: string | null;
  retailerId: string;
  retailerName: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  fullName?: string;
  role?: string;
  retailerId: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  role?: string;
  retailerId?: string;
  isActive?: boolean;
}

export interface ChangePasswordRequest {
  password: string;
}
