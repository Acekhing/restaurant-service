import { useRestaurant } from "@/context/RestaurantContext";
import BottomNav from "./BottomNav";
import { Store } from "lucide-react";

export default function AppShell({ children }: { children: React.ReactNode }) {
  const { retailerName } = useRestaurant();

  return (
    <div className="flex min-h-screen flex-col">
      {/* Top bar */}
      <header className="sticky top-0 z-40 flex h-12 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/80">
        <Store className="h-4 w-4 text-muted-foreground" />
        <span className="text-sm font-semibold truncate">
          {retailerName || "Waiter Portal"}
        </span>
      </header>

      {/* Page content */}
      <main className="flex-1 px-4 pt-4 pb-20">{children}</main>

      {/* Bottom navigation */}
      <BottomNav />
    </div>
  );
}
