import { useQuery } from "@tanstack/react-query";
import { listRetailers } from "@/api/inventory";

export function useRetailers() {
  return useQuery({
    queryKey: ["retailers"],
    queryFn: () => listRetailers(),
  });
}
