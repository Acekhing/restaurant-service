import { useState, useMemo } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import {
  ArrowLeft,
  DollarSign,
  ToggleLeft,
  Megaphone,
  ScrollText,
  Trash2,
  ImageOff,
  Store,
  Clock,
  MapPin,
  Tag,
  Info,
  ShoppingBag,
  QrCode,
} from "lucide-react";
import type { Variety } from "@/types/variety";
import { useItemAuditLog } from "@/hooks/useAudit";
import AuditTable from "@/components/audit/AuditTable";
import { useItem, useDeleteItem, useUpdateInventoryItemCode } from "@/hooks/useInventory";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { Dialog } from "@/components/ui/dialog";
import { formatCurrency, itemTypeBadgeColor } from "@/lib/utils";
import { cn } from "@/lib/utils";
import PromotionHistory from "@/components/inventory/PromotionHistory";
import PriceUpdateDialog from "@/components/inventory/PriceUpdateDialog";
import AvailabilityDialog from "@/components/inventory/AvailabilityDialog";
import PromotionDialog from "@/components/inventory/PromotionDialog";

type Tab = "overview" | "promotions" | "audit";

export default function ItemDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: item, isLoading, isError } = useItem(id!);
  const deleteMutation = useDeleteItem();
  const [tab, setTab] = useState<Tab>("overview");
  const [priceOpen, setPriceOpen] = useState(false);
  const [availOpen, setAvailOpen] = useState(false);
  const [promoOpen, setPromoOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  if (isLoading)
    return (
      <div className="flex justify-center py-20">
        <Spinner />
      </div>
    );

  if (isError || !item)
    return (
      <div className="py-20 text-center">
        <p className="text-muted-foreground">Item not found.</p>
        <Link to="/" className="mt-2 inline-block text-sm underline">
          Back to inventory
        </Link>
      </div>
    );

  const tabs: { key: Tab; label: string }[] = [
    { key: "overview", label: "Overview" },
    { key: "promotions", label: "Promotions" },
    { key: "audit", label: "Audit Trail" },
  ];

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-start gap-3">
          <Link to="/">
            <Button variant="ghost" size="icon" className="mt-0.5">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{item.name}</h1>
              <Badge className={itemTypeBadgeColor(item.itemType)}>
                {item.itemType}
              </Badge>
            </div>
            {item.shortName && (
              <p className="text-sm text-muted-foreground">{item.shortName}</p>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => setPriceOpen(true)}>
            <DollarSign className="h-3.5 w-3.5" />
            Update Price
          </Button>
          <Button variant="outline" size="sm" onClick={() => setAvailOpen(true)}>
            <ToggleLeft className="h-3.5 w-3.5" />
            Availability
          </Button>
          <Button size="sm" onClick={() => setPromoOpen(true)}>
            <Megaphone className="h-3.5 w-3.5" />
            Run Promotion
          </Button>
          <Button variant="destructive" size="sm" onClick={() => setDeleteOpen(true)}>
            <Trash2 className="h-3.5 w-3.5" />
            Delete
          </Button>
        </div>
      </div>

      {/* Tabs */}
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

      {/* Tab content */}
      {tab === "overview" && <OverviewTab item={item} />}
      {tab === "promotions" && <PromotionHistory ownerId={item.retailerId} />}
      {tab === "audit" && <ItemAuditTab itemId={item.id} />}

      {/* Dialogs */}
      <PriceUpdateDialog
        open={priceOpen}
        onClose={() => setPriceOpen(false)}
        itemId={item.id}
        currentPrice={item.displayPrice}
        currency={item.displayCurrency}
      />
      <AvailabilityDialog
        open={availOpen}
        onClose={() => setAvailOpen(false)}
        itemId={item.id}
        currentAvailable={item.isAvailable}
        currentOutOfStock={item.outOfStock}
      />
      <PromotionDialog
        open={promoOpen}
        onClose={() => setPromoOpen(false)}
        ownerId={item.retailerId}
        currency={item.displayCurrency}
      />
      <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)} title="Delete Item">
        <div className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Are you sure you want to permanently delete <span className="font-medium text-foreground">{item.name}</span>? This action cannot be undone.
          </p>
          {deleteMutation.isError && (
            <p className="text-sm text-destructive">
              {(deleteMutation.error as any)?.response?.data?.error ??
                "Failed to delete item. It may be part of a menu."}
            </p>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() =>
                deleteMutation.mutate(item.id, {
                  onSuccess: () => navigate("/"),
                })
              }
            >
              {deleteMutation.isPending && <Spinner className="h-4 w-4" />}
              Delete Permanently
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}

