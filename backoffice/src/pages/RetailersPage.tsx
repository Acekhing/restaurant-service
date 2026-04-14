import { useState } from "react";
import { Link } from "react-router-dom";
import {
  Plus,
  Store,
  X,
  Check,
  Copy,
  UtensilsCrossed,
  Pill,
  ShoppingBag,
  Trash2,
  ChevronRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Badge } from "@/components/ui/badge";
import { useRetailers, useCreateRetailer } from "@/hooks/useRetailer";
import type {
  Retailer,
  RetailerType,
  PaymentMethod,
  RetailerOpeningDayHour,
  CreateRetailerRequest,
} from "@/types/retailer";

const typeConfig: Record<
  RetailerType,
  { label: string; icon: typeof Store; color: string }
> = {
  restaurant: {
    label: "Restaurant",
    icon: UtensilsCrossed,
    color: "bg-orange-100 text-orange-800",
  },
  pharmacy: {
    label: "Pharmacy",
    icon: Pill,
    color: "bg-blue-100 text-blue-800",
  },
  shop: {
    label: "Shop",
    icon: ShoppingBag,
    color: "bg-green-100 text-green-800",
  },
};

const DAYS = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

const defaultHours: RetailerOpeningDayHour[] = DAYS.map((day) => ({
  day,
  openingTime: "08:00",
  closingTime: "18:00",
  isAvailable: day !== "Sunday",
}));

