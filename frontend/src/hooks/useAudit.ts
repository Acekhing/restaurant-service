import { useQuery } from "@tanstack/react-query";
import { searchAuditLog, getAuditByAggregateId } from "@/api/audit";

export function useAuditLog(params: {
  aggregateId?: string;
  eventType?: string;
  aggregateType?: string;
  page?: number;
  size?: number;
}) {
  return useQuery({
    queryKey: ["auditLog", params],
    queryFn: () => searchAuditLog(params),
  });
}

export function useItemAuditLog(aggregateId: string) {
  return useQuery({
    queryKey: ["auditLog", "item", aggregateId],
    queryFn: () => getAuditByAggregateId(aggregateId),
    enabled: !!aggregateId,
  });
}
