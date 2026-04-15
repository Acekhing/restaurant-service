import { Link, NavLink } from "react-router-dom";
import { Briefcase, UtensilsCrossed, Barcode, Tag, Store, Activity, Users, Menu, X } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import RetailerSelector from "@/components/RetailerSelector";
import { useRetailerContext } from "@/context/RetailerContext";

const navItems = [
  { to: "/menu-editor", label: "Menu Editor", icon: UtensilsCrossed },
  { to: "/barcodes", label: "Barcodes", icon: Barcode },
  { to: "/promotions", label: "Promotions", icon: Tag },
  { to: "/retailers", label: "Retailers", icon: Store },
  { to: "/users", label: "Users", icon: Users },
  { to: "/stress-dashboard", label: "Stress Test", icon: Activity },
];

export default function AppShell({ children }: { children: React.ReactNode }) {
  const { retailerId, setRetailerId } = useRetailerContext();
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <div className="flex h-screen overflow-hidden bg-muted/40">
      <aside className="hidden w-56 flex-col border-r bg-background md:flex">
        <div className="flex h-14 items-center border-b px-5">
          <Link to="/" className="flex items-center gap-2 font-semibold">
            <Briefcase className="h-5 w-5" />
            <span>BackOffice / Admin</span>
          </Link>
        </div>
        <nav className="flex-1 space-y-1 p-3">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-accent text-accent-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                )
              }
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex h-14 items-center gap-4 border-b bg-background px-6">
          <Link to="/" className="flex items-center gap-2 font-semibold md:hidden">
            <Briefcase className="h-5 w-5" />
            <span>BackOffice / Admin</span>
          </Link>

          <div className="ml-auto flex items-center gap-3">
            <RetailerSelector
              value={retailerId}
              onChange={setRetailerId}
              className="w-52"
            />
            <button
              type="button"
              onClick={() => setMobileOpen((o) => !o)}
              className="rounded-md p-2 text-muted-foreground hover:bg-accent md:hidden"
            >
              {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>
          </div>
        </header>

        {mobileOpen && (
          <nav className="flex flex-col gap-1 border-b bg-background p-3 md:hidden">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={() => setMobileOpen(false)}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-accent text-accent-foreground"
                      : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                  )
                }
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            ))}
          </nav>
        )}

        <main className="flex-1 overflow-y-auto p-6">{children}</main>
      </div>
    </div>
  );
}
