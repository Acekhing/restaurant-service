import { useState, useEffect } from "react";
import { Plus, Trash2, Layers, Check, Search, ChevronDown, Pencil, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import {
  useVarieties,
  useCreateVariety,
  useUpdateVariety,
  useDeleteVariety,
  useInventoryItemsByOwner,
} from "@/hooks/useVariety";
import type { Variety, VarietyData } from "@/types/variety";

function ItemPicker({
  ownerId,
  selectedIds,
  onToggle,
}: {
  ownerId: string;
  selectedIds: Set<string>;
  onToggle: (id: string) => void;
}) {
  const { data: items, isLoading, isError } = useInventoryItemsByOwner(ownerId);
  const [filter, setFilter] = useState("");

  if (!ownerId.trim()) {
    return (
      <p className="text-sm text-muted-foreground">
        Enter an Owner ID above to load items.
      </p>
    );
  }

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 py-4">
        <Spinner className="h-4 w-4" />
        <span className="text-sm text-muted-foreground">Loading items...</span>
      </div>
    );
  }

  if (isError) {
    return (
      <p className="text-sm text-destructive">Failed to load items for this owner.</p>
    );
  }

  if (!items || items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No inventory items found for this owner.
      </p>
    );
  }

  const filtered = filter.trim()
    ? items.filter((i) =>
        i.name.toLowerCase().includes(filter.toLowerCase())
      )
    : items;

  return (
    <div className="space-y-2">
      <div className="relative">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          placeholder="Filter items..."
          className="pl-9"
        />
      </div>
      <div className="max-h-52 space-y-1 overflow-y-auto rounded-md border p-1">
        {filtered.map((item) => {
          const selected = selectedIds.has(item.id);
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onToggle(item.id)}
              className={`flex w-full items-center gap-3 rounded-md px-3 py-2 text-left text-sm transition-colors ${
                selected
                  ? "bg-primary/10 text-primary"
                  : "hover:bg-muted/50"
              }`}
            >
              <span
                className={`flex h-5 w-5 shrink-0 items-center justify-center rounded border ${
                  selected
                    ? "border-primary bg-primary text-primary-foreground"
                    : "border-muted-foreground/30"
                }`}
              >
                {selected && <Check className="h-3.5 w-3.5" />}
              </span>
              <span className="flex-1 truncate font-medium">{item.name}</span>
              <span className="shrink-0 text-xs text-muted-foreground">
                {item.displayCurrency} {item.displayPrice.toFixed(2)}
              </span>
            </button>
          );
        })}
        {filtered.length === 0 && (
          <p className="py-3 text-center text-sm text-muted-foreground">
            No items match your filter.
          </p>
        )}
      </div>
      {selectedIds.size > 0 && (
        <p className="text-xs text-muted-foreground">
          {selectedIds.size} item{selectedIds.size !== 1 ? "s" : ""} selected
        </p>
      )}
    </div>
  );
}

