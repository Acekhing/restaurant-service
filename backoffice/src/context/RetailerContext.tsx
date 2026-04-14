import { createContext, useContext, useState, type ReactNode } from "react";

interface RetailerContextValue {
  retailerId: string;
  setRetailerId: (id: string) => void;
}

const RetailerContext = createContext<RetailerContextValue | null>(null);

export function RetailerProvider({ children }: { children: ReactNode }) {
  const [retailerId, setRetailerId] = useState("");

  return (
    <RetailerContext.Provider value={{ retailerId, setRetailerId }}>
      {children}
    </RetailerContext.Provider>
  );
}

export function useRetailerContext(): RetailerContextValue {
  const ctx = useContext(RetailerContext);
  if (!ctx) {
    throw new Error("useRetailerContext must be used within a RetailerProvider");
  }
  return ctx;
}
