import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { listBranches, createBranch } from "@/api/branch";
import type { CreateBranchRequest } from "@/types/retailer";

export function useBranches(retailerId: string) {
  return useQuery({
    queryKey: ["branches", retailerId],
    queryFn: () => listBranches(retailerId),
    enabled: !!retailerId,
  });
}

export function useCreateBranch() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateBranchRequest) => createBranch(body),
    onSuccess: (_data, variables) =>
      qc.invalidateQueries({
        queryKey: ["branches", variables.retailerId],
      }),
  });
}
