import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { useCreateItem } from "@/hooks/useInventory";
import { useRetailers } from "@/hooks/useRetailer";
import type { InventoryOpeningHours } from "@/types/inventory";
import type { RetailerType } from "@/types/retailer";

const DAYS = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

function defaultOpeningHours(): InventoryOpeningHours[] {
  return DAYS.map((day) => ({
    id: crypto.randomUUID(),
    day,
    openingTime: "08:00",
    closingTime: "17:00",
    isAvailable: true,
  }));
}

export default function AddItemPage() {
  const navigate = useNavigate();
  const mutation = useCreateItem();

  const [name, setName] = useState("");
  const [ownerId, setOwnerId] = useState("");
  const [supplierPrice, setSupplierPrice] = useState("");
  const [deliveryFee, setDeliveryFee] = useState("");
  const [itemType, setItemType] = useState<RetailerType>("restaurant");
  const [avgPrepTime, setAvgPrepTime] = useState("");
  const { data: retailers, isLoading: retailersLoading } = useRetailers(itemType);
  const [openingHours, setOpeningHours] = useState<InventoryOpeningHours[]>(defaultOpeningHours);

  const updateHour = (idx: number, patch: Partial<InventoryOpeningHours>) => {
    setOpeningHours((prev) =>
      prev.map((h, i) => (i === idx ? { ...h, ...patch } : h))
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(
      {
        name,
        ownerId,
        supplierPrice: parseFloat(supplierPrice),
        deliveryFee: parseFloat(deliveryFee) || 0,
        itemType,
        averagePreparationTime: avgPrepTime ? parseInt(avgPrepTime, 10) : undefined,
        openingDayHours: openingHours,
      },
      {
        onSuccess: (data) => navigate(`/items/${data.id}`),
      }
    );
  };

  return (
    <div className="mx-auto max-w-lg space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <h1 className="text-2xl font-bold">Add Inventory Item</h1>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-4 rounded-lg border bg-background p-6"
      >
        <div>
          <label htmlFor="name" className="mb-1.5 block text-sm font-medium">
            Name
          </label>
          <Input
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Jollof Rice with Chicken"
            required
          />
        </div>

        <div>
          <label htmlFor="itemType" className="mb-1.5 block text-sm font-medium">
            Item Type
          </label>
          <Select
            id="itemType"
            value={itemType}
            onChange={(e) => {
              setItemType(e.target.value as RetailerType);
              setOwnerId("");
            }}
          >
            <option value="restaurant">Restaurant</option>
            <option value="pharmacy">Pharmacy</option>
            <option value="shop">Shop</option>
          </Select>
        </div>

        <div>
          <label htmlFor="ownerId" className="mb-1.5 block text-sm font-medium">
            Owner
          </label>
          <Select
            id="ownerId"
            value={ownerId}
            onChange={(e) => setOwnerId(e.target.value)}
            required
            disabled={retailersLoading}
          >
            <option value="">
              {retailersLoading ? "Loading..." : "Select an owner"}
            </option>
            {retailers?.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name}
              </option>
            ))}
          </Select>
        </div>

        <div>
          <label htmlFor="price" className="mb-1.5 block text-sm font-medium">
            Supplier Price (GHS)
          </label>
          <Input
            id="price"
            type="number"
            min="0"
            step="0.01"
            value={supplierPrice}
            onChange={(e) => setSupplierPrice(e.target.value)}
            placeholder="e.g. 45.00"
            required
          />
        </div>

        <div>
          <label htmlFor="deliveryFee" className="mb-1.5 block text-sm font-medium">
            Delivery Fee (GHS)
          </label>
          <Input
            id="deliveryFee"
            type="number"
            min="0"
            step="0.01"
            value={deliveryFee}
            onChange={(e) => setDeliveryFee(e.target.value)}
            placeholder="e.g. 5.00"
          />
        </div>

        <div>
          <label htmlFor="avgPrepTime" className="mb-1.5 block text-sm font-medium">
            Average Preparation Time (minutes)
          </label>
          <Input
            id="avgPrepTime"
            type="number"
            min="0"
            step="1"
            value={avgPrepTime}
            onChange={(e) => setAvgPrepTime(e.target.value)}
            placeholder="e.g. 30"
          />
        </div>

        <div className="space-y-3">
          <h3 className="text-sm font-medium">Opening Hours</h3>
          {openingHours.map((h, idx) => (
            <div key={h.id} className="flex items-center gap-2">
              <label className="w-24 shrink-0 text-sm">{h.day}</label>
              <input
                type="checkbox"
                checked={h.isAvailable}
                onChange={(e) => updateHour(idx, { isAvailable: e.target.checked })}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Input
                type="time"
                value={h.openingTime}
                onChange={(e) => updateHour(idx, { openingTime: e.target.value })}
                disabled={!h.isAvailable}
                className="w-28"
              />
              <span className="text-xs text-muted-foreground">to</span>
              <Input
                type="time"
                value={h.closingTime}
                onChange={(e) => updateHour(idx, { closingTime: e.target.value })}
                disabled={!h.isAvailable}
                className="w-28"
              />
            </div>
          ))}
        </div>

        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to create item. Please try again.
          </p>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <Link to="/">
            <Button type="button" variant="outline">
              Cancel
            </Button>
          </Link>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            Create Item
          </Button>
        </div>
      </form>
    </div>
  );
}
