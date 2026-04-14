import { useState } from "react";
import { useSearchItems } from "@/hooks/useInventory";
import { useUpdateAvailability } from "@/hooks/useInventory";
import { useRetailerContext } from "@/context/RetailerContext";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { Package } from "lucide-react";

function AvailabilityRow({ item }: { item: { id: string; name: string; itemType: string; isAvailable: boolean; outOfStock: boolean; displayCurrency: string; displayPrice: number } }) {
  const mutation = useUpdateAvailability(item.id);

  const handleToggle = () => {
    mutation.mutate({
      isAvailable: !item.isAvailable,
      outOfStock: item.isAvailable,
    });
  };

  return (
    <li className="flex items-center gap-4 border-b px-4 py-3 last:border-b-0">
      <div className="min-w-0 flex-1">
        <p className="truncate font-medium">{item.name}</p>
        <p className="text-xs text-muted-foreground capitalize">{item.itemType}</p>
      </div>

      <span className="text-sm text-muted-foreground">
        {item.displayCurrency} {item.displayPrice.toFixed(2)}
      </span>

      {item.outOfStock && (
        <Badge className="bg-amber-100 text-amber-800">Out of Stock</Badge>
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

      <Button
        variant="outline"
        size="sm"
        disabled={mutation.isPending}
        onClick={handleToggle}
      >
        {mutation.isPending ? (
          <Spinner className="h-4 w-4" />
        ) : item.isAvailable ? (
          "Mark Unavailable"
        ) : (
          "Mark Available"
        )}
      </Button>
    </li>
  );
}

export default function AvailabilityTab() {
  const { retailerId } = useRetailerContext();
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, isError } = useSearchItems({
    ownerId: retailerId || undefined,
    page,
    size: pageSize,
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-bold">Availability</h2>
        <p className="text-sm text-muted-foreground">
          {data
            ? `${data.total} item${data.total !== 1 ? "s" : ""}`
            : "Manage inventory availability"}
        </p>
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load items. Make sure the API and Elasticsearch are running.
        </p>
      )}

      {data && (
        <>
          {data.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Package className="mb-3 h-10 w-10 text-muted-foreground/50" />
              <p className="text-sm text-muted-foreground">
                No inventory items found.{" "}
                {!retailerId && "Select a retailer to filter items."}
              </p>
            </div>
          ) : (
            <ul className="rounded-lg border bg-background">
              {data.items.map((item) => (
                <AvailabilityRow key={item.id} item={item} />
              ))}
            </ul>
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
