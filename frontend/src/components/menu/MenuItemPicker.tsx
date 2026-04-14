import { useState } from "react";
import { Search, Plus, X } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useSearchItems } from "@/hooks/useInventory";
import { formatCurrency } from "@/lib/utils";
import type { InventoryItem } from "@/types/inventory";

interface MenuItemPickerProps {
  selectedItems: InventoryItem[];
  requiredItemIds: Set<string>;
  onAdd: (item: InventoryItem) => void;
  onRemove: (itemId: string) => void;
  onToggleRequired: (itemId: string) => void;
}

export default function MenuItemPicker({
  selectedItems,
  requiredItemIds,
  onAdd,
  onRemove,
  onToggleRequired,
}: MenuItemPickerProps) {
  const [query, setQuery] = useState("");
  const { data, isLoading } = useSearchItems({
    q: query || undefined,
    size: 10,
  });

  const selectedIds = new Set(selectedItems.map((i) => i.id));

  return (
    <div className="grid gap-4 sm:grid-cols-2">
      <div className="space-y-3">
        <h4 className="text-sm font-medium">Search Items</h4>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search inventory..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="max-h-64 space-y-1 overflow-y-auto rounded-lg border p-2">
          {isLoading && (
            <div className="flex justify-center py-4">
              <Spinner />
            </div>
          )}
          {data?.items.map((item) => (
            <div
              key={item.id}
              className="flex items-center justify-between rounded-md px-2 py-1.5 text-sm hover:bg-accent"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">{item.name}</p>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(item.displayPrice, item.displayCurrency)}
                </p>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="ml-2 h-7 w-7 shrink-0"
                disabled={selectedIds.has(item.id)}
                onClick={() => onAdd(item)}
              >
                <Plus className="h-3.5 w-3.5" />
              </Button>
            </div>
          ))}
          {data && data.items.length === 0 && (
            <p className="py-4 text-center text-xs text-muted-foreground">
              No items found.
            </p>
          )}
        </div>
      </div>

      <div className="space-y-3">
        <h4 className="text-sm font-medium">
          Selected ({selectedItems.length})
        </h4>
        <div className="max-h-80 space-y-1 overflow-y-auto rounded-lg border p-2">
          {selectedItems.length === 0 && (
            <p className="py-8 text-center text-xs text-muted-foreground">
              No items selected yet. Search and add items from the left.
            </p>
          )}
          {selectedItems.map((item, idx) => (
            <div
              key={item.id}
              className="flex items-center justify-between rounded-md bg-muted/50 px-2 py-1.5 text-sm"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">
                  <span className="mr-1.5 text-xs text-muted-foreground">
                    {idx + 1}.
                  </span>
                  {item.name}
                </p>
                <div className="flex items-center gap-2 mt-0.5">
                  <p className="text-xs text-muted-foreground">
                    {formatCurrency(item.displayPrice, item.displayCurrency)}
                  </p>
                  <label className="flex items-center gap-1 text-xs text-muted-foreground cursor-pointer">
                    <input
                      type="checkbox"
                      className="h-3 w-3 rounded border-gray-300"
                      checked={requiredItemIds.has(item.id)}
                      onChange={() => onToggleRequired(item.id)}
                    />
                    Required
                  </label>
                </div>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="ml-2 h-7 w-7 shrink-0 text-muted-foreground hover:text-destructive"
                onClick={() => onRemove(item.id)}
              >
                <X className="h-3.5 w-3.5" />
              </Button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
