import { useState } from "react";
import { Pencil, Trash2, PowerOff } from "lucide-react";
import {
  usePromotions,
  useDeletePromotion,
  useDeactivatePromotion,
} from "@/hooks/useInventory";
import { Spinner } from "@/components/ui/spinner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import { formatDate } from "@/lib/utils";
import PromotionDialog from "@/components/inventory/PromotionDialog";
import type { Promotion } from "@/types/inventory";

export default function PromotionHistory({ ownerId }: { ownerId: string }) {
  const { data, isLoading, isError } = usePromotions(ownerId);
  const deleteMutation = useDeletePromotion();
  const deactivateMutation = useDeactivatePromotion();
  const [editingPromo, setEditingPromo] = useState<Promotion | null>(null);
  const [deletingPromo, setDeletingPromo] = useState<Promotion | null>(null);

  const handleDelete = () => {
    if (!deletingPromo) return;
    deleteMutation.mutate(deletingPromo.id, {
      onSuccess: () => setDeletingPromo(null),
    });
  };

  if (isLoading)
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    );

  if (isError)
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        Failed to load promotions.
      </p>
    );

  if (!data || data.length === 0)
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        No promotions have been created yet.
      </p>
    );

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-muted-foreground">
              <th className="pb-2 pr-4 font-medium">Discount</th>
              <th className="pb-2 pr-4 font-medium">Scope</th>
              <th className="pb-2 pr-4 font-medium">From</th>
              <th className="pb-2 pr-4 font-medium">To</th>
              <th className="pb-2 pr-4 font-medium">Status</th>
              <th className="pb-2 pr-4 font-medium">Created</th>
              <th className="pb-2 font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {data.map((promo) => {
              const now = new Date();
              const from = new Date(promo.effectiveFrom);
              const to = new Date(promo.effectiveTo);
              const isRunning = promo.isActive && now >= from && now <= to;
              const isExpired = now > to;

              const menuCount = promo.menuIds?.length ?? 0;
              const itemCount = promo.inventoryItemIds?.length ?? 0;

              return (
                <tr key={promo.id} className="border-b last:border-0">
                  <td className="py-3 pr-4 font-medium">
                    {promo.discountInPercentage}%
                  </td>
                  <td className="py-3 pr-4">
                    <div className="flex flex-wrap gap-1">
                      {promo.isAppliedToMenu && (
                        <Badge className="bg-purple-100 text-purple-800">
                          {menuCount > 0 ? `${menuCount} Menu${menuCount > 1 ? "s" : ""}` : "Menu"}
                        </Badge>
                      )}
                      {promo.isAppliedToItems && (
                        <Badge className="bg-indigo-100 text-indigo-800">
                          {itemCount > 0 ? `${itemCount} Item${itemCount > 1 ? "s" : ""}` : "Items"}
                        </Badge>
                      )}
                      {promo.isFreeDelivery && (
                        <Badge className="bg-amber-100 text-amber-800">
                          Free Delivery
                        </Badge>
                      )}
                    </div>
                  </td>
                  <td className="py-3 pr-4 text-muted-foreground">
                    {formatDate(promo.effectiveFrom)}
                  </td>
                  <td className="py-3 pr-4 text-muted-foreground">
                    {formatDate(promo.effectiveTo)}
                  </td>
                  <td className="py-3 pr-4">
                    {!promo.isActive ? (
                      <Badge className="bg-gray-100 text-gray-600">
                        Inactive
                      </Badge>
                    ) : isRunning ? (
                      <Badge className="bg-green-100 text-green-800">
                        Active
                      </Badge>
                    ) : isExpired ? (
                      <Badge className="bg-gray-100 text-gray-600">
                        Expired
                      </Badge>
                    ) : (
                      <Badge className="bg-blue-100 text-blue-800">
                        Scheduled
                      </Badge>
                    )}
                  </td>
                  <td className="py-3 pr-4 text-muted-foreground">
                    {formatDate(promo.createdAt)}
                  </td>
                  <td className="py-3">
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => setEditingPromo(promo)}
                        className="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
                        title="Edit"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </button>
                      {promo.isActive && (
                        <button
                          onClick={() => deactivateMutation.mutate(promo.id)}
                          disabled={deactivateMutation.isPending}
                          className="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
                          title="Deactivate"
                        >
                          <PowerOff className="h-3.5 w-3.5" />
                        </button>
                      )}
                      <button
                        onClick={() => setDeletingPromo(promo)}
                        className="rounded p-1 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                        title="Delete"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <PromotionDialog
        open={!!editingPromo}
        onClose={() => setEditingPromo(null)}
        ownerId={ownerId}
        currency="GHS"
        promotion={editingPromo}
      />

      <Dialog
        open={!!deletingPromo}
        onClose={() => setDeletingPromo(null)}
        title="Delete Promotion"
      >
        <div className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete this promotion? This action cannot
            be undone.
          </p>
          {deleteMutation.isError && (
            <p className="text-sm text-destructive">
              Failed to delete promotion.
            </p>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setDeletingPromo(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={handleDelete}
            >
              {deleteMutation.isPending && <Spinner className="h-4 w-4" />}
              Delete
            </Button>
          </div>
        </div>
      </Dialog>
    </>
  );
}