function VarietyForm({
  mode,
  initial,
  onDone,
}: {
  mode: "create" | "edit";
  initial?: Variety;
  onDone: () => void;
}) {
  const createMutation = useCreateVariety();
  const updateMutation = useUpdateVariety();
  const isPending = createMutation.isPending || updateMutation.isPending;
  const isError = createMutation.isError || updateMutation.isError;

  const [name, setName] = useState(initial?.name ?? "");
  const [ownerId, setOwnerId] = useState(initial?.ownerId ?? "");
  const [selectedItemIds, setSelectedItemIds] = useState<Set<string>>(
    new Set(initial?.inventoryItemIds ?? [])
  );
  const [varieties, setVarieties] = useState<VarietyData[]>(
    initial?.varieties?.length ? initial.varieties : [{ name: "", price: 0 }]
  );

  useEffect(() => {
    if (initial) {
      setName(initial.name);
      setOwnerId(initial.ownerId);
      setSelectedItemIds(new Set(initial.inventoryItemIds));
      setVarieties(initial.varieties?.length ? initial.varieties : [{ name: "", price: 0 }]);
    }
  }, [initial]);

  const toggleItem = (id: string) =>
    setSelectedItemIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const addEntry = () => setVarieties((v) => [...v, { name: "", price: 0 }]);

  const removeEntry = (idx: number) =>
    setVarieties((v) => v.filter((_, i) => i !== idx));

  const updateEntry = (idx: number, field: keyof VarietyData, value: string) =>
    setVarieties((v) =>
      v.map((entry, i) =>
        i === idx
          ? {
              ...entry,
              [field]: field === "price" ? parseFloat(value) || 0 : value,
            }
          : entry
      )
    );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedItemIds.size === 0) return;

    const cleanVarieties = varieties.filter((v) => v.name.trim() !== "");

    if (mode === "edit" && initial) {
      updateMutation.mutate(
        {
          id: initial.id,
          body: {
            name,
            inventoryItemIds: [...selectedItemIds],
            varieties: cleanVarieties,
          },
        },
        { onSuccess: onDone }
      );
    } else {
      createMutation.mutate(
        {
          name,
          inventoryItemIds: [...selectedItemIds],
          ownerId,
          varieties: cleanVarieties,
        },
        {
          onSuccess: () => {
            setName("");
            setOwnerId("");
            setSelectedItemIds(new Set());
            setVarieties([{ name: "", price: 0 }]);
            onDone();
          },
        }
      );
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-5 rounded-lg border bg-background p-6"
    >
      <h2 className="text-lg font-semibold">
        {mode === "edit" ? "Edit Variety" : "Create Variety"}
      </h2>

      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="v-name" className="mb-1.5 block text-sm font-medium">
            Name
          </label>
          <Input
            id="v-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Size Options"
            required
          />
        </div>
        <div>
          <label
            htmlFor="v-ownerId"
            className="mb-1.5 block text-sm font-medium"
          >
            Owner ID
          </label>
          <Input
            id="v-ownerId"
            value={ownerId}
            onChange={(e) => {
              setOwnerId(e.target.value);
              if (mode === "create") setSelectedItemIds(new Set());
            }}
            placeholder="e.g. rest-001"
            required
            disabled={mode === "edit"}
          />
        </div>
      </div>

      <div>
        <label className="mb-1.5 block text-sm font-medium">
          Inventory Items
        </label>
        <ItemPicker
          ownerId={ownerId}
          selectedIds={selectedItemIds}
          onToggle={toggleItem}
        />
      </div>

      <div>
        <div className="mb-2 flex items-center justify-between">
          <label className="text-sm font-medium">Variety Options</label>
          <Button type="button" variant="outline" size="sm" onClick={addEntry}>
            <Plus className="h-3.5 w-3.5" />
            Add Option
          </Button>
        </div>

        <div className="space-y-2">
          {varieties.map((entry, idx) => (
            <div key={idx} className="flex items-center gap-3">
              <Input
                value={entry.name}
                onChange={(e) => updateEntry(idx, "name", e.target.value)}
                placeholder="Option name"
                className="flex-1"
              />
              <Input
                type="number"
                min="0"
                step="0.01"
                value={entry.price || ""}
                onChange={(e) => updateEntry(idx, "price", e.target.value)}
                placeholder="Price"
                className="w-32"
              />
              {varieties.length > 1 && (
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeEntry(idx)}
                >
                  <Trash2 className="h-4 w-4 text-destructive" />
                </Button>
              )}
            </div>
          ))}
        </div>
      </div>

      {isError && (
        <p className="text-sm text-destructive">
          Failed to {mode === "edit" ? "update" : "create"} variety. Please try again.
        </p>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button
          type="button"
          variant="outline"
          onClick={onDone}
        >
          Cancel
        </Button>
        <Button
          type="submit"
          disabled={isPending || selectedItemIds.size === 0}
        >
          {isPending && <Spinner className="h-4 w-4" />}
          {mode === "edit" ? "Save Changes" : "Create Variety"}
        </Button>
      </div>
    </form>
  );
}

