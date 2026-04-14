import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/context/AuthContext";
import { RestaurantProvider } from "@/context/RestaurantContext";
import { CartProvider } from "@/context/CartContext";
import RequireAuth from "@/components/auth/RequireAuth";
import AppShell from "@/components/layout/AppShell";
import LoginPage from "@/pages/LoginPage";
import ScannerPage from "@/pages/ScannerPage";
import ReviewPage from "@/pages/ReviewPage";
import OrderTicketPage from "@/pages/OrderTicketPage";
import OrdersListPage from "@/pages/OrdersListPage";
import OrderDetailPage from "@/pages/OrderDetailPage";
import SettingsPage from "@/pages/SettingsPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 15_000,
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RestaurantProvider>
          <CartProvider>
            <BrowserRouter basename={import.meta.env.BASE_URL}>
              <Routes>
                <Route path="/login" element={<LoginPage />} />

                <Route
                  path="/scanner"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <ScannerPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />
                <Route
                  path="/orders/review"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <ReviewPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />
                <Route
                  path="/orders/:id/ticket"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <OrderTicketPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />
                <Route
                  path="/orders"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <OrdersListPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />
                <Route
                  path="/orders/:id"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <OrderDetailPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />
                <Route
                  path="/settings"
                  element={
                    <RequireAuth>
                      <AppShell>
                        <SettingsPage />
                      </AppShell>
                    </RequireAuth>
                  }
                />

                <Route path="/" element={<Navigate to="/login" replace />} />
                <Route path="*" element={<Navigate to="/login" replace />} />
              </Routes>
            </BrowserRouter>
          </CartProvider>
        </RestaurantProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}
