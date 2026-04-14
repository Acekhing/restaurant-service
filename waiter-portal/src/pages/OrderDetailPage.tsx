import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Clock, User, Hash, MessageSquare, Phone } from "lucide-react";
import { useOrder, useUpdateOrderStatus } from "@/hooks/useOrders";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency, formatDate } from "@/lib/utils";
import { cn } from "@/lib/utils";
import type { OrderStatus } from "@/types/order";

const statusColors: Record<OrderStatus, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  Confirmed: "bg-blue-100 text-blue-800",
  Preparing: "bg-orange-100 text-orange-800",
  Ready: "bg-green-100 text-green-800",
  Served: "bg-gray-100 text-gray-800",
  Cancelled: "bg-red-100 text-red-800",
};

const statusFlow: OrderStatus[] = [
  "Pending",
  "Confirmed",
  "Preparing",
  "Ready",
  "Served",
];

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: order, isLoading } = useOrder(id!);
  const statusMutation = useUpdateOrderStatus(id!);

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

  const currentIndex = statusFlow.indexOf(order.status as OrderStatus);
  const nextStatus = currentIndex >= 0 && currentIndex < statusFlow.length - 1
    ? statusFlow[currentIndex + 1]
    : null;

  const isFinal = order.status === "Served" || order.status === "Cancelled";

  return (
    <div className="flex flex-col gap-5 pb-24">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/orders")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h1 className="text-xl font-bold">Order #{order.orderNumber}</h1>
            <Badge
              className={
                statusColors[order.status as OrderStatus] ??
                "bg-gray-100 text-gray-800"
              }
            >
              {order.status}
            </Badge>
          </div>
        </div>
      </div>

      {/* Status progress */}
      {!isFinal && (
        <div className="flex gap-1">
          {statusFlow.map((s, i) => (
            <div
              key={s}
              className={cn(
                "h-1.5 flex-1 rounded-full",
                i <= currentIndex ? "bg-primary" : "bg-muted"
              )}
            />
          ))}
        </div>
      )}

      {/* Meta */}
      <div className="grid grid-cols-2 gap-3">
        <div className="flex items-center gap-2 rounded-lg border p-3">
          <Clock className="h-4 w-4 text-muted-foreground" />
          <div>
            <p className="text-[10px] uppercase text-muted-foreground">Time</p>
            <p className="text-sm font-medium">{formatDate(order.createdAt)}</p>
          </div>
        </div>
        {order.tableNumber && (
          <div className="flex items-center gap-2 rounded-lg border p-3">
            <Hash className="h-4 w-4 text-muted-foreground" />
            <div>
              <p className="text-[10px] uppercase text-muted-foreground">
                Table
              </p>
              <p className="text-sm font-medium">{order.tableNumber}</p>
            </div>
          </div>
        )}
        {order.waiterName && (
          <div className="flex items-center gap-2 rounded-lg border p-3">
            <User className="h-4 w-4 text-muted-foreground" />
            <div>
              <p className="text-[10px] uppercase text-muted-foreground">
                Waiter
              </p>
              <p className="text-sm font-medium">{order.waiterName}</p>
            </div>
          </div>
        )}
        {order.customerPhone && (
          <div className="flex items-center gap-2 rounded-lg border p-3">
            <Phone className="h-4 w-4 text-muted-foreground" />
            <div>
              <p className="text-[10px] uppercase text-muted-foreground">
                Phone
              </p>
              <p className="text-sm font-medium">{order.customerPhone}</p>
            </div>
          </div>
        )}
        {order.customerNotes && (
          <div className="col-span-2 flex items-start gap-2 rounded-lg border p-3">
            <MessageSquare className="mt-0.5 h-4 w-4 text-muted-foreground" />
            <div>
              <p className="text-[10px] uppercase text-muted-foreground">
                Notes
              </p>
              <p className="text-sm">{order.customerNotes}</p>
            </div>
          </div>
        )}
      </div>

      {/* Line items */}
      <div className="space-y-2">
        <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Items
        </h3>
        {order.lines.map((line) => (
          <div
            key={line.id}
            className="flex items-center justify-between rounded-lg border px-4 py-3"
          >
            <div>
              <p className="font-medium">{line.itemName}</p>
              <p className="text-sm text-muted-foreground">
                {formatCurrency(line.unitPrice, order.displayCurrency)} x{" "}
                {line.quantity}
              </p>
            </div>
            <p className="font-semibold">
              {formatCurrency(
                line.unitPrice * line.quantity,
                order.displayCurrency
              )}
            </p>
          </div>
        ))}
      </div>

      {/* Total */}
      <div className="flex items-center justify-between rounded-lg border bg-muted/50 px-4 py-3 text-lg font-bold">
        <span>Total</span>
        <span>
          {formatCurrency(order.totalAmount, order.displayCurrency)}
        </span>
      </div>

      {/* Status actions */}
      {!isFinal && (
        <div className="flex gap-3">
          {nextStatus && (
            <Button
              size="lg"
              className="flex-1 text-base"
              disabled={statusMutation.isPending}
              onClick={() => statusMutation.mutate(nextStatus)}
            >
              {statusMutation.isPending && <Spinner className="h-5 w-5" />}
              Mark as {nextStatus}
            </Button>
          )}
          <Button
            variant="destructive"
            size="lg"
            disabled={statusMutation.isPending}
            onClick={() => statusMutation.mutate("Cancelled")}
          >
            Cancel
          </Button>
        </div>
      )}
    </div>
  );
}
