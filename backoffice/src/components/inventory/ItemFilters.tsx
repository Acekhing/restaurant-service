import { Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import RetailerSelector from "@/components/RetailerSelector";

const retailerTypes = ["all", "restaurant", "pharmacy", "shop"] as const;

interface ItemFiltersProps {
  query: string;
  onQueryChange: (q: string) => void;
  selectedRetailerType: string;
  onRetailerTypeChange: (t: string) => void;
  selectedOwner?: string;
  onOwnerChange?: (ownerId: string) => void;
}

export default function ItemFilters({
  query,
  onQueryChange,
  selectedRetailerType,
  onRetailerTypeChange,
  selectedOwner,
  onOwnerChange,
}: ItemFiltersProps) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search items..."
          value={query}
          onChange={(e) => onQueryChange(e.target.value)}
          className="pl-9"
        />
      </div>
      {onOwnerChange && (
        <RetailerSelector
          value={selectedOwner ?? ""}
          onChange={onOwnerChange}
          className="w-48"
        />
      )}
      <div className="flex gap-1">
        {retailerTypes.map((type) => (
          <button
            key={type}
            onClick={() => onRetailerTypeChange(type === "all" ? "" : type)}
            className={cn(
              "rounded-full px-3 py-1.5 text-xs font-medium capitalize transition-colors",
              (type === "all" && selectedRetailerType === "") ||
                type === selectedRetailerType
                ? "bg-primary text-primary-foreground"
                : "bg-muted text-muted-foreground hover:bg-accent"
            )}
          >
            {type}
          </button>
        ))}
      </div>
    </div>
  );
}
