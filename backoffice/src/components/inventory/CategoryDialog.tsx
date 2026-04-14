import { useState, useEffect } from "react";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { useCreateCategory, useUpdateCategory } from "@/hooks/useCategory";
import { useRetailers } from "@/hooks/useRetailer";
import type { Category } from "@/types/category";

interface Props {
  open: boolean;
  onClose: () => void;
  ownerId?: string;
  category?: Category | null;
}

export default function CategoryDialog({
  open,
  onClose,
  ownerId: preselectedOwnerId,
  category,
}: Props) {
  const isEdit = !!category;
  const [name, setName] = useState("");
  const [selectedOwnerId, setSelectedOwnerId] = useState(preselectedOwnerId ?? "");

  const { data: retailers, isLoading: retailersLoading } = useRetailers();
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const mutation = isEdit ? updateMutation : createMutation;

  useEffect(() => {
    if (open && category) {
      setName(category.name);
      setSelectedOwnerId(category.ownerId);
    } else if (open) {
      setName("");
      setSelectedOwnerId(preselectedOwnerId ?? "");
      createMutation.reset();
      updateMutation.reset();
    }
  }, [open, category, preselectedOwnerId]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = name.trim();
    if (!trimmed || !selectedOwnerId) return;

    if (isEdit && category) {
      updateMutation.mutate(
        { id: category.id, body: { name: trimmed } },
        {
          onSuccess: () => {
            onClose();
            setName("");
          },
        }
      );
    } else {
      createMutation.mutate(
        { name: trimmed, ownerId: selectedOwnerId },
        {
          onSuccess: () => {
            onClose();
            setName("");
          },
        }
      );
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={isEdit ? "Edit Category" : "Create Category"}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="cat-owner" className="mb-1.5 block text-sm font-medium">
            Owner
          </label>
          <Select
            id="cat-owner"
            value={selectedOwnerId}
            onChange={(e) => setSelectedOwnerId(e.target.value)}
            required
            disabled={isEdit}
          >
            <option value="" disabled>
              {retailersLoading ? "Loading retailers..." : "Select an owner"}
            </option>
            {retailers?.map((r) => (
              <option key={`${r.type}-${r.id}`} value={r.id}>
                {r.name} ({r.type})
              </option>
            ))}
          </Select>
        </div>

        <div>
          <label htmlFor="cat-name" className="mb-1.5 block text-sm font-medium">
            Category Name
          </label>
          <Input
            id="cat-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Beverages"
            required
            autoFocus
          />
        </div>

        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to {isEdit ? "update" : "create"} category. Please try again.
          </p>
        )}

        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={mutation.isPending || !name.trim() || !selectedOwnerId}
          >
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            {isEdit ? "Update Category" : "Create Category"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
