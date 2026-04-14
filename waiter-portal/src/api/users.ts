import client from "./client";
import type { LoginRequest, LoginResponse } from "@/types/user";

export async function login(body: LoginRequest): Promise<LoginResponse> {
  const { data } = await client.post<LoginResponse>("/users/login", body);
  return data;
}
