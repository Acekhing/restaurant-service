import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ClipboardList, Clock } from "lucide-react";
import { useOrders } from "@/hooks/useOrders";
import { useRestaurant } from "@/context/RestaurantContext";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency, formatTime } from "@/lib/utils";
import { cn } from "@/lib/utils";
import type { OrderStatus } from "@/types/order";

const statusFilters: { key: string; label: string }[] = [
  { key: "", label: "All" },
  { key: "Pending", label: "Pending" },
  { key: "Confirmed", label: "Confirmed" },
  { key: "Preparing", label: "Preparing" },
  { key: "Ready", label: "Ready" },
  { key: "Served", label: "Served" },
  { key: "Cancelled", label: "Cancelled" },
];

const statusColors: Record<OrderStatus, string> = {
  Pending: "bg-yellow-100 text-yellow-800",
  Confirmed: "bg-blue-100 text-blue-800",
  Preparing: "bg-orange-100 text-orange-800",
  Ready: "bg-green-100 text-green-800",
  Served: "bg-gray-100 text-gray-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function OrdersListPage() {
  const navigate = useNavigate();
  const { retailerId } = useRestaurant();
  const [statusFilter, setStatusFilter] = useState("");
  const today = new Date().toISOString().split("T")[0];

  const { data: orders, isLoading } = useOrders({
    retailerId,
    status: statusFilter || undefined,
    date: today,
  });

  if (!retailerId) {
    navigate("/", { replace: true });
    return null;
  }

  return (
    <div className="flex flex-col gap-4 pb-24">
      <div className="flex items-center gap-2">
        <ClipboardList className="h-5 w-5 text-muted-foreground" />
        <h1 className="text-xl font-bold">Today's Orders</h1>
      </div>

      {/* Status filter pills */}
      <div className="flex gap-2 overflow-x-auto pb-1 -mx-1 px-1">
        {statusFilters.map((f) => (
          <button
            key={f.key}
            onClick={() => setStatusFilter(f.key)}
            className={cn(
              "shrink-0 rounded-full px-3 py-1.5 text-xs font-medium transition-colors",
              statusFilter === f.key
                ? "bg-primary text-primary-foreground"
                : "bg-muted text-muted-foreground hover:bg-accent"
            )}
          >
            {f.label}
          </button>
        ))}
      </div>

      {/* Orders list */}
      {isLoading ? (
        <div className="flex justify-center py-12">
          <Spinner />
        </div>
      ) : !orders || orders.length === 0 ? (
        <div className="flex flex-col items-center gap-2 py-12">
          <ClipboardList className="h-10 w-10 text-muted-foreground/30" />
          <p className="text-sm text-muted-foreground">
            No orders {statusFilter ? `with status "${statusFilter}"` : "today"}.
          </p>
        </div>
      ) : (
        <div className="space-y-2">
          {orders.map((order) => (
            <button
              key={order.id}
              onClick={() => navigate(`/orders/${order.id}`)}
              className="flex w-full items-center justify-between rounded-lg border bg-background px-4 py-3 text-left transition-colors hover:bg-accent/50 active:bg-accent"
            >
              <div className="flex items-center gap-4">
                <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-muted font-bold text-lg">
                  {order.orderNumber}
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <Badge
                      className={
                        statusColors[order.status as OrderStatus] ??
                        "bg-gray-100 text-gray-800"
                      }
                    >
                      {order.status}
                    </Badge>
                    {order.tableNumber && (
                      <span className="text-xs text-muted-foreground">
                        Table {order.tableNumber}
                      </span>
                    )}
                  </div>
                  <div className="mt-1 flex items-center gap-2 text-xs text-muted-foreground">
                    <Clock className="h-3 w-3" />
                    {formatTime(order.createdAt)}
                    <span>&middot;</span>
                    <span>
                      {order.lines.reduce((s, l) => s + l.quantity, 0)} items
                    </span>
                    {order.waiterName && (
                      <>
                        <span>&middot;</span>
                        <span>{order.waiterName}</span>
                      </>
                    )}
                  </div>
                </div>
              </div>
              <span className="font-semibold">
                {formatCurrency(order.totalAmount, order.displayCurrency)}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
