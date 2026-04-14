import { NavLink } from "react-router-dom";
import { ScanLine, ClipboardList, Settings } from "lucide-react";
import { cn } from "@/lib/utils";
import { useCart } from "@/context/CartContext";

const navItems = [
  { to: "/scanner", label: "Scanner", icon: ScanLine },
  { to: "/orders", label: "Orders", icon: ClipboardList },
  { to: "/settings", label: "Settings", icon: Settings },
];

export default function BottomNav() {
  const { totalItems } = useCart();

  return (
    <nav className="fixed inset-x-0 bottom-0 z-50 border-t bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80 safe-area-pb">
      <div className="flex h-16 items-center justify-around">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                "relative flex flex-col items-center gap-0.5 px-4 py-1 text-[10px] font-medium transition-colors",
                isActive
                  ? "text-primary"
                  : "text-muted-foreground"
              )
            }
          >
            <div className="relative">
              <item.icon className="h-6 w-6" />
              {item.to === "/scanner" && totalItems > 0 && (
                <span className="absolute -right-2 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white">
                  {totalItems}
                </span>
              )}
            </div>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
