import { useState, useEffect } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  useCreatePromotion,
  useUpdatePromotion,
} from "@/hooks/useInventory";
import { useInventoryItemsByOwner } from "@/hooks/useVariety";
import { useMenusByOwner } from "@/hooks/useMenu";
import type { Promotion } from "@/types/inventory";

interface Props {
  open: boolean;
  onClose: () => void;
  ownerId: string;
  currency: string;
  promotion?: Promotion | null;
}

export default function PromotionDialog({
  open,
  onClose,
  ownerId,
  currency,
  promotion,
}: Props) {
  const isEdit = !!promotion;

  const [discount, setDiscount] = useState("");
  const [from, setFrom] = useState<Date | null>(null);
  const [to, setTo] = useState<Date | null>(null);
  const [isAppliedToMenu, setIsAppliedToMenu] = useState(false);
  const [isAppliedToItems, setIsAppliedToItems] = useState(false);
  const [isFreeDelivery, setIsFreeDelivery] = useState(false);
  const [selectedItemIds, setSelectedItemIds] = useState<Set<string>>(
    new Set()
  );
  const [selectedMenuIds, setSelectedMenuIds] = useState<Set<string>>(
    new Set()
  );

  const createMutation = useCreatePromotion();
  const updateMutation = useUpdatePromotion();
  const mutation = isEdit ? updateMutation : createMutation;

  const { data: ownerItems, isLoading: itemsLoading } =
    useInventoryItemsByOwner(isAppliedToItems && !isFreeDelivery ? ownerId : "");
  const { data: ownerMenus, isLoading: menusLoading } =
    useMenusByOwner(isAppliedToMenu && !isFreeDelivery ? ownerId : "");

  useEffect(() => {
    if (open && promotion) {
      setDiscount(String(promotion.discountInPercentage));
      setFrom(new Date(promotion.effectiveFrom));
      setTo(new Date(promotion.effectiveTo));
      setIsAppliedToMenu(promotion.isAppliedToMenu);
      setIsAppliedToItems(promotion.isAppliedToItems);
      setIsFreeDelivery(promotion.isFreeDelivery);
      setSelectedItemIds(
        new Set(promotion.inventoryItemIds ?? [])
      );
      setSelectedMenuIds(new Set(promotion.menuIds ?? []));
    } else if (open) {
      resetForm();
    }
  }, [open, promotion]);

  const resetForm = () => {
    setDiscount("");
    setFrom(null);
    setTo(null);
    setIsAppliedToMenu(false);
    setIsAppliedToItems(false);
    setIsFreeDelivery(false);
    setSelectedItemIds(new Set());
    setSelectedMenuIds(new Set());
    createMutation.reset();
    updateMutation.reset();
  };

  const toggleItem = (id: string) => {
    setSelectedItemIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const toggleMenu = (id: string) => {
    setSelectedMenuIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!from || !to) return;

    const itemIds =
      isAppliedToItems && !isFreeDelivery && selectedItemIds.size > 0
        ? Array.from(selectedItemIds)
        : undefined;
    const menuIds =
      isAppliedToMenu && !isFreeDelivery && selectedMenuIds.size > 0
        ? Array.from(selectedMenuIds)
        : undefined;

    if (isEdit && promotion) {
      updateMutation.mutate(
        {
          id: promotion.id,
          body: {
            discountInPercentage: parseInt(discount, 10),
            currency,
            effectiveFrom: from.toISOString(),
            effectiveTo: to.toISOString(),
            isAppliedToMenu,
            isAppliedToItems,
            isFreeDelivery,
            inventoryItemIds: itemIds,
            menuIds,
          },
        },
        {
          onSuccess: () => {
            onClose();
            resetForm();
          },
        }
      );
    } else {
      createMutation.mutate(
        {
          ownerId,
          discountInPercentage: parseInt(discount, 10),
          currency,
          effectiveFrom: from.toISOString(),
          effectiveTo: to.toISOString(),
          isAppliedToMenu,
          isAppliedToItems,
          isFreeDelivery,
          inventoryItemIds: itemIds,
          menuIds,
        },
        {
          onSuccess: () => {
            onClose();
            resetForm();
          },
        }
      );
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={isEdit ? "Edit Promotion" : "Run Promotion"}
      className="max-w-lg"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label
            htmlFor="discount"
            className="mb-1.5 block text-sm font-medium"
          >
            Discount (%)
          </label>
          <Input
            id="discount"
            type="number"
            min="1"
            max="100"
            step="1"
            value={discount}
            onChange={(e) => setDiscount(e.target.value)}
            placeholder="e.g. 15"
            required
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="mb-1.5 block text-sm font-medium">From</label>
            <DatePicker
              selected={from}
              onChange={(date) => setFrom(date)}
              showTimeSelect
              timeIntervals={15}
              dateFormat="dd MMM yyyy, h:mm aa"
              placeholderText="Pick start date & time"
              minDate={new Date()}
              className="flex h-9 w-full rounded-md border bg-background px-3 py-1 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              required
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium">To</label>
            <DatePicker
              selected={to}
              onChange={(date) => setTo(date)}
              showTimeSelect
              timeIntervals={15}
              dateFormat="dd MMM yyyy, h:mm aa"
              placeholderText="Pick end date & time"
              minDate={from ?? new Date()}
              className="flex h-9 w-full rounded-md border bg-background px-3 py-1 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              required
            />
          </div>
        </div>

        <fieldset className="space-y-3 rounded-md border p-3">
          <legend className="px-1 text-sm font-medium">Apply to</legend>

          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={isFreeDelivery}
              onChange={(e) => setIsFreeDelivery(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300"
            />
            Free Delivery
          </label>

          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={isAppliedToItems}
              onChange={(e) => setIsAppliedToItems(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300"
            />
            Specific Items
          </label>

          {isAppliedToItems && !isFreeDelivery && (
            <div className="ml-6 max-h-40 space-y-1 overflow-y-auto rounded-md border p-2">
              {itemsLoading ? (
                <div className="flex justify-center py-2">
                  <Spinner className="h-4 w-4" />
                </div>
              ) : ownerItems && ownerItems.length > 0 ? (
                ownerItems.map((item) => (
                  <label
                    key={item.id}
                    className="flex items-center gap-2 rounded px-2 py-1 text-xs hover:bg-muted/50"
                  >
                    <input
                      type="checkbox"
                      checked={selectedItemIds.has(item.id)}
                      onChange={() => toggleItem(item.id)}
                      className="h-3.5 w-3.5 rounded border-gray-300"
                    />
                    <span className="truncate">{item.name}</span>
                  </label>
                ))
              ) : (
                <p className="py-2 text-center text-xs text-muted-foreground">
                  No items found for this owner.
                </p>
              )}
            </div>
          )}

          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={isAppliedToMenu}
              onChange={(e) => setIsAppliedToMenu(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300"
            />
            Menus
          </label>

          {isAppliedToMenu && !isFreeDelivery && (
            <div className="ml-6 max-h-40 space-y-1 overflow-y-auto rounded-md border p-2">
              {menusLoading ? (
                <div className="flex justify-center py-2">
                  <Spinner className="h-4 w-4" />
                </div>
              ) : ownerMenus && ownerMenus.length > 0 ? (
                ownerMenus.map((menu) => (
                  <label
                    key={menu.id}
                    className="flex items-center gap-2 rounded px-2 py-1 text-xs hover:bg-muted/50"
                  >
                    <input
                      type="checkbox"
                      checked={selectedMenuIds.has(menu.id)}
                      onChange={() => toggleMenu(menu.id)}
                      className="h-3.5 w-3.5 rounded border-gray-300"
                    />
                    <span className="truncate">{menu.categoryName ?? menu.id}</span>
                  </label>
                ))
              ) : (
                <p className="py-2 text-center text-xs text-muted-foreground">
                  No menus found for this owner.
                </p>
              )}
            </div>
          )}
        </fieldset>

        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to {isEdit ? "update" : "create"} promotion. Please try
            again.
          </p>
        )}
        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={mutation.isPending || !from || !to}
          >
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            {isEdit ? "Update Promotion" : "Create Promotion"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
