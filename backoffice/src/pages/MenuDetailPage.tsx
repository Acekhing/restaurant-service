import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Trash2, Plus, X, ArrowUp, ArrowDown, History, ChevronRight, Globe, CalendarClock, QrCode } from "lucide-react";
import { useMenu, useDeleteMenu, useUpdateMenu, useAddMenuItems, useRemoveMenuItem, useSortMenuItems, useMenuHistory, useUpdateMenuItemCode } from "@/hooks/useMenu";
import { useSearchItems } from "@/hooks/useInventory";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency, formatDate, cn } from "@/lib/utils";
import type { MenuInventoryItem } from "@/types/menu";

type Tab = "overview";

export default function MenuDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: menu, isLoading, isError } = useMenu(id!);
  const deleteMutation = useDeleteMenu(id!);
  const updateMutation = useUpdateMenu(id!);
  const [tab, setTab] = useState<Tab>("overview");

  if (isLoading)
    return (
      <div className="flex justify-center py-20">
        <Spinner />
      </div>
    );

  if (isError || !menu)
    return (
      <div className="py-20 text-center">
        <p className="text-muted-foreground">Menu not found.</p>
        <Link to="/menus" className="mt-2 inline-block text-sm underline">
          Back to menus
        </Link>
      </div>
    );

  const handleDelete = () => {
    if (!confirm("Are you sure you want to delete this menu?")) return;
    deleteMutation.mutate(undefined, {
      onSuccess: () => navigate("/menus"),
    });
  };

  const items: MenuInventoryItem[] = menu.inventoryItems ?? [];

  const tabs: { key: Tab; label: string }[] = [
    { key: "overview", label: "Overview" },
  ];

  return (
    <div className="mx-auto max-w-6xl">
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_280px]">
        <div className="space-y-6">
          <div className="flex items-start justify-between">
            <div className="flex items-start gap-3">
              <Link to="/menus">
                <Button variant="ghost" size="icon" className="mt-0.5">
                  <ArrowLeft className="h-4 w-4" />
                </Button>
              </Link>
              <div>
                <div className="flex items-center gap-2">
                  <h1 className="text-2xl font-bold">{menu.categoryName ?? "Untitled Menu"}</h1>
                  <Badge
                    className={
                      menu.isActive
                        ? "bg-green-100 text-green-800"
                        : "bg-gray-100 text-gray-800"
                    }
                  >
                    {menu.isActive ? "Active" : "Inactive"}
                  </Badge>
                  {menu.isPublished && (
                    <Badge className="bg-blue-100 text-blue-800">Published</Badge>
                  )}
                  {menu.isScheduled && (
                    <Badge className="bg-amber-100 text-amber-800">Scheduled</Badge>
                  )}
                </div>
                <p className="text-sm text-muted-foreground">
                  {menu.ownerName ?? menu.ownerId}
                  {menu.description && ` \u2014 ${menu.description}`}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant={menu.isScheduled ? "outline" : "secondary"}
                size="sm"
                onClick={() =>
                  updateMutation.mutate({ isScheduled: !menu.isScheduled })
                }
                disabled={updateMutation.isPending}
              >
                <CalendarClock className="h-3.5 w-3.5" />
                {menu.isScheduled ? "Unschedule" : "Schedule"}
              </Button>
              <Button
                size="sm"
                variant={menu.isPublished ? "outline" : "default"}
                onClick={() =>
                  updateMutation.mutate({
                    isPublished: !menu.isPublished,
                    ...(!menu.isPublished && { publishedAt: new Date().toISOString() }),
                  })
                }
                disabled={updateMutation.isPending}
              >
                <Globe className="h-3.5 w-3.5" />
                {menu.isPublished ? "Unpublish" : "Publish"}
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="text-destructive hover:text-destructive"
                onClick={handleDelete}
                disabled={deleteMutation.isPending}
              >
                <Trash2 className="h-3.5 w-3.5" />
                Delete
              </Button>
            </div>
          </div>

          <div className="border-b">
            <nav className="flex gap-6">
              {tabs.map((t) => (
                <button
                  key={t.key}
                  onClick={() => setTab(t.key)}
                  className={cn(
                    "border-b-2 pb-2 text-sm font-medium transition-colors",
                    tab === t.key
                      ? "border-primary text-foreground"
                      : "border-transparent text-muted-foreground hover:text-foreground"
                  )}
                >
                  {t.label}
                </button>
              ))}
            </nav>
          </div>

          {tab === "overview" && (
            <OverviewTab menuId={id!} items={items} menu={menu} />
          )}
        </div>

        <MenuHistoryPanel menuId={id!} />
      </div>
    </div>
  );
}