function InventoryItemCodeSection({ item }: { item: import("@/types/inventory").InventoryItem }) {
  const [editing, setEditing] = useState(false);
  const [code, setCode] = useState(item.inventoryItemCode ?? "");
  const mutation = useUpdateInventoryItemCode(item.id);

  const handleSave = () => {
    mutation.mutate(
      { inventoryItemCode: code.trim() || null },
      { onSuccess: () => setEditing(false) }
    );
  };

  return (
    <div className="space-y-4 rounded-lg border p-5">
      <div className="flex items-center gap-2">
        <QrCode className="h-4 w-4 text-muted-foreground" />
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          Inventory Item Code
        </h3>
      </div>
      {editing ? (
        <div className="space-y-2">
          <input
            type="text"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="e.g. JR-001"
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
            <Button size="sm" variant="outline" onClick={() => { setEditing(false); setCode(item.inventoryItemCode ?? ""); }}>
              Cancel
            </Button>
          </div>
        </div>
      ) : (
        <div className="flex items-center justify-between">
          <span className="font-mono text-sm">
            {item.inventoryItemCode ?? <span className="text-muted-foreground italic">Not set</span>}
          </span>
          <Button size="sm" variant="outline" onClick={() => setEditing(true)}>
            {item.inventoryItemCode ? "Edit" : "Set Code"}
          </Button>
        </div>
      )}
    </div>
  );
}

