import client from "./client";
import type { AuditLogEntry, AuditSearchResult } from "@/types/audit";

export async function searchAuditLog(params: {
  aggregateId?: string;
  eventType?: string;
  aggregateType?: string;
  page?: number;
  size?: number;
}): Promise<AuditSearchResult> {
  const { data } = await client.get<AuditSearchResult>("/audit-log", {
    params,
  });
  return data;
}

export async function getAuditByAggregateId(
  aggregateId: string
): Promise<AuditLogEntry[]> {
  const { data } = await client.get<AuditLogEntry[]>(
    `/audit-log/${aggregateId}`
  );
  return data;
}
