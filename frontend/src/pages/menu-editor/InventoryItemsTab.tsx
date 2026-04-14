import { useState } from "react";
import { Link } from "react-router-dom";
import { Plus } from "lucide-react";
import { useSearchItems } from "@/hooks/useInventory";
import { useRetailerContext } from "@/context/RetailerContext";
import ItemCard from "@/components/inventory/ItemCard";
import ItemFilters from "@/components/inventory/ItemFilters";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";

export default function InventoryItemsTab() {
  const { retailerId } = useRetailerContext();
  const [query, setQuery] = useState("");
  const [retailerType, setRetailerType] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, isError } = useSearchItems({
    q: query || undefined,
    retailerType: retailerType || undefined,
    ownerId: retailerId || undefined,
    page,
    size: pageSize,
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">Inventory Items</h2>
          <p className="text-sm text-muted-foreground">
            {data ? `${data.total} item${data.total !== 1 ? "s" : ""}` : ""}
          </p>
        </div>
        <Link to="/items/new">
          <Button>
            <Plus className="h-4 w-4" />
            Add Item
          </Button>
        </Link>
      </div>

      <ItemFilters
        query={query}
        onQueryChange={(q) => {
          setQuery(q);
          setPage(1);
        }}
        selectedRetailerType={retailerType}
        onRetailerTypeChange={(t) => {
          setRetailerType(t);
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
          Failed to load inventory items. Make sure the API and Elasticsearch
          are running.
        </p>
      )}

      {data && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((item) => (
              <ItemCard key={item.id} item={item} />
            ))}
          </div>

          {data.items.length === 0 && (
            <p className="py-12 text-center text-sm text-muted-foreground">
              No items found. Try adjusting your filters.
            </p>
          )}

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-4">
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
