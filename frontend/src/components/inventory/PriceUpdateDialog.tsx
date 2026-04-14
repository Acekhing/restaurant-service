import { useState } from "react";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useUpdatePrice } from "@/hooks/useInventory";
import { formatCurrency } from "@/lib/utils";

interface Props {
  open: boolean;
  onClose: () => void;
  itemId: string;
  currentPrice: number;
  currency: string;
}

export default function PriceUpdateDialog({
  open,
  onClose,
  itemId,
  currentPrice,
  currency,
}: Props) {
  const [price, setPrice] = useState(String(currentPrice));
  const mutation = useUpdatePrice(itemId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(
      { displayPrice: parseFloat(price) },
      {
        onSuccess: () => {
          onClose();
          mutation.reset();
        },
      }
    );
  };

  return (
    <Dialog open={open} onClose={onClose} title="Update Price">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1.5 block text-sm font-medium">
            Current Price
          </label>
          <p className="text-sm text-muted-foreground">
            {formatCurrency(currentPrice, currency)}
          </p>
        </div>
        <div>
          <label htmlFor="newPrice" className="mb-1.5 block text-sm font-medium">
            New Price ({currency})
          </label>
          <Input
            id="newPrice"
            type="number"
            min="0"
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            required
          />
        </div>
        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to update price. Please try again.
          </p>
        )}
        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            Update Price
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
