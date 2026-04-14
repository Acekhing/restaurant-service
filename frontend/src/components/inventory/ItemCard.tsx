import { Link } from "react-router-dom";
import { ImageOff } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { formatCurrency, itemTypeBadgeColor } from "@/lib/utils";
import type { InventoryItem } from "@/types/inventory";

export default function ItemCard({ item }: { item: InventoryItem }) {
  return (
    <Link
      to={`/items/${item.id}`}
      className="group flex flex-col rounded-lg border bg-background overflow-hidden transition-shadow hover:shadow-md"
    >
      {item.image ? (
        <div className="relative h-40 w-full bg-muted">
          <img
            src={item.image}
            alt={item.name}
            className="h-full w-full object-cover"
          />
        </div>
      ) : (
        <div className="flex h-28 w-full items-center justify-center bg-muted/50">
          <ImageOff className="h-8 w-8 text-muted-foreground/40" />
        </div>
      )}

      <div className="flex flex-1 flex-col p-4">
        <div className="mb-3 flex items-start justify-between">
          <div className="min-w-0 flex-1">
            <h3 className="truncate text-sm font-semibold group-hover:text-primary">
              {item.name}
            </h3>
            <p className="mt-0.5 truncate text-xs text-muted-foreground">
              {item.retailerId?.slice(0, 8)}... &middot; {item.retailerType}
            </p>
          </div>
          <Badge className={itemTypeBadgeColor(item.itemType)}>
            {item.itemType}
          </Badge>
        </div>

        <div className="mt-auto flex items-end justify-between">
          <div>
            <p className="text-lg font-bold">
              {formatCurrency(item.displayPrice, item.displayCurrency)}
            </p>
            {item.oldSellingPrice != null &&
              item.oldSellingPrice > item.displayPrice && (
                <p className="text-xs text-muted-foreground line-through">
                  {formatCurrency(item.oldSellingPrice, item.displayCurrency)}
                </p>
              )}
          </div>
          <div className="flex items-center gap-2">
            {item.hasDeals && (
              <Badge className="bg-amber-100 text-amber-800">Deal</Badge>
            )}
            <Badge
              className={
                item.isAvailable
                  ? "bg-green-100 text-green-800"
                  : "bg-red-100 text-red-800"
              }
            >
              {item.isAvailable ? "Available" : "Unavailable"}
            </Badge>
          </div>
        </div>
      </div>
    </Link>
  );
}
