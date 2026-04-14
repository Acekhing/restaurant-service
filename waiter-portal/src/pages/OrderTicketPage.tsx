import { useParams, useNavigate } from "react-router-dom";
import { CheckCircle2, Plus, ClipboardList } from "lucide-react";
import { useOrder } from "@/hooks/useOrders";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency } from "@/lib/utils";

export default function OrderTicketPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: order, isLoading } = useOrder(id!);

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="py-20 text-center">
        <p className="text-muted-foreground">Order not found.</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-6 px-4 py-8">
      {/* Success Icon */}
      <div className="flex h-20 w-20 items-center justify-center rounded-full bg-green-100">
        <CheckCircle2 className="h-10 w-10 text-green-600" />
      </div>

      <div className="text-center">
        <p className="text-sm text-muted-foreground">Order Placed</p>
        <h1 className="mt-1 text-lg font-medium text-muted-foreground">
          Order Number
        </h1>
      </div>

      {/* Large Order Number */}
      <div className="flex h-32 w-32 items-center justify-center rounded-2xl border-4 border-primary bg-primary/5">
        <span className="text-5xl font-black tracking-tight">
          {order.orderNumber}
        </span>
      </div>

      <p className="text-sm text-muted-foreground">
        Write this number for the customer
      </p>

      {/* Order Summary */}
      <div className="w-full max-w-sm space-y-3 rounded-xl border p-4">
        {order.tableNumber && (
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Table</span>
            <span className="font-medium">{order.tableNumber}</span>
          </div>
        )}
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Items</span>
          <span className="font-medium">
            {order.lines.reduce((s, l) => s + l.quantity, 0)}
          </span>
        </div>
        {order.customerPhone && (
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Phone</span>
            <span className="font-medium">{order.customerPhone}</span>
          </div>
        )}
        <div className="border-t pt-2">
          {order.lines.map((line) => (
            <div
              key={line.id}
              className="flex justify-between py-1 text-sm"
            >
              <span>
                {line.itemName} x{line.quantity}
              </span>
              <span className="text-muted-foreground">
                {formatCurrency(
                  line.unitPrice * line.quantity,
                  order.displayCurrency
                )}
              </span>
            </div>
          ))}
        </div>
        <div className="flex justify-between border-t pt-2 text-base font-bold">
          <span>Total</span>
          <span>
            {formatCurrency(order.totalAmount, order.displayCurrency)}
          </span>
        </div>
      </div>

      {/* Actions */}
      <div className="flex w-full max-w-sm gap-3">
        <Button
          variant="outline"
          size="lg"
          className="flex-1"
          onClick={() => navigate(`/orders/${order.id}`)}
        >
          <ClipboardList className="h-5 w-5" />
          View Order
        </Button>
        <Button
          size="lg"
          className="flex-1"
          onClick={() => navigate("/scanner")}
        >
          <Plus className="h-5 w-5" />
          New Order
        </Button>
      </div>
    </div>
  );
}