function CreateRetailerForm({ onCreated }: { onCreated: () => void }) {
  const mutation = useCreateRetailer();
  const [type, setType] = useState<RetailerType>("restaurant");
  const [businessName, setBusinessName] = useState("");
  const [businessEmail, setBusinessEmail] = useState("");
  const [businessPhoneNumber, setBusinessPhoneNumber] = useState("");
  const [isReadyToServe, setIsReadyToServe] = useState(false);
  const [isSetupOnPortal, setIsSetupOnPortal] = useState(false);
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([]);
  const [preferredIdx, setPreferredIdx] = useState<number | null>(null);
  const [hours, setHours] = useState<RetailerOpeningDayHour[]>(defaultHours);

  const addPaymentMethod = () => {
    setPaymentMethods((prev) => [...prev, { providerType: "" }]);
  };

  const removePaymentMethod = (idx: number) => {
    setPaymentMethods((prev) => prev.filter((_, i) => i !== idx));
    if (preferredIdx === idx) setPreferredIdx(null);
    else if (preferredIdx !== null && preferredIdx > idx)
      setPreferredIdx(preferredIdx - 1);
  };

  const updatePaymentMethod = (
    idx: number,
    field: keyof PaymentMethod,
    value: string
  ) => {
    setPaymentMethods((prev) =>
      prev.map((pm, i) => (i === idx ? { ...pm, [field]: value } : pm))
    );
  };

  const updateHour = (
    idx: number,
    field: keyof RetailerOpeningDayHour,
    value: string | boolean
  ) => {
    setHours((prev) =>
      prev.map((h, i) => (i === idx ? { ...h, [field]: value } : h))
    );
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const body: CreateRetailerRequest = {
      retailerType: type,
      businessName: businessName.trim() || undefined,
      businessEmail: businessEmail.trim() || undefined,
      businessPhoneNumber: businessPhoneNumber.trim() || undefined,
      paymentMethods: paymentMethods.length > 0 ? paymentMethods : undefined,
      preferredPaymentMethods:
        preferredIdx !== null ? paymentMethods[preferredIdx] : undefined,
      openingDayHours: hours,
      isReadyToServe,
      isSetupOnPortal,
    };
    mutation.mutate(body, {
      onSuccess: () => {
        setBusinessName("");
        setBusinessEmail("");
        setBusinessPhoneNumber("");
        setPaymentMethods([]);
        setPreferredIdx(null);
        setHours(defaultHours);
        setIsReadyToServe(false);
        setIsSetupOnPortal(false);
        onCreated();
      },
    });
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-6 rounded-lg border bg-background p-6"
    >
      <h2 className="text-lg font-semibold">Add Retailer</h2>

      {/* Business Info */}
      <fieldset className="space-y-3">
        <legend className="text-sm font-medium text-muted-foreground">
          Business Info
        </legend>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <label
              htmlFor="r-type"
              className="mb-1.5 block text-sm font-medium"
            >
              Type
            </label>
            <Select
              id="r-type"
              value={type}
              onChange={(e) => setType(e.target.value as RetailerType)}
            >
              <option value="restaurant">Restaurant</option>
              <option value="pharmacy">Pharmacy</option>
              <option value="shop">Shop</option>
            </Select>
          </div>
          <div>
            <label
              htmlFor="r-name"
              className="mb-1.5 block text-sm font-medium"
            >
              Business Name
            </label>
            <Input
              id="r-name"
              value={businessName}
              onChange={(e) => setBusinessName(e.target.value)}
              placeholder="e.g. Golden Palace"
              required
            />
          </div>
          <div>
            <label
              htmlFor="r-email"
              className="mb-1.5 block text-sm font-medium"
            >
              Business Email
            </label>
            <Input
              id="r-email"
              type="email"
              value={businessEmail}
              onChange={(e) => setBusinessEmail(e.target.value)}
              placeholder="info@example.com"
            />
          </div>
          <div>
            <label
              htmlFor="r-phone"
              className="mb-1.5 block text-sm font-medium"
            >
              Business Phone
            </label>
            <Input
              id="r-phone"
              type="tel"
              value={businessPhoneNumber}
              onChange={(e) => setBusinessPhoneNumber(e.target.value)}
              placeholder="+233..."
            />
          </div>
        </div>
      </fieldset>

      {/* Payment Methods */}
      <fieldset className="space-y-3">
        <legend className="text-sm font-medium text-muted-foreground">
          Payment Methods
        </legend>
        {paymentMethods.map((pm, idx) => (
          <div key={idx} className="flex items-center gap-3">
            <Input
              value={pm.providerType}
              onChange={(e) =>
                updatePaymentMethod(idx, "providerType", e.target.value)
              }
              placeholder="Provider (e.g. Mobile Money)"
              className="flex-1"
              required
            />
            <Input
              value={pm.accountNumber ?? ""}
              onChange={(e) =>
                updatePaymentMethod(idx, "accountNumber", e.target.value)
              }
              placeholder="Account #"
              className="w-40"
            />
            <label className="flex items-center gap-1.5 text-sm whitespace-nowrap cursor-pointer">
              <input
                type="radio"
                name="preferred-payment"
                checked={preferredIdx === idx}
                onChange={() => setPreferredIdx(idx)}
                className="accent-primary"
              />
              Preferred
            </label>
            <Button
              type="button"
              size="icon"
              variant="ghost"
              onClick={() => removePaymentMethod(idx)}
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        ))}
        <Button type="button" variant="outline" size="sm" onClick={addPaymentMethod}>
          <Plus className="mr-1 h-3.5 w-3.5" />
          Add Payment Method
        </Button>
      </fieldset>

      {/* Operating Hours */}
      <fieldset className="space-y-3">
        <legend className="text-sm font-medium text-muted-foreground">
          Operating Hours
        </legend>
        <div className="space-y-2">
          {hours.map((h, idx) => (
            <div key={h.day} className="flex items-center gap-3">
              <label className="flex items-center gap-2 w-28 cursor-pointer">
                <input
                  type="checkbox"
                  checked={h.isAvailable}
                  onChange={(e) =>
                    updateHour(idx, "isAvailable", e.target.checked)
                  }
                  className="accent-primary"
                />
                <span className="text-sm">{h.day.slice(0, 3)}</span>
              </label>
              <Input
                type="time"
                value={h.openingTime}
                onChange={(e) =>
                  updateHour(idx, "openingTime", e.target.value)
                }
                disabled={!h.isAvailable}
                className="w-32"
              />
              <span className="text-sm text-muted-foreground">to</span>
              <Input
                type="time"
                value={h.closingTime}
                onChange={(e) =>
                  updateHour(idx, "closingTime", e.target.value)
                }
                disabled={!h.isAvailable}
                className="w-32"
              />
            </div>
          ))}
        </div>
      </fieldset>

      {/* Toggles */}
      <fieldset className="space-y-3">
        <legend className="text-sm font-medium text-muted-foreground">
          Status Toggles
        </legend>
        <div className="flex flex-wrap gap-6">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={isReadyToServe}
              onChange={(e) => setIsReadyToServe(e.target.checked)}
              className="accent-primary h-4 w-4"
            />
            <span className="text-sm">Ready to Serve</span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={isSetupOnPortal}
              onChange={(e) => setIsSetupOnPortal(e.target.checked)}
              className="accent-primary h-4 w-4"
            />
            <span className="text-sm">Setup on Portal</span>
          </label>
        </div>
      </fieldset>

      {mutation.isError && (
        <p className="text-sm text-destructive">
          Failed to create retailer. Please try again.
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending && <Spinner className="h-4 w-4" />}
          Create Retailer
        </Button>
      </div>
    </form>
  );
}

function RetailerRow({ retailer }: { retailer: Retailer }) {
  const cfg = typeConfig[retailer.retailerType];
  const Icon = cfg.icon;
  const [copied, setCopied] = useState(false);

  const handleCopyId = async () => {
    await navigator.clipboard.writeText(retailer.id);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  return (
    <li className="flex items-center gap-3 border-b px-4 py-3 last:border-b-0 hover:bg-muted/50 transition-colors">
      {retailer.displayImage ? (
        <img
          src={retailer.displayImage}
          alt=""
          className="h-8 w-8 shrink-0 rounded-full object-cover"
        />
      ) : (
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
          <Icon className="h-4 w-4 text-muted-foreground" />
        </span>
      )}
      <div className="min-w-0 flex-1">
        <Link
          to={`/retailers/${retailer.id}`}
          className="font-medium hover:underline"
        >
          {retailer.businessName || "Unnamed"}
        </Link>
        <div className="flex items-center gap-1.5">
          <code className="text-xs text-muted-foreground font-mono">
            {retailer.id}
          </code>
          <button
            type="button"
            onClick={handleCopyId}
            className="shrink-0 rounded p-0.5 text-muted-foreground hover:text-foreground transition-colors"
            title="Copy ID"
          >
            {copied ? (
              <Check className="h-3 w-3 text-green-600" />
            ) : (
              <Copy className="h-3 w-3" />
            )}
          </button>
        </div>
      </div>
      {retailer.status && (
        <Badge
          className={
            retailer.status === "Approved"
              ? "bg-emerald-100 text-emerald-800"
              : retailer.status === "Suspended"
                ? "bg-amber-100 text-amber-800"
                : "bg-red-100 text-red-800"
          }
        >
          {retailer.status}
        </Badge>
      )}
      <Badge className={cfg.color}>{cfg.label}</Badge>
      <Link
        to={`/retailers/${retailer.id}`}
        className="shrink-0 rounded p-1 text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
        title="View details"
      >
        <ChevronRight className="h-4 w-4" />
      </Link>
    </li>
  );
}

export default function RetailersPage() {
  const [showCreate, setShowCreate] = useState(false);
  const [filterType, setFilterType] = useState<RetailerType | "">("");
  const {
    data: retailers,
    isLoading,
    isError,
  } = useRetailers(filterType || undefined);

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Retailers</h1>
          <p className="text-sm text-muted-foreground">
            Manage restaurants, pharmacies, and shops
          </p>
        </div>
        <Button onClick={() => setShowCreate((s) => !s)}>
          {showCreate ? (
            <>
              <X className="h-4 w-4" />
              Cancel
            </>
          ) : (
            <>
              <Plus className="h-4 w-4" />
              Add Retailer
            </>
          )}
        </Button>
      </div>

      {showCreate && (
        <CreateRetailerForm onCreated={() => setShowCreate(false)} />
      )}

      <div className="flex items-center gap-3">
        <span className="text-sm text-muted-foreground">Filter:</span>
        <Select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value as RetailerType | "")}
          className="w-40"
        >
          <option value="">All types</option>
          <option value="restaurant">Restaurant</option>
          <option value="pharmacy">Pharmacy</option>
          <option value="shop">Shop</option>
        </Select>
        {retailers && (
          <span className="text-sm text-muted-foreground">
            {retailers.length} retailer{retailers.length !== 1 ? "s" : ""}
          </span>
        )}
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load retailers. Make sure the API is running.
        </p>
      )}

      {retailers && (
        <>
          {retailers.length === 0 && !showCreate && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <Store className="mb-3 h-10 w-10 text-muted-foreground/50" />
              <p className="text-sm text-muted-foreground">
                No retailers yet. Add one to get started.
              </p>
            </div>
          )}

          {retailers.length > 0 && (
            <ul className="rounded-lg border bg-background">
              {retailers.map((r) => (
                <RetailerRow key={r.id} retailer={r} />
              ))}
            </ul>
          )}
        </>
      )}
    </div>
  );
}
