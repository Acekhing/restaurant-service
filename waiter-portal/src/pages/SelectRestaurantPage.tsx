import { useNavigate } from "react-router-dom";
import { Store, ChevronRight } from "lucide-react";
import { useRetailers } from "@/hooks/useRetailers";
import { useRestaurant } from "@/context/RestaurantContext";
import { Spinner } from "@/components/ui/spinner";

export default function SelectRestaurantPage() {
  const navigate = useNavigate();
  const { retailerId, setRestaurant } = useRestaurant();
  const { data: retailers, isLoading } = useRetailers();

  const restaurants = retailers?.filter(
    (r) => r.retailerType.toLowerCase() === "restaurant"
  );

  const handleSelect = (id: string, name: string) => {
    setRestaurant(id, name);
    navigate("/scanner");
  };

  if (retailerId) {
    navigate("/scanner", { replace: true });
    return null;
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center px-4">
      <div className="w-full max-w-sm space-y-6">
        {/* Header */}
        <div className="text-center">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10">
            <Store className="h-8 w-8 text-primary" />
          </div>
          <h1 className="mt-4 text-2xl font-bold">Waiter Portal</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Select your restaurant to get started
          </p>
        </div>

        {/* Restaurant list */}
        {isLoading ? (
          <div className="flex justify-center py-8">
            <Spinner />
          </div>
        ) : !restaurants || restaurants.length === 0 ? (
          <p className="py-8 text-center text-sm text-muted-foreground">
            No restaurants found. Please create one in the retailer portal first.
          </p>
        ) : (
          <div className="space-y-2">
            {restaurants.map((r) => (
              <button
                key={r.id}
                onClick={() =>
                  handleSelect(r.id, r.businessName ?? r.id)
                }
                className="flex w-full items-center justify-between rounded-xl border bg-background px-4 py-4 text-left transition-colors hover:bg-accent/50 active:bg-accent"
              >
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-muted">
                    <Store className="h-5 w-5 text-muted-foreground" />
                  </div>
                  <div>
                    <p className="font-medium">
                      {r.businessName ?? "Unnamed Restaurant"}
                    </p>
                    <p className="text-xs text-muted-foreground font-mono">
                      {r.id}
                    </p>
                  </div>
                </div>
                <ChevronRight className="h-5 w-5 text-muted-foreground" />
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
