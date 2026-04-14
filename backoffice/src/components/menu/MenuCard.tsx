import { Link } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import type { Menu } from "@/types/menu";

export default function MenuCard({ menu }: { menu: Menu }) {
  const itemCount = menu.inventoryItems?.length ?? 0;

  return (
    <Link
      to={`/menus/${menu.id}`}
      className="group flex flex-col rounded-lg border bg-background p-4 transition-shadow hover:shadow-md"
    >
      <div className="mb-3 flex items-start justify-between">
        <div className="min-w-0 flex-1">
          <h3 className="truncate text-sm font-semibold group-hover:text-primary">
            {menu.categoryName ?? "Untitled Menu"}
          </h3>
          <p className="mt-0.5 text-xs text-muted-foreground">
            {menu.ownerName ?? menu.ownerId}
          </p>
        </div>
      </div>

      {menu.description && (
        <p className="mb-3 line-clamp-2 text-xs text-muted-foreground">
          {menu.description}
        </p>
      )}

      {menu.priceRange && (
        <div className="mb-3 text-xs text-muted-foreground">
          {menu.displayCurrency} {menu.priceRange}
        </div>
      )}

      <div className="mt-auto flex items-end justify-between">
        <p className="text-sm text-muted-foreground">
          {itemCount} item{itemCount !== 1 && "s"}
        </p>
        <Badge
          className={
            menu.isActive
              ? "bg-green-100 text-green-800"
              : "bg-gray-100 text-gray-800"
          }
        >
          {menu.isActive ? "Active" : "Inactive"}
        </Badge>
      </div>
    </Link>
  );
}
