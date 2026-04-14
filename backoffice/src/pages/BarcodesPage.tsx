import { useState } from "react";
import Barcode from "react-barcode";
import { Printer, Wand2, Package, UtensilsCrossed } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { Badge } from "@/components/ui/badge";
import { useRetailerContext } from "@/context/RetailerContext";
import {
  useListItems,
  useGenerateInventoryItemCodes,
} from "@/hooks/useInventory";
import {
  useMenusByOwner,
  useGenerateMenuCodes,
} from "@/hooks/useMenu";

type Tab = "items" | "menus";

export default function BarcodesPage() {
  const { retailerId } = useRetailerContext();
  const [activeTab, setActiveTab] = useState<Tab>("items");
  const [generateResult, setGenerateResult] = useState<string | null>(null);

  const {
    data: items,
    isLoading: itemsLoading,
    isError: itemsError,
  } = useListItems(retailerId);

  const {
    data: menus,
    isLoading: menusLoading,
    isError: menusError,
  } = useMenusByOwner(retailerId);

  const generateItemCodes = useGenerateInventoryItemCodes();
  const generateMenuCodesMut = useGenerateMenuCodes();

  const isGenerating =
    generateItemCodes.isPending || generateMenuCodesMut.isPending;

  const handleGenerateCodes = async () => {
    setGenerateResult(null);
    const [itemResult, menuResult] = await Promise.all([
      generateItemCodes.mutateAsync(),
      generateMenuCodesMut.mutateAsync(),
    ]);
    const total = itemResult.generated + menuResult.generated;
    if (total === 0) {
      setGenerateResult("All items and menus already have codes.");
    } else {
      setGenerateResult(
        `Generated ${itemResult.generated} item code${itemResult.generated !== 1 ? "s" : ""} and ${menuResult.generated} menu code${menuResult.generated !== 1 ? "s" : ""}.`
      );
    }
  };

  const handlePrint = () => window.print();

  const itemsMissingCode = items?.filter((i) => !i.inventoryItemCode).length ?? 0;
  const menusMissingCode = menus?.filter((m) => !m.menuItemCode).length ?? 0;
  const totalMissing = itemsMissingCode + menusMissingCode;

  if (!retailerId) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <Package className="mb-3 h-10 w-10 text-muted-foreground/50" />
        <p className="text-sm text-muted-foreground">
          Select a retailer to view barcodes.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between print:hidden">
        <div>
          <h1 className="text-2xl font-bold">Barcodes</h1>
          <p className="text-sm text-muted-foreground">
            View and print Code 128 barcodes for inventory items and menus
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button
            onClick={handleGenerateCodes}
            disabled={isGenerating || totalMissing === 0}
            variant="outline"
          >
            {isGenerating ? (
              <Spinner className="h-4 w-4" />
            ) : (
              <Wand2 className="h-4 w-4" />
            )}
            Generate Missing Codes
            {totalMissing > 0 && (
              <Badge className="ml-1 bg-amber-100 text-amber-800">
                {totalMissing}
              </Badge>
            )}
          </Button>
          <Button onClick={handlePrint} variant="outline">
            <Printer className="h-4 w-4" />
            Print
          </Button>
        </div>
      </div>

      {/* Result message */}
      {generateResult && (
        <div className="rounded-md border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800 print:hidden">
          {generateResult}
        </div>
      )}

      {/* Tabs */}
      <div className="flex gap-1 rounded-lg border bg-muted/50 p-1 print:hidden">
        <button
          type="button"
          onClick={() => setActiveTab("items")}
          className={`flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === "items"
              ? "bg-background text-foreground shadow-sm"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          <Package className="h-4 w-4" />
          Inventory Items
          {items && (
            <Badge className="bg-muted text-muted-foreground">{items.length}</Badge>
          )}
        </button>
        <button
          type="button"
          onClick={() => setActiveTab("menus")}
          className={`flex items-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === "menus"
              ? "bg-background text-foreground shadow-sm"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          <UtensilsCrossed className="h-4 w-4" />
          Menus
          {menus && (
            <Badge className="bg-muted text-muted-foreground">{menus.length}</Badge>
          )}
        </button>
      </div>

      {/* Content */}
      {activeTab === "items" && (
        <ItemsSection items={items} isLoading={itemsLoading} isError={itemsError} />
      )}
      {activeTab === "menus" && (
        <MenusSection menus={menus} isLoading={menusLoading} isError={menusError} />
      )}
    </div>
  );
}

function ItemsSection({
  items,
  isLoading,
  isError,
}: {
  items: { id: string; name: string; inventoryItemCode: string | null; displayPrice: number; displayCurrency: string }[] | undefined;
  isLoading: boolean;
  isError: boolean;
}) {
  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (isError) {
    return (
      <p className="py-12 text-center text-sm text-muted-foreground">
        Failed to load inventory items. Make sure the API is running.
      </p>
    );
  }

  if (!items || items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <Package className="mb-3 h-10 w-10 text-muted-foreground/50" />
        <p className="text-sm text-muted-foreground">No inventory items found.</p>
      </div>
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 print:grid-cols-3 print:gap-2">
      {items.map((item) => (
        <BarcodeCard
          key={item.id}
          name={item.name}
          code={item.inventoryItemCode}
          subtitle={`${item.displayCurrency} ${item.displayPrice.toFixed(2)}`}
        />
      ))}
    </div>
  );
}

function MenusSection({
  menus,
  isLoading,
  isError,
}: {
  menus: { id: string; description: string | null; menuItemCode: string | null; priceRange: string | null }[] | undefined;
  isLoading: boolean;
  isError: boolean;
}) {
  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (isError) {
    return (
      <p className="py-12 text-center text-sm text-muted-foreground">
        Failed to load menus. Make sure the API is running.
      </p>
    );
  }

  if (!menus || menus.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-center">
        <UtensilsCrossed className="mb-3 h-10 w-10 text-muted-foreground/50" />
        <p className="text-sm text-muted-foreground">No menus found.</p>
      </div>
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 print:grid-cols-3 print:gap-2">
      {menus.map((menu) => (
        <BarcodeCard
          key={menu.id}
          name={menu.description || "Untitled Menu"}
          code={menu.menuItemCode}
          subtitle={menu.priceRange ?? ""}
        />
      ))}
    </div>
  );
}

function BarcodeCard({
  name,
  code,
  subtitle,
}: {
  name: string;
  code: string | null;
  subtitle: string;
}) {
  return (
    <div className="flex flex-col items-center rounded-lg border bg-background p-4 text-center print:break-inside-avoid print:border-gray-300 print:p-2">
      <p className="mb-1 text-sm font-semibold truncate max-w-full">{name}</p>
      {subtitle && (
        <p className="mb-2 text-xs text-muted-foreground">{subtitle}</p>
      )}
      {code ? (
        <>
          <Barcode
            value={code}
            format="CODE128"
            width={1.5}
            height={50}
            fontSize={12}
            margin={4}
            background="transparent"
          />
          <p className="mt-1 text-xs font-mono text-muted-foreground">{code}</p>
        </>
      ) : (
        <div className="flex h-[70px] items-center justify-center">
          <Badge className="bg-amber-100 text-amber-800">No code</Badge>
        </div>
      )}
    </div>
  );
}
