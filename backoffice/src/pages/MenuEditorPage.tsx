import { NavLink, Outlet } from "react-router-dom";
import {
  UtensilsCrossed,
  Package,
  Layers,
  History,
  Clock,
  ToggleRight,
} from "lucide-react";
import { cn } from "@/lib/utils";

const tabs = [
  { to: "menus", label: "Menus", icon: UtensilsCrossed },
  { to: "items", label: "Inventory Items", icon: Package },
  { to: "varieties", label: "Varieties", icon: Layers },
  { to: "history", label: "Menu History", icon: History },
  { to: "timetable", label: "Timetable", icon: Clock },
  { to: "availability", label: "Availability", icon: ToggleRight },
];

export default function MenuEditorPage() {
  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <nav className="flex gap-1 overflow-x-auto border-b">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            end
            className={({ isActive }) =>
              cn(
                "flex shrink-0 items-center gap-2 border-b-2 px-4 py-2.5 text-sm font-medium transition-colors",
                isActive
                  ? "border-primary text-foreground"
                  : "border-transparent text-muted-foreground hover:border-muted-foreground/30 hover:text-foreground"
              )
            }
          >
            <tab.icon className="h-4 w-4" />
            {tab.label}
          </NavLink>
        ))}
      </nav>

      <Outlet />
    </div>
  );
}
