import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RetailerProvider } from "@/context/RetailerContext";
import AppShell from "@/components/layout/AppShell";
import MenuEditorPage from "@/pages/MenuEditorPage";
import MenusTab from "@/pages/menu-editor/MenusTab";
import InventoryItemsTab from "@/pages/menu-editor/InventoryItemsTab";
import VarietiesTab from "@/pages/menu-editor/VarietiesTab";
import MenuHistoryTab from "@/pages/menu-editor/MenuHistoryTab";
import TimetableTab from "@/pages/menu-editor/TimetableTab";
import AvailabilityTab from "@/pages/menu-editor/AvailabilityTab";
import PromotionsPage from "@/pages/PromotionsPage";
import AuditLogPage from "@/pages/AuditLogPage";
import AddItemPage from "@/pages/AddItemPage";
import ItemDetailPage from "@/pages/ItemDetailPage";
import CreateMenuPage from "@/pages/CreateMenuPage";
import MenuDetailPage from "@/pages/MenuDetailPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 30_000,
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <RetailerProvider>
        <BrowserRouter>
          <AppShell>
            <Routes>
              <Route path="/" element={<Navigate to="/menu-editor/menus" replace />} />

              <Route path="/menu-editor" element={<MenuEditorPage />}>
                <Route index element={<Navigate to="menus" replace />} />
                <Route path="menus" element={<MenusTab />} />
                <Route path="items" element={<InventoryItemsTab />} />
                <Route path="varieties" element={<VarietiesTab />} />
                <Route path="history" element={<MenuHistoryTab />} />
                <Route path="timetable" element={<TimetableTab />} />
                <Route path="availability" element={<AvailabilityTab />} />
              </Route>

              <Route path="/promotions" element={<PromotionsPage />} />
              <Route path="/audit" element={<AuditLogPage />} />

              <Route path="/items/new" element={<AddItemPage />} />
              <Route path="/items/:id" element={<ItemDetailPage />} />
              <Route path="/menus/new" element={<CreateMenuPage />} />
              <Route path="/menus/:id" element={<MenuDetailPage />} />
            </Routes>
          </AppShell>
        </BrowserRouter>
      </RetailerProvider>
    </QueryClientProvider>
  );
}
