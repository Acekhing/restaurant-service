export interface AuditLogEntry {
  id: string;
  outboxId: string;
  aggregateId: string;
  aggregateType: string;
  eventType: string;
  actorId: string;
  beforeJson: string | null;
  afterJson: string | null;
  occurredAt: string;
  recordedAt: string;
}

export interface AuditSearchResult {
  items: AuditLogEntry[];
  total: number;
  page: number;
  size: number;
}