function MenuItemCodeSection({ menuId, currentCode }: { menuId: string; currentCode: string | null }) {
  const [editing, setEditing] = useState(false);
  const [code, setCode] = useState(currentCode ?? "");
  const mutation = useUpdateMenuItemCode(menuId);

  const handleSave = () => {
    mutation.mutate(
      { menuItemCode: code.trim() || null },
      { onSuccess: () => setEditing(false) }
    );
  };

  return (
    <div className="rounded-lg border p-5 space-y-3">
      <div className="flex items-center gap-2">
        <QrCode className="h-4 w-4 text-muted-foreground" />
        <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Menu Item Code
        </h3>
      </div>
      {editing ? (
        <div className="space-y-2">
          <input
            type="text"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="e.g. MENU-001"
            className="w-full rounded-md border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
          {mutation.isError && (
            <p className="text-xs text-destructive">
              {(mutation.error as any)?.response?.data?.error ?? "Failed to update code."}
            </p>
          )}
          <div className="flex gap-2">
            <Button size="sm" disabled={mutation.isPending} onClick={handleSave}>
              Save
            </Button>
            <Button size="sm" variant="outline" onClick={() => { setEditing(false); setCode(currentCode ?? ""); }}>
              Cancel
            </Button>
          </div>
        </div>
      ) : (
        <div className="flex items-center justify-between">
          <span className="font-mono text-sm">
            {currentCode ?? <span className="text-muted-foreground italic">Not set</span>}
          </span>
          <Button size="sm" variant="outline" onClick={() => setEditing(true)}>
            {currentCode ? "Edit" : "Set Code"}
          </Button>
        </div>
      )}
    </div>
  );
}

