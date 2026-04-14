import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useAuditLog } from "@/hooks/useAudit";
import AuditTable from "@/components/audit/AuditTable";
import AuditFilters from "@/components/audit/AuditFilters";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";

export default function AuditLogPage() {
  const [searchParams] = useSearchParams();
  const [aggregateId, setAggregateId] = useState(
    searchParams.get("aggregateId") ?? ""
  );
  const [eventType, setEventType] = useState("");
  const [aggregateType, setAggregateType] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 25;

  const { data, isLoading, isError } = useAuditLog({
    aggregateId: aggregateId || undefined,
    eventType: eventType || undefined,
    aggregateType: aggregateType || undefined,
    page,
    size: pageSize,
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Audit Log</h1>
        <p className="text-sm text-muted-foreground">
          {data
            ? `${data.total} entr${data.total !== 1 ? "ies" : "y"}`
            : "Track all inventory changes"}
        </p>
      </div>

      <AuditFilters
        aggregateId={aggregateId}
        onAggregateIdChange={(v) => {
          setAggregateId(v);
          setPage(1);
        }}
        eventType={eventType}
        onEventTypeChange={(v) => {
          setEventType(v);
          setPage(1);
        }}
        aggregateType={aggregateType}
        onAggregateTypeChange={(v) => {
          setAggregateType(v);
          setPage(1);
        }}
      />

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load audit log. Make sure the API is running.
        </p>
      )}

      {data && (
        <>
          <div className="rounded-lg border bg-background p-4">
            <AuditTable entries={data.items} />
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage(page - 1)}
              >
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(page + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
