import { useNavigate } from "react-router-dom";
import { Store, LogOut, User } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useRestaurant } from "@/context/RestaurantContext";
import { useCart } from "@/context/CartContext";
import { Button } from "@/components/ui/button";

export default function SettingsPage() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { retailerName } = useRestaurant();
  const { dispatch } = useCart();

  const handleLogout = () => {
    dispatch({ type: "CLEAR" });
    logout();
    navigate("/login");
  };

  return (
    <div className="flex flex-col gap-6 pb-24">
      <h1 className="text-xl font-bold">Settings</h1>

      {/* Logged-in user */}
      <div className="rounded-xl border p-4 space-y-3">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-muted">
            <User className="h-5 w-5 text-muted-foreground" />
          </div>
          <div>
            <p className="font-medium">{user?.fullName || "User"}</p>
            <p className="text-xs text-muted-foreground">{user?.email}</p>
          </div>
        </div>
      </div>

      {/* Current restaurant */}
      <div className="rounded-xl border p-4 space-y-3">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-muted">
            <Store className="h-5 w-5 text-muted-foreground" />
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Assigned Restaurant</p>
            <p className="font-medium">{retailerName || "Not selected"}</p>
            <p className="text-xs text-muted-foreground font-mono">
              {user?.retailerId}
            </p>
          </div>
        </div>
      </div>

      <Button
        variant="outline"
        className="w-full"
        onClick={handleLogout}
      >
        <LogOut className="h-4 w-4" />
        Log Out
      </Button>
    </div>
  );
}