function OverviewTab({
  menuId,
  items,
  menu,
}: {
  menuId: string;
  items: MenuInventoryItem[];
  menu: import("@/types/menu").Menu;
}) {
  const addMutation = useAddMenuItems(menuId);
  const removeMutation = useRemoveMenuItem(menuId);
  const sortMutation = useSortMenuItems(menuId);
  const [showDetails, setShowDetails] = useState(false);
  const [showPicker, setShowPicker] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const { data: searchData } = useSearchItems({
    q: searchQuery || undefined,
    size: 8,
  });

  const existingIds = new Set(items.map((i) => i.id));

  const handleAddItem = (inventoryItemId: string) => {
    addMutation.mutate(
      { items: [{ inventoryItemId, isRequired: false }] },
      { onSuccess: () => setShowPicker(false) }
    );
  };

  const handleMove = (index: number, direction: "up" | "down") => {
    const sorted = [...items].sort((a, b) => a.sortOrder - b.sortOrder);
    const swapIdx = direction === "up" ? index - 1 : index + 1;
    if (swapIdx < 0 || swapIdx >= sorted.length) return;

    const updated = sorted.map((item, i) => {
      if (i === index) return { id: item.id, sortOrder: swapIdx };
      if (i === swapIdx) return { id: item.id, sortOrder: index };
      return { id: item.id, sortOrder: i };
    });

    sortMutation.mutate(updated);
  };

  return (
    <div className="space-y-6">
      <div className="rounded-lg border">
        <button
          type="button"
          onClick={() => setShowDetails((v) => !v)}
          className="flex w-full items-center gap-2 p-5 text-left"
        >
          <ChevronRight
            className={cn(
              "h-4 w-4 text-muted-foreground transition-transform",
              showDetails && "rotate-90"
            )}
          />
          <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Details
          </h3>
        </button>
        {showDetails && (
          <div className="space-y-2 border-t px-5 pb-5 pt-3">
            <Row label="Owner" value={menu.ownerName ?? menu.ownerId} />
            {menu.categoryName && <Row label="Category" value={menu.categoryName} />}
            <Row label="Items" value={String(menu.inventoryItems?.length ?? 0)} />
            <Row label="Currency" value={menu.displayCurrency} />
            <Row label="Published" value={menu.isPublished ? "Yes" : "No"} />
            {menu.publishedAt && <Row label="Published At" value={formatDate(menu.publishedAt)} />}
            <Row label="Created" value={formatDate(menu.createdAt)} />
            <Row label="Updated" value={formatDate(menu.updatedAt)} />
          </div>
        )}
      </div>

      <MenuItemCodeSection menuId={menuId} currentCode={menu.menuItemCode ?? null} />

      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Menu Items ({items.length})
          </h3>
          <Button size="sm" variant="outline" onClick={() => setShowPicker(!showPicker)}>
            <Plus className="h-3.5 w-3.5" />
            Add Item
          </Button>
        </div>

        {showPicker && (
          <div className="rounded-lg border p-4 space-y-3">
            <div className="relative">
              <Input
                placeholder="Search items to add..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-3"
              />
            </div>
            <div className="max-h-48 space-y-1 overflow-y-auto">
              {searchData?.items
                .filter((i) => !existingIds.has(i.id))
                .map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between rounded-md px-2 py-1.5 text-sm hover:bg-accent"
                  >
                    <div>
                      <p className="font-medium">{item.name}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatCurrency(item.displayPrice, item.displayCurrency)}
                      </p>
                    </div>
                    <Button
                      size="sm"
                      variant="ghost"
                      disabled={addMutation.isPending}
                      onClick={() => handleAddItem(item.id)}
                    >
                      <Plus className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                ))}
            </div>
          </div>
        )}

        {items.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">
            No items in this menu yet. Click "Add Item" to get started.
          </p>
        ) : (
          <div className="space-y-2">
            {[...items]
              .sort((a, b) => a.sortOrder - b.sortOrder)
              .map((item, index) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between rounded-lg border px-4 py-3"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex flex-col gap-0.5">
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-5 w-5"
                        disabled={index === 0 || sortMutation.isPending}
                        onClick={() => handleMove(index, "up")}
                      >
                        <ArrowUp className="h-3 w-3" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-5 w-5"
                        disabled={index === items.length - 1 || sortMutation.isPending}
                        onClick={() => handleMove(index, "down")}
                      >
                        <ArrowDown className="h-3 w-3" />
                      </Button>
                    </div>
                    <div>
                      <div className="flex items-center gap-1.5">
                        <Link
                          to={`/items/${item.id}`}
                          className="text-sm font-medium hover:underline"
                        >
                          {item.name}
                        </Link>
                        {item.isRequired && (
                          <Badge className="bg-violet-100 text-violet-800 text-[10px] px-1.5 py-0">
                            Required
                          </Badge>
                        )}
                        {item.variety && (
                          <Badge className="bg-teal-100 text-teal-800 text-[10px] px-1.5 py-0">
                            {item.variety.name}
                          </Badge>
                        )}
                      </div>
                      <p className="text-xs text-muted-foreground">
                        {formatCurrency(item.displayPrice, menu.displayCurrency)}
                      </p>
                      {item.variety && item.variety.options.length > 0 && (
                        <div className="mt-1 flex flex-wrap gap-1">
                          {item.variety.options.map((opt, idx) => (
                            <span
                              key={idx}
                              className="inline-flex items-center gap-1 rounded bg-muted px-1.5 py-0.5 text-[10px] text-muted-foreground"
                            >
                              {opt.name}
                              <span className="font-medium">
                                {formatCurrency(opt.price, menu.displayCurrency)}
                              </span>
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 text-muted-foreground hover:text-destructive"
                    disabled={removeMutation.isPending}
                    onClick={() => removeMutation.mutate(item.id)}
                  >
                    <X className="h-3.5 w-3.5" />
                  </Button>
                </div>
              ))}
          </div>
        )}
      </div>
    </div>
  );
}

const statusConfig: Record<string, { dot: string; badge: string }> = {
  Created: { dot: "bg-emerald-500", badge: "bg-emerald-100 text-emerald-800" },
  Published: { dot: "bg-blue-500", badge: "bg-blue-100 text-blue-800" },
  Scheduled: { dot: "bg-amber-500", badge: "bg-amber-100 text-amber-800" },
  Edited: { dot: "bg-gray-400", badge: "bg-gray-100 text-gray-800" },
};

function MenuHistoryPanel({ menuId }: { menuId: string }) {
  const { data: history, isLoading } = useMenuHistory(menuId);

  return (
    <div className="space-y-3 rounded-lg border p-5 lg:sticky lg:top-6 self-start">
      <div className="flex items-center gap-2">
        <History className="h-4 w-4 text-muted-foreground" />
        <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          History
        </h3>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-6">
          <Spinner />
        </div>
      ) : !history || history.length === 0 ? (
        <p className="py-4 text-center text-xs text-muted-foreground">
          No history yet.
        </p>
      ) : (
        <ol className="relative space-y-4 border-l border-border pl-4">
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
      )}
    </div>
  );
}

function Row({
  label,
  value,
}: {
  label: string;
  value: React.ReactNode;
}) {
  return (
    <div className="flex items-center justify-between text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  );
}
