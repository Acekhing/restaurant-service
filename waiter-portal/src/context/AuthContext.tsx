import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import type { LoginResponse } from "@/types/user";

interface AuthContextValue {
  user: LoginResponse | null;
  setUser: (user: LoginResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  setUser: () => {},
  logout: () => {},
});

const STORAGE_KEY = "waiter_auth_user";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUserState] = useState<LoginResponse | null>(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });

  useEffect(() => {
    if (user) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, [user]);

  const setUser = useCallback((u: LoginResponse) => {
    setUserState(u);
  }, []);

  const logout = useCallback(() => {
    setUserState(null);
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem("waiter_retailerId");
    localStorage.removeItem("waiter_retailerName");
  }, []);

  return (
    <AuthContext.Provider value={{ user, setUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
