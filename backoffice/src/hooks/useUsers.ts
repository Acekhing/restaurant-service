import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  listUsers,
  getUser,
  createUser,
  updateUser,
  changePassword,
  deleteUser,
} from "@/api/users";
import type {
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
} from "@/types/user";

export function useUsers(retailerId?: string) {
  return useQuery({
    queryKey: ["users", retailerId],
    queryFn: () => listUsers(retailerId),
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: ["users", id],
    queryFn: () => getUser(id),
    enabled: !!id,
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateUserRequest) => createUser(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateUserRequest }) =>
      updateUser(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: ChangePasswordRequest }) =>
      changePassword(id, body),
  });
}

export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}
