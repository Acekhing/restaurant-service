import { useState } from "react";
import { Dialog } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useUpdateAvailability } from "@/hooks/useInventory";

interface Props {
  open: boolean;
  onClose: () => void;
  itemId: string;
  currentAvailable: boolean;
  currentOutOfStock: boolean;
}

export default function AvailabilityDialog({
  open,
  onClose,
  itemId,
  currentAvailable,
  currentOutOfStock,
}: Props) {
  const [isAvailable, setIsAvailable] = useState(currentAvailable);
  const [outOfStock, setOutOfStock] = useState(currentOutOfStock);
  const mutation = useUpdateAvailability(itemId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(
      { isAvailable, outOfStock },
      {
        onSuccess: () => {
          onClose();
          mutation.reset();
        },
      }
    );
  };

  return (
    <Dialog open={open} onClose={onClose} title="Update Availability">
      <form onSubmit={handleSubmit} className="space-y-4">
        <label className="flex items-center gap-3 rounded-md border p-3">
          <input
            type="checkbox"
            checked={isAvailable}
            onChange={(e) => setIsAvailable(e.target.checked)}
            className="h-4 w-4 rounded border-gray-300"
          />
          <div>
            <p className="text-sm font-medium">Available</p>
            <p className="text-xs text-muted-foreground">
              Item is visible and can be ordered
            </p>
          </div>
        </label>
        <label className="flex items-center gap-3 rounded-md border p-3">
          <input
            type="checkbox"
            checked={outOfStock}
            onChange={(e) => setOutOfStock(e.target.checked)}
            className="h-4 w-4 rounded border-gray-300"
          />
          <div>
            <p className="text-sm font-medium">Out of Stock</p>
            <p className="text-xs text-muted-foreground">
              Mark item as temporarily out of stock
            </p>
          </div>
        </label>
        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to update availability. Please try again.
          </p>
        )}
        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            Save
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
