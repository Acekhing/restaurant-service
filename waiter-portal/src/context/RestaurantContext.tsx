import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { useAuth } from "@/context/AuthContext";

interface RestaurantContextValue {
  retailerId: string;
  retailerName: string;
  setRestaurant: (id: string, name: string) => void;
  clear: () => void;
}

const RestaurantContext = createContext<RestaurantContextValue>({
  retailerId: "",
  retailerName: "",
  setRestaurant: () => {},
  clear: () => {},
});

export function RestaurantProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();

  const [retailerId, setRetailerId] = useState(
    () => localStorage.getItem("waiter_retailerId") ?? ""
  );
  const [retailerName, setRetailerName] = useState(
    () => localStorage.getItem("waiter_retailerName") ?? ""
  );

  useEffect(() => {
    if (user) {
      setRetailerId(user.retailerId);
      setRetailerName(user.retailerName ?? "Restaurant");
    }
  }, [user]);

  useEffect(() => {
    localStorage.setItem("waiter_retailerId", retailerId);
    localStorage.setItem("waiter_retailerName", retailerName);
  }, [retailerId, retailerName]);

  const setRestaurant = (id: string, name: string) => {
    setRetailerId(id);
    setRetailerName(name);
  };

  const clear = () => {
    setRetailerId("");
    setRetailerName("");
  };

  return (
    <RestaurantContext.Provider
      value={{ retailerId, retailerName, setRestaurant, clear }}
    >
      {children}
    </RestaurantContext.Provider>
  );
}

export function useRestaurant() {
  return useContext(RestaurantContext);
}
