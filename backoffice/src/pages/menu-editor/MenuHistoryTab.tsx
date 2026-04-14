import { useState } from "react";
import { History, ChevronRight } from "lucide-react";
import { useSearchMenus, useMenuHistory } from "@/hooks/useMenu";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { formatDate, cn } from "@/lib/utils";
import type { Menu } from "@/types/menu";

const statusConfig: Record<string, { dot: string; badge: string }> = {
  Created: { dot: "bg-emerald-500", badge: "bg-emerald-100 text-emerald-800" },
  Published: { dot: "bg-blue-500", badge: "bg-blue-100 text-blue-800" },
  Scheduled: { dot: "bg-amber-500", badge: "bg-amber-100 text-amber-800" },
  Edited: { dot: "bg-gray-400", badge: "bg-gray-100 text-gray-800" },
};

export default function MenuHistoryTab() {
  const { data, isLoading } = useSearchMenus({ size: 100 });
  const [expandedId, setExpandedId] = useState<string | null>(null);

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner />
      </div>
    );
  }

  const menus = data?.items ?? [];

  if (menus.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <History className="mb-4 h-12 w-12 text-muted-foreground/40" />
        <h2 className="text-lg font-semibold">No Menus</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          There are no menus to show history for.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <History className="h-5 w-5 text-muted-foreground" />
        <h2 className="text-lg font-semibold">Menu History</h2>
        <span className="text-sm text-muted-foreground">({menus.length})</span>
      </div>

      <div className="divide-y rounded-lg border">
        {menus.map((menu) => (
          <MenuHistoryRow
            key={menu.id}
            menu={menu}
            isExpanded={expandedId === menu.id}
            onToggle={() =>
              setExpandedId((prev) => (prev === menu.id ? null : menu.id))
            }
          />
        ))}
      </div>
    </div>
  );
}

function MenuHistoryRow({
  menu,
  isExpanded,
  onToggle,
}: {
  menu: Menu;
  isExpanded: boolean;
  onToggle: () => void;
}) {
  return (
    <div>
      <button
        type="button"
        onClick={onToggle}
        className="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-accent/50"
      >
        <ChevronRight
          className={cn(
            "h-4 w-4 shrink-0 text-muted-foreground transition-transform",
            isExpanded && "rotate-90"
          )}
        />
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className="truncate text-sm font-medium">
              {menu.categoryName ?? "Untitled Menu"}
            </span>
            <Badge
              className={cn(
                "shrink-0 text-[10px]",
                menu.isActive
                  ? "bg-green-100 text-green-800"
                  : "bg-gray-100 text-gray-800"
              )}
            >
              {menu.isActive ? "Active" : "Inactive"}
            </Badge>
            {menu.isPublished && (
              <Badge className="shrink-0 bg-blue-100 text-blue-800 text-[10px]">
                Published
              </Badge>
            )}
          </div>
          <p className="truncate text-xs text-muted-foreground">
            {menu.ownerName ?? menu.ownerId}
            {menu.description ? ` \u2014 ${menu.description}` : ""}
          </p>
        </div>
        <span className="shrink-0 text-xs text-muted-foreground">
          {formatDate(menu.updatedAt)}
        </span>
      </button>

      {isExpanded && <MenuHistoryTimeline menuId={menu.id} />}
    </div>
  );
}

function MenuHistoryTimeline({ menuId }: { menuId: string }) {
  const { data: history, isLoading } = useMenuHistory(menuId);

  if (isLoading) {
    return (
      <div className="flex justify-center py-6">
        <Spinner />
      </div>
    );
  }

  if (!history || history.length === 0) {
    return (
      <p className="px-11 pb-4 text-xs text-muted-foreground">
        No history recorded for this menu.
      </p>
    );
  }

  return (
    <div className="px-11 pb-4">
      <ol className="relative space-y-3 border-l border-border pl-4">
        {history.map((entry) => {
          const config = statusConfig[entry.status] ?? statusConfig.Edited;
          return (
            <li key={entry.id} className="relative">
              <span
                className={cn(
                  "absolute -left-[21px] top-1 h-2.5 w-2.5 rounded-full ring-2 ring-background",
                  config.dot
                )}
              />
              <Badge className={cn("text-[10px] px-1.5 py-0", config.badge)}>
                {entry.status}
              </Badge>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {formatDate(entry.date)}
              </p>
            </li>
          );
        })}
      </ol>
    </div>
  );
}
