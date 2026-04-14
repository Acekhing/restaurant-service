export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  email: string;
  fullName: string | null;
  role: string | null;
  retailerId: string;
  retailerName: string | null;
}
