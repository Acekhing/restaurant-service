import { useState } from "react";
import { ChevronDown, ChevronRight } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { formatDate } from "@/lib/utils";
import type { AuditLogEntry } from "@/types/audit";

function eventBadgeColor(eventType: string) {
  if (eventType.includes("Created")) return "bg-green-100 text-green-800";
  if (eventType.includes("Updated") || eventType.includes("Changed"))
    return "bg-blue-100 text-blue-800";
  if (eventType.includes("Deleted")) return "bg-red-100 text-red-800";
  return "bg-gray-100 text-gray-800";
}

function JsonDiff({
  before,
  after,
}: {
  before: string | null;
  after: string | null;
}) {
  let beforeObj: Record<string, unknown> | null = null;
  let afterObj: Record<string, unknown> | null = null;
  try {
    if (before) beforeObj = JSON.parse(before);
  } catch {
    /* ignore */
  }
  try {
    if (after) afterObj = JSON.parse(after);
  } catch {
    /* ignore */
  }

  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div>
        <p className="mb-1 text-xs font-medium text-muted-foreground">
          Before
        </p>
        <pre className="max-h-60 overflow-auto rounded-md bg-muted p-3 text-xs">
          {beforeObj ? JSON.stringify(beforeObj, null, 2) : "—"}
        </pre>
      </div>
      <div>
        <p className="mb-1 text-xs font-medium text-muted-foreground">After</p>
        <pre className="max-h-60 overflow-auto rounded-md bg-muted p-3 text-xs">
          {afterObj ? JSON.stringify(afterObj, null, 2) : "—"}
        </pre>
      </div>
    </div>
  );
}

export default function AuditTable({ entries }: { entries: AuditLogEntry[] }) {
  const [expandedId, setExpandedId] = useState<string | null>(null);

  if (entries.length === 0)
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No audit entries found.
      </p>
    );

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-muted-foreground">
            <th className="w-8 pb-2"></th>
            <th className="pb-2 pr-4 font-medium">Timestamp</th>
            <th className="pb-2 pr-4 font-medium">Type</th>
            <th className="pb-2 pr-4 font-medium">Event</th>
            <th className="pb-2 font-medium">Actor</th>
          </tr>
        </thead>
        <tbody>
          {entries.map((entry) => (
            <>
              <tr
                key={entry.id}
                className="cursor-pointer border-b hover:bg-muted/50"
                onClick={() =>
                  setExpandedId(expandedId === entry.id ? null : entry.id)
                }
              >
                <td className="py-3 pr-1">
                  {expandedId === entry.id ? (
                    <ChevronDown className="h-4 w-4 text-muted-foreground" />
                  ) : (
                    <ChevronRight className="h-4 w-4 text-muted-foreground" />
                  )}
                </td>
                <td className="py-3 pr-4 whitespace-nowrap text-muted-foreground">
                  {formatDate(entry.occurredAt)}
                </td>
                <td className="py-3 pr-4">{entry.aggregateType}</td>
                <td className="py-3 pr-4">
                  <Badge className={eventBadgeColor(entry.eventType)}>
                    {entry.eventType}
                  </Badge>
                </td>
                <td className="py-3 text-muted-foreground">
                  {entry.actorId || "system"}
                </td>
              </tr>
              {expandedId === entry.id && (
                <tr key={`${entry.id}-detail`}>
                  <td colSpan={5} className="border-b bg-muted/30 p-4">
                    <JsonDiff
                      before={entry.beforeJson}
                      after={entry.afterJson}
                    />
                  </td>
                </tr>
              )}
            </>
          ))}
        </tbody>
      </table>
    </div>
  );
}
