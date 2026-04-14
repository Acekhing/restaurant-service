import client from "./client";
import type {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
} from "@/types/user";

export async function listUsers(retailerId?: string): Promise<User[]> {
  const { data } = await client.get<User[]>("/users", {
    params: retailerId ? { retailerId } : undefined,
  });
  return data;
}

export async function getUser(id: string): Promise<User> {
  const { data } = await client.get<User>(`/users/${id}`);
  return data;
}

export async function createUser(
  body: CreateUserRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>("/users", body);
  return data;
}

export async function updateUser(
  id: string,
  body: UpdateUserRequest
): Promise<void> {
  await client.put(`/users/${id}`, body);
}

export async function changePassword(
  id: string,
  body: ChangePasswordRequest
): Promise<void> {
  await client.patch(`/users/${id}/password`, body);
}

export async function deleteUser(id: string): Promise<void> {
  await client.delete(`/users/${id}`);
}
