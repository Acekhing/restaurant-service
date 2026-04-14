import { useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { ScanLine, ShoppingCart, Check, AlertCircle } from "lucide-react";
import BarcodeScannerInput from "@/components/scanner/BarcodeScannerInput";
import { useCart } from "@/context/CartContext";
import { useRestaurant } from "@/context/RestaurantContext";
import { getItemByCode, getMenuByCode } from "@/api/inventory";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { formatCurrency } from "@/lib/utils";
import { cn } from "@/lib/utils";

type ScanFeedback =
  | { type: "success"; name: string; price: number; currency: string }
  | { type: "error"; message: string }
  | null;

export default function ScannerPage() {
  const navigate = useNavigate();
  const { retailerId } = useRestaurant();
  const { state, dispatch, totalAmount, totalItems } = useCart();
  const [feedback, setFeedback] = useState<ScanFeedback>(null);
  const [loading, setLoading] = useState(false);

  const handleScan = useCallback(
    async (code: string) => {
      setFeedback(null);
      setLoading(true);

      try {
        try {
          const item = await getItemByCode(code);
          dispatch({
            type: "ADD_ITEM",
            payload: {
              inventoryItemId: item.id,
              itemName: item.name,
              unitPrice: item.displayPrice,
              displayCurrency: item.displayCurrency,
            },
          });
          setFeedback({
            type: "success",
            name: item.name,
            price: item.displayPrice,
            currency: item.displayCurrency,
          });
          setTimeout(() => setFeedback(null), 2500);
          return;
        } catch {
          // Not an inventory item code; try menu code
        }

        try {
          const menu = await getMenuByCode(code);
          if (menu.inventoryItems && menu.inventoryItems.length > 0) {
            for (const mi of menu.inventoryItems) {
              dispatch({
                type: "ADD_ITEM",
                payload: {
                  inventoryItemId: mi.id,
                  itemName: mi.name,
                  unitPrice: mi.displayPrice,
                  displayCurrency: menu.displayCurrency,
                },
              });
            }
            setFeedback({
              type: "success",
              name: `Menu: ${menu.categoryName ?? "Untitled"} (${menu.inventoryItems.length} items)`,
              price: menu.inventoryItems.reduce((s, i) => s + i.displayPrice, 0),
              currency: menu.displayCurrency,
            });
            setTimeout(() => setFeedback(null), 2500);
            return;
          }
        } catch {
          // Not a menu code either
        }

        setFeedback({ type: "error", message: `Unknown code: "${code}"` });
        setTimeout(() => setFeedback(null), 3000);
      } finally {
        setLoading(false);
      }
    },
    [dispatch]
  );

  if (!retailerId) {
    navigate("/", { replace: true });
    return null;
  }

  return (
    <div className="flex flex-col gap-4 pb-24">
      {/* Scanner */}
      <div className="space-y-3">
        <div className="flex items-center gap-2">
          <ScanLine className="h-5 w-5 text-muted-foreground" />
          <h2 className="text-lg font-semibold">Scan Menu Item</h2>
        </div>

        <BarcodeScannerInput onScan={handleScan} disabled={loading} />

        {/* Scan feedback toast */}
        {feedback && (
          <div
            className={cn(
              "flex items-center gap-3 rounded-lg px-4 py-3 text-sm font-medium animate-in fade-in slide-in-from-top-2",
              feedback.type === "success"
                ? "bg-green-100 text-green-800"
                : "bg-red-100 text-red-800"
            )}
          >
            {feedback.type === "success" ? (
              <>
                <Check className="h-5 w-5 shrink-0" />
                <span className="flex-1">{feedback.name}</span>
                <span className="font-bold">
                  {formatCurrency(feedback.price, feedback.currency)}
                </span>
              </>
            ) : (
              <>
                <AlertCircle className="h-5 w-5 shrink-0" />
                <span>{feedback.message}</span>
              </>
            )}
          </div>
        )}
      </div>

      {/* Cart summary */}
      {state.items.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Current Order ({totalItems} items)
            </h3>
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive"
              onClick={() => dispatch({ type: "CLEAR" })}
            >
              Clear All
            </Button>
          </div>

          <div className="space-y-2">
            {state.items.map((item) => (
              <div
                key={item.inventoryItemId}
                className="flex items-center justify-between rounded-lg border bg-background px-4 py-3"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium truncate">{item.itemName}</p>
                  <p className="text-sm text-muted-foreground">
                    {formatCurrency(item.unitPrice, item.displayCurrency)}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() =>
                      dispatch({
                        type: "SET_QUANTITY",
                        payload: {
                          inventoryItemId: item.inventoryItemId,
                          quantity: item.quantity - 1,
                        },
                      })
                    }
                  >
                    -
                  </Button>
                  <span className="w-6 text-center font-medium">
                    {item.quantity}
                  </span>
                  <Button
                    variant="outline"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() =>
                      dispatch({
                        type: "SET_QUANTITY",
                        payload: {
                          inventoryItemId: item.inventoryItemId,
                          quantity: item.quantity + 1,
                        },
                      })
                    }
                  >
                    +
                  </Button>
                </div>
              </div>
            ))}
          </div>

          {/* Review button */}
          <Button
            size="lg"
            className="w-full text-base"
            onClick={() => navigate("/orders/review")}
          >
            <ShoppingCart className="h-5 w-5" />
            Review Order
            <Badge className="bg-white/20 text-white ml-2">
              {formatCurrency(totalAmount, state.items[0]?.displayCurrency)}
            </Badge>
          </Button>
        </div>
      )}
    </div>
  );
}
