import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Trash2, ShoppingCart } from "lucide-react";
import { useCart } from "@/context/CartContext";
import { useRestaurant } from "@/context/RestaurantContext";
import { useAuth } from "@/context/AuthContext";
import { useCreateOrder } from "@/hooks/useOrders";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency } from "@/lib/utils";

export default function ReviewPage() {
  const navigate = useNavigate();
  const { state, dispatch, totalAmount } = useCart();
  const { retailerId } = useRestaurant();
  const { user } = useAuth();
  const createOrder = useCreateOrder();
  const [tableNumber, setTableNumber] = useState(state.tableNumber);
  const [customerNotes, setCustomerNotes] = useState(state.customerNotes);
  const [customerPhone, setCustomerPhone] = useState("");

  if (state.items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 py-20">
        <ShoppingCart className="h-12 w-12 text-muted-foreground/30" />
        <p className="text-muted-foreground">Your cart is empty.</p>
        <Button onClick={() => navigate("/scanner")}>Start Scanning</Button>
      </div>
    );
  }

  const currency = state.items[0]?.displayCurrency ?? "GHS";

  const handlePlaceOrder = () => {
    createOrder.mutate(
      {
        retailerId,
        waiterName: user?.fullName || user?.email || undefined,
        tableNumber: tableNumber || undefined,
        customerNotes: customerNotes || undefined,
        customerPhone: customerPhone || undefined,
        displayCurrency: currency,
        lines: state.items.map((item) => ({
          inventoryItemId: item.inventoryItemId,
          itemName: item.itemName,
          unitPrice: item.unitPrice,
          quantity: item.quantity,
        })),
      },
      {
        onSuccess: (result) => {
          dispatch({ type: "CLEAR" });
          navigate(`/orders/${result.id}/ticket`);
        },
      }
    );
  };

  return (
    <div className="flex flex-col gap-5 pb-24">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => navigate("/scanner")}
        >
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-xl font-bold">Review Order</h1>
      </div>

      {/* Items */}
      <div className="space-y-2">
        {state.items.map((item) => (
          <div
            key={item.inventoryItemId}
            className="flex items-center justify-between rounded-lg border px-4 py-3"
          >
            <div className="flex-1 min-w-0">
              <p className="font-medium truncate">{item.itemName}</p>
              <p className="text-sm text-muted-foreground">
                {formatCurrency(item.unitPrice, item.displayCurrency)} x{" "}
                {item.quantity}
              </p>
            </div>
            <div className="flex items-center gap-3">
              <p className="font-semibold">
                {formatCurrency(
                  item.unitPrice * item.quantity,
                  item.displayCurrency
                )}
              </p>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-muted-foreground hover:text-destructive"
                onClick={() =>
                  dispatch({
                    type: "REMOVE_ITEM",
                    payload: item.inventoryItemId,
                  })
                }
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          </div>
        ))}
      </div>

      {/* Details */}
      <div className="space-y-3">
        <div>
          <label className="text-sm font-medium text-muted-foreground">
            Table Number (optional)
          </label>
          <input
            type="text"
            value={tableNumber}
            onChange={(e) => setTableNumber(e.target.value)}
            placeholder="e.g. T-05"
            className="mt-1 w-full rounded-lg border bg-background px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">
            Customer Phone (optional)
          </label>
          <input
            type="tel"
            value={customerPhone}
            onChange={(e) => setCustomerPhone(e.target.value)}
            placeholder="e.g. +233 24 000 0000"
            className="mt-1 w-full rounded-lg border bg-background px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-muted-foreground">
            Customer Notes (optional)
          </label>
          <textarea
            value={customerNotes}
            onChange={(e) => setCustomerNotes(e.target.value)}
            placeholder="Any special requests..."
            rows={2}
            className="mt-1 w-full rounded-lg border bg-background px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring resize-none"
          />
        </div>
      </div>

      {/* Total + Place Order */}
      <div className="space-y-3 rounded-lg border bg-muted/50 p-4">
        <div className="flex items-center justify-between text-lg font-bold">
          <span>Total</span>
          <span>{formatCurrency(totalAmount, currency)}</span>
        </div>

        {createOrder.isError && (
          <p className="text-sm text-destructive">
            Failed to place order. Please try again.
          </p>
        )}

        <Button
          size="lg"
          className="w-full text-base"
          disabled={createOrder.isPending}
          onClick={handlePlaceOrder}
        >
          {createOrder.isPending && <Spinner className="h-5 w-5" />}
          Place Order
        </Button>
      </div>
    </div>
  );
}
