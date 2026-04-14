import { useState } from "react";
import { Tag, Plus, Pencil, Trash2, PowerOff } from "lucide-react";
import { useRetailerContext } from "@/context/RetailerContext";
import {
  usePromotions,
  useDeletePromotion,
  useDeactivatePromotion,
} from "@/hooks/useInventory";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { Dialog } from "@/components/ui/dialog";
import { formatDate } from "@/lib/utils";
import PromotionDialog from "@/components/inventory/PromotionDialog";
import type { Promotion } from "@/types/inventory";

export default function PromotionsPage() {
  const { retailerId } = useRetailerContext();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPromo, setEditingPromo] = useState<Promotion | null>(null);
  const [deletingPromo, setDeletingPromo] = useState<Promotion | null>(null);
  const { data, isLoading, isError } = usePromotions(retailerId);
  const deleteMutation = useDeletePromotion();
  const deactivateMutation = useDeactivatePromotion();

  const openEdit = (promo: Promotion) => {
    setEditingPromo(promo);
    setDialogOpen(true);
  };

  const openCreate = () => {
    setEditingPromo(null);
    setDialogOpen(true);
  };

  const handleDelete = () => {
    if (!deletingPromo) return;
    deleteMutation.mutate(deletingPromo.id, {
      onSuccess: () => setDeletingPromo(null),
    });
  };

  if (!retailerId) {
    return (
      <div className="mx-auto max-w-6xl">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <Tag className="mb-4 h-12 w-12 text-muted-foreground/40" />
          <h1 className="text-2xl font-bold">Promotions</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Select a retailer to view and manage promotions.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Promotions</h1>
          <p className="text-sm text-muted-foreground">
            Manage promotions for the selected retailer.
          </p>
        </div>
        <Button size="sm" onClick={openCreate}>
          <Plus className="h-4 w-4" />
          New Promotion
        </Button>
      </div>

      {isLoading && (
        <div className="flex justify-center py-12">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-8 text-center text-sm text-muted-foreground">
          Failed to load promotions.
        </p>
      )}

      {data && data.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-16 text-center">
          <Tag className="mb-3 h-10 w-10 text-muted-foreground/40" />
          <p className="text-sm text-muted-foreground">
            No promotions yet. Create your first promotion to get started.
          </p>
        </div>
      )}

      {data && data.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50 text-left text-muted-foreground">
                <th className="px-4 py-3 font-medium">Discount</th>
                <th className="px-4 py-3 font-medium">Scope</th>
                <th className="px-4 py-3 font-medium">From</th>
                <th className="px-4 py-3 font-medium">To</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Created</th>
                <th className="px-4 py-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.map((promo) => (
                <PromotionRow
                  key={promo.id}
                  promo={promo}
                  onEdit={() => openEdit(promo)}
                  onDelete={() => setDeletingPromo(promo)}
                  onDeactivate={() => deactivateMutation.mutate(promo.id)}
                  deactivating={deactivateMutation.isPending}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}

      <PromotionDialog
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingPromo(null);
        }}
        ownerId={retailerId}
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
    </div>
  );
}

function PromotionRow({
  promo,
  onEdit,
  onDelete,
  onDeactivate,
  deactivating,
}: {
  promo: Promotion;
  onEdit: () => void;
  onDelete: () => void;
  onDeactivate: () => void;
  deactivating: boolean;
}) {
  const now = new Date();
  const from = new Date(promo.effectiveFrom);
  const to = new Date(promo.effectiveTo);
  const isRunning = promo.isActive && now >= from && now <= to;
  const isExpired = now > to;

  const menuCount = promo.menuIds?.length ?? 0;
  const itemCount = promo.inventoryItemIds?.length ?? 0;

  return (
    <tr className="border-b last:border-0">
      <td className="px-4 py-3 font-medium">{promo.discountInPercentage}%</td>
      <td className="px-4 py-3">
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
      <td className="px-4 py-3 text-muted-foreground">
        {formatDate(promo.effectiveFrom)}
      </td>
      <td className="px-4 py-3 text-muted-foreground">
        {formatDate(promo.effectiveTo)}
      </td>
      <td className="px-4 py-3">
        {!promo.isActive ? (
          <Badge className="bg-gray-100 text-gray-600">Inactive</Badge>
        ) : isRunning ? (
          <Badge className="bg-green-100 text-green-800">Active</Badge>
        ) : isExpired ? (
          <Badge className="bg-gray-100 text-gray-600">Expired</Badge>
        ) : (
          <Badge className="bg-blue-100 text-blue-800">Scheduled</Badge>
        )}
      </td>
      <td className="px-4 py-3 text-muted-foreground">
        {formatDate(promo.createdAt)}
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-1">
          <button
            onClick={onEdit}
            className="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
            title="Edit"
          >
            <Pencil className="h-3.5 w-3.5" />
          </button>
          {promo.isActive && (
            <button
              onClick={onDeactivate}
              disabled={deactivating}
              className="rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
              title="Deactivate"
            >
              <PowerOff className="h-3.5 w-3.5" />
            </button>
          )}
          <button
            onClick={onDelete}
            className="rounded p-1 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
            title="Delete"
          >
            <Trash2 className="h-3.5 w-3.5" />
          </button>
        </div>
      </td>
    </tr>
  );
}