function VarietyRow({
  variety,
  onEdit,
  onDelete,
  isDeleting,
}: {
  variety: Variety;
  onEdit: (v: Variety) => void;
  onDelete: (id: string) => void;
  isDeleting: boolean;
}) {
  const [open, setOpen] = useState(false);
  const hasOptions = variety.varieties.length > 0;
  const itemNames = (variety.inventoryItems ?? []).map((i) => i.name);

  return (
    <li className="border-b last:border-b-0">
      <button
        type="button"
        onClick={() => hasOptions && setOpen((o) => !o)}
        className={`flex w-full items-center gap-3 px-4 py-3 text-left transition-colors ${
          hasOptions ? "cursor-pointer hover:bg-muted/40" : "cursor-default"
        }`}
      >
        <div className="min-w-0 flex-1">
          <span className="font-medium">{variety.name}</span>
          <span className="ml-2 text-xs text-muted-foreground">
            {variety.inventoryItemIds.length} item
            {variety.inventoryItemIds.length !== 1 ? "s" : ""}
          </span>
        </div>

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onEdit(variety);
          }}
          className="shrink-0 rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
          title="Edit variety"
        >
          <Pencil className="h-3.5 w-3.5" />
        </button>

        <button
          type="button"
          disabled={isDeleting}
          onClick={(e) => {
            e.stopPropagation();
            if (confirm(`Delete variety "${variety.name}"? This cannot be undone.`)) {
              onDelete(variety.id);
            }
          }}
          className="shrink-0 rounded p-1 text-muted-foreground hover:bg-destructive/10 hover:text-destructive transition-colors disabled:opacity-50"
          title="Delete variety"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>

        <span className="shrink-0 text-xs text-muted-foreground">
          {new Date(variety.createdAt).toLocaleDateString()}
        </span>

        {hasOptions && (
          <span className="shrink-0 flex items-center gap-1 rounded-full bg-accent px-2 py-0.5 text-xs font-medium">
            {variety.varieties.length} option
            {variety.varieties.length !== 1 ? "s" : ""}
            <ChevronDown
              className={`h-3.5 w-3.5 transition-transform ${open ? "rotate-180" : ""}`}
            />
          </span>
        )}
      </button>

      {open && (
        <div className="border-t bg-muted/20 px-4 py-3 space-y-2">
          {itemNames.length > 0 && (
            <div className="flex flex-wrap gap-1 pb-1">
              {itemNames.map((name, i) => (
                <span
                  key={i}
                  className="inline-block rounded bg-muted px-2 py-0.5 text-xs text-muted-foreground"
                >
                  {name}
                </span>
              ))}
            </div>
          )}
          {variety.varieties.map((v, i) => (
            <div
              key={i}
              className="flex items-center justify-between rounded-md bg-background px-3 py-1.5 text-sm"
            >
              <span>{v.name}</span>
              <span className="font-medium">GHS {v.price.toFixed(2)}</span>
            </div>
          ))}
        </div>
      )}
    </li>
  );
}

export default function VarietyPage() {
  const [showCreate, setShowCreate] = useState(false);
  const [editingVariety, setEditingVariety] = useState<Variety | null>(null);
  const { data: varieties, isLoading, isError } = useVarieties();
  const deleteMutation = useDeleteVariety();

  const handleEdit = (v: Variety) => {
    setShowCreate(false);
    setEditingVariety(v);
  };

  const handleEditDone = () => setEditingVariety(null);

  const handleDelete = (id: string) => {
    if (editingVariety?.id === id) setEditingVariety(null);
    deleteMutation.mutate(id);
  };

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Varieties</h1>
          <p className="text-sm text-muted-foreground">
            {varieties
              ? `${varieties.length} variet${varieties.length !== 1 ? "ies" : "y"}`
              : ""}
          </p>
        </div>
        <Button
          onClick={() => {
            setEditingVariety(null);
            setShowCreate((s) => !s);
          }}
        >
          {showCreate ? (
            <>
              <X className="h-4 w-4" />
              Cancel
            </>
          ) : (
            <>
              <Plus className="h-4 w-4" />
              New Variety
            </>
          )}
        </Button>
      </div>

      {editingVariety && (
        <VarietyForm
          mode="edit"
          initial={editingVariety}
          onDone={handleEditDone}
        />
      )}

      {showCreate && !editingVariety && (
        <VarietyForm mode="create" onDone={() => setShowCreate(false)} />
      )}

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load varieties. Make sure the API is running.
        </p>
      )}

      {varieties && (
        <>
          {varieties.length === 0 && !showCreate && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Layers className="mb-3 h-10 w-10 text-muted-foreground/50" />
              <p className="text-sm text-muted-foreground">
                No varieties yet. Create one to get started.
              </p>
            </div>
          )}

          <ul className="rounded-lg border bg-background">
            {varieties.map((v) => (
              <VarietyRow
                key={v.id}
                variety={v}
                onEdit={handleEdit}
                onDelete={handleDelete}
                isDeleting={deleteMutation.isPending}
              />
            ))}
          </ul>
        </>
      )}
    </div>
  );
}