function OverviewTab({
  item,
}: {
  item: import("@/types/inventory").InventoryItem;
}) {
  const variety = useMemo<Variety | null>(() => {
    if (!item.variety) return null;
    try {
      return typeof item.variety === "string"
        ? JSON.parse(item.variety)
        : item.variety;
    } catch {
      return null;
    }
  }, [item.variety]);

  return (
    <div className="space-y-6">
      {/* Image + Basic Info Hero */}
      <div className="flex gap-6 rounded-lg border p-5">
        {item.image ? (
          <div className="h-48 w-48 shrink-0 overflow-hidden rounded-lg bg-muted">
            <img
              src={item.image}
              alt={item.name}
              className="h-full w-full object-cover"
            />
          </div>
        ) : (
          <div className="flex h-48 w-48 shrink-0 items-center justify-center rounded-lg bg-muted/50">
            <ImageOff className="h-12 w-12 text-muted-foreground/30" />
          </div>
        )}
        <div className="flex flex-1 flex-col justify-between">
          <div className="space-y-2">
            <div>
              <h2 className="text-xl font-bold">{item.name}</h2>
              {item.shortName && (
                <p className="text-sm text-muted-foreground">{item.shortName}</p>
              )}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Badge className={itemTypeBadgeColor(item.itemType)}>
                {item.itemType}
              </Badge>
              <Badge
                className={
                  item.isAvailable
                    ? "bg-green-100 text-green-800"
                    : "bg-red-100 text-red-800"
                }
              >
                {item.isAvailable ? "Available" : "Unavailable"}
              </Badge>
              {item.outOfStock && (
                <Badge className="bg-red-100 text-red-800">Out of Stock</Badge>
              )}
              {item.hasDeals && (
                <Badge className="bg-amber-100 text-amber-800">Has Deals</Badge>
              )}
              {item.hasVariety && (
                <Badge className="bg-teal-100 text-teal-800">Has Variety</Badge>
              )}
            </div>
            {item.tags && (
              <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                <Tag className="h-3.5 w-3.5" />
                {item.tags}
              </div>
            )}
            {item.notes && (
              <p className="text-sm text-muted-foreground">{item.notes}</p>
            )}
          </div>
          <div className="mt-3">
            <span className="text-2xl font-bold">
              {formatCurrency(item.displayPrice, item.displayCurrency)}
            </span>
            {item.oldSellingPrice != null && item.oldSellingPrice > item.displayPrice && (
              <span className="ml-2 text-sm text-muted-foreground line-through">
                {formatCurrency(item.oldSellingPrice, item.displayCurrency)}
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Sections Grid */}
      <div className="grid gap-6 sm:grid-cols-2">
        {/* Pricing */}
        <Section icon={DollarSign} title="Pricing">
          <Row label="Display Price" value={formatCurrency(item.displayPrice, item.displayCurrency)} />
          {item.supplierPrice != null && (
            <Row label="Supplier Price" value={formatCurrency(item.supplierPrice, item.displayCurrency)} />
          )}
          {item.oldSellingPrice != null && item.oldSellingPrice > 0 && (
            <Row label="Previous Price" value={formatCurrency(item.oldSellingPrice, item.displayCurrency)} />
          )}
          <Row label="Delivery Fee" value={formatCurrency(item.deliveryFee, item.displayCurrency)} />
          {item.priceRange && <Row label="Price Range" value={item.priceRange} />}
          <Row label="Currency" value={item.displayCurrency} />
        </Section>

        {/* Availability & Status */}
        <Section icon={ToggleLeft} title="Availability & Status">
          <Row
            label="Available"
            value={
              <Badge className={item.isAvailable ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800"}>
                {item.isAvailable ? "Yes" : "No"}
              </Badge>
            }
          />
          <Row label="Out of Stock" value={item.outOfStock ? "Yes" : "No"} />
          <Row label="Has Deals" value={item.hasDeals ? "Yes" : "No"} />
          <Row label="Has Variety" value={item.hasVariety ? "Yes" : "No"} />
          {item.averagePreparationTime != null && (
            <Row label="Avg Prep Time" value={`${item.averagePreparationTime} min`} />
          )}
        </Section>

        {/* Retailer & Location */}
        <Section icon={Store} title="Retailer & Location">
          <Row label="Retailer ID" value={
            <span className="font-mono text-xs">{item.retailerId}</span>
          } />
          <Row label="Retailer Type" value={
            <Badge className={itemTypeBadgeColor(item.retailerType)}>{item.retailerType}</Badge>
          } />
          {item.stationId && (
            <Row label="Station ID" value={
              <span className="font-mono text-xs">{item.stationId}</span>
            } />
          )}
          {item.zoneId && (
            <Row label="Zone ID" value={
              <span className="font-mono text-xs">{item.zoneId}</span>
            } />
          )}
        </Section>

        {/* Image & Media */}
        <Section icon={Info} title="Media & Identity">
          <Row label="Item ID" value={
            <span className="font-mono text-xs">{item.id}</span>
          } />
          <Row label="Has Image" value={item.image ? "Yes" : "No"} />
          {item.rawImageUrl && (
            <Row label="Raw Image" value={
              <a href={item.rawImageUrl} target="_blank" rel="noopener noreferrer" className="text-xs underline">
                View
              </a>
            } />
          )}
          <Row label="Original Image" value={item.isOriginalImage ? "Yes" : "No"} />
          <Row label="Item Type" value={item.itemType} />
          {item.shortName && <Row label="Short Name" value={item.shortName} />}
        </Section>
      </div>

      {/* Inventory Item Code */}
      <InventoryItemCodeSection item={item} />

      {/* Opening Hours */}
      {item.openingDayHours && item.openingDayHours.length > 0 && (
        <div className="space-y-4 rounded-lg border p-5">
          <div className="flex items-center gap-2">
            <Clock className="h-4 w-4 text-muted-foreground" />
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
              Opening Hours
            </h3>
          </div>
          <div className="grid gap-1 sm:grid-cols-2">
            {item.openingDayHours.map((h) => (
              <div
                key={h.id}
                className="flex items-center justify-between rounded-md bg-muted/50 px-3 py-2 text-sm"
              >
                <span className="w-24 font-medium">{h.day}</span>
                {h.isAvailable ? (
                  <span>
                    {h.openingTime} &ndash; {h.closingTime}
                  </span>
                ) : (
                  <span className="text-muted-foreground">Closed</span>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Display Times */}
      {item.displayTimes && item.displayTimes.length > 0 && (
        <div className="space-y-4 rounded-lg border p-5">
          <div className="flex items-center gap-2">
            <ShoppingBag className="h-4 w-4 text-muted-foreground" />
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
              Display Times
            </h3>
          </div>
          <div className="grid gap-1 sm:grid-cols-2">
            {item.displayTimes.map((h) => (
              <div
                key={h.id}
                className="flex items-center justify-between rounded-md bg-muted/50 px-3 py-2 text-sm"
              >
                <span className="w-24 font-medium">{h.day}</span>
                {h.isAvailable ? (
                  <span>
                    {h.openingTime} &ndash; {h.closingTime}
                  </span>
                ) : (
                  <span className="text-muted-foreground">N/A</span>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Variety */}
      {variety && <VarietySection variety={variety} currency={item.displayCurrency} />}

      {/* Additional Details */}
      {(item.tags || item.notes) && (
        <div className="space-y-4 rounded-lg border p-5">
          <div className="flex items-center gap-2">
            <Tag className="h-4 w-4 text-muted-foreground" />
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
              Tags & Notes
            </h3>
          </div>
          <div className="space-y-2">
            {item.tags && <Row label="Tags" value={item.tags} />}
            {item.notes && <Row label="Notes" value={item.notes} />}
          </div>
        </div>
      )}
    </div>
  );
}

function Section({
  icon: Icon,
  title,
  children,
}: {
  icon: React.ComponentType<{ className?: string }>;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-4 rounded-lg border p-5">
      <div className="flex items-center gap-2">
        <Icon className="h-4 w-4 text-muted-foreground" />
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          {title}
        </h3>
      </div>
      <div className="space-y-2">{children}</div>
    </div>
  );
}

function VarietySection({
  variety,
  currency,
}: {
  variety: Variety;
  currency: string;
}) {
  const options = Array.isArray(variety.varieties)
    ? variety.varieties
    : typeof variety.varieties === "string"
      ? (() => { try { return JSON.parse(variety.varieties); } catch { return []; } })()
      : [];

  return (
    <div className="space-y-4 rounded-lg border p-5">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          Variety &mdash; {variety.name}
        </h3>
        <span className="rounded-full bg-accent px-2.5 py-0.5 text-xs font-medium">
          {options.length} option{options.length !== 1 ? "s" : ""}
        </span>
      </div>

      {options.length > 0 && (
        <div className="space-y-1">
          {options.map(
            (opt: Record<string, unknown>, i: number) => {
              const optName = (opt.name ?? opt.Name ?? "") as string;
              const optPrice = (opt.price ?? opt.Price ?? 0) as number;
              return (
              <div
                key={i}
                className="flex items-center justify-between rounded-md bg-muted/50 px-3 py-2 text-sm"
              >
                <span>{optName}</span>
                <span className="font-medium">
                  {currency} {optPrice.toFixed(2)}
                </span>
              </div>
              );
            }
          )}
        </div>
      )}

      {variety.inventoryItemIds && variety.inventoryItemIds.length > 1 && (
        <p className="text-xs text-muted-foreground">
          Shared across {variety.inventoryItemIds.length} items
        </p>
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

function ItemAuditTab({ itemId }: { itemId: string }) {
  const { data, isLoading, isError } = useItemAuditLog(itemId);

  if (isLoading)
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    );

  if (isError)
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        Failed to load audit trail.
      </p>
    );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {data?.length ?? 0} audit entries for this item
        </p>
        <Link
          to={`/audit?aggregateId=${itemId}`}
          className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ScrollText className="h-3.5 w-3.5" />
          View in Audit Log
        </Link>
      </div>
      <AuditTable entries={data ?? []} />
    </div>
  );
}
