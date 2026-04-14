import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { useCreateMenu } from "@/hooks/useMenu";
import { useCategories } from "@/hooks/useCategory";
import MenuItemPicker from "@/components/menu/MenuItemPicker";
import type { InventoryItem } from "@/types/inventory";

export default function CreateMenuPage() {
  const navigate = useNavigate();
  const mutation = useCreateMenu();

  const [description, setDescription] = useState("");
  const [ownerId, setOwnerId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [selectedItems, setSelectedItems] = useState<InventoryItem[]>([]);
  const [requiredItemIds, setRequiredItemIds] = useState<Set<string>>(
    new Set()
  );

  const { data: categories, isLoading: categoriesLoading } =
    useCategories(ownerId);

  const handleAdd = (item: InventoryItem) => {
    setSelectedItems((prev) =>
      prev.some((i) => i.id === item.id) ? prev : [...prev, item]
    );
  };

  const handleRemove = (itemId: string) => {
    setSelectedItems((prev) => prev.filter((i) => i.id !== itemId));
    setRequiredItemIds((prev) => {
      const next = new Set(prev);
      next.delete(itemId);
      return next;
    });
  };

  const handleToggleRequired = (itemId: string) => {
    setRequiredItemIds((prev) => {
      const next = new Set(prev);
      if (next.has(itemId)) next.delete(itemId);
      else next.add(itemId);
      return next;
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutation.mutate(
      {
        description: description || undefined,
        ownerId,
        categoryId,
        inventoryItems: selectedItems.map((i) => ({
          inventoryItemId: i.id,
          isRequired: requiredItemIds.has(i.id),
        })),
      },
      {
        onSuccess: (data) => navigate(`/menus/${data.id}`),
      }
    );
  };

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/menus">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <h1 className="text-2xl font-bold">Create Menu</h1>
      </div>

      <form
        onSubmit={handleSubmit}
        className="space-y-6 rounded-lg border bg-background p-6"
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label
              htmlFor="ownerId"
              className="mb-1.5 block text-sm font-medium"
            >
              Owner ID
            </label>
            <Input
              id="ownerId"
              value={ownerId}
              onChange={(e) => {
                setOwnerId(e.target.value);
                setCategoryId("");
              }}
              placeholder="e.g. rest-001"
              required
            />
          </div>

          <div>
            <label
              htmlFor="categoryId"
              className="mb-1.5 block text-sm font-medium"
            >
              Category
            </label>
            <Select
              id="categoryId"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              required
            >
              <option value="" disabled>
                {!ownerId
                  ? "Enter an Owner ID first"
                  : categoriesLoading
                    ? "Loading categories..."
                    : categories && categories.length > 0
                      ? "Select a category"
                      : "No categories found"}
              </option>
              {categories?.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </Select>
          </div>

          <div className="sm:col-span-2">
            <label
              htmlFor="description"
              className="mb-1.5 block text-sm font-medium"
            >
              Description (optional)
            </label>
            <Input
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="A brief description of the menu"
            />
          </div>
        </div>

        <div>
          <h3 className="mb-3 text-sm font-medium">Menu Items</h3>
          <MenuItemPicker
            selectedItems={selectedItems}
            requiredItemIds={requiredItemIds}
            onAdd={handleAdd}
            onRemove={handleRemove}
            onToggleRequired={handleToggleRequired}
          />
        </div>

        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to create menu. Please try again.
          </p>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <Link to="/menus">
            <Button type="button" variant="outline">
              Cancel
            </Button>
          </Link>
          <Button type="submit" disabled={mutation.isPending || !categoryId}>
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            Create Menu
          </Button>
        </div>
      </form>
    </div>
  );
}
