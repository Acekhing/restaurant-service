import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import {
  ArrowLeft,
  Plus,
  Store,
  MapPin,
  Phone,
  Mail,
  Clock,
  GitBranch,
  CheckCircle2,
  XCircle,
  Copy,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { Badge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { Sheet } from "@/components/ui/sheet";
import { useRetailer } from "@/hooks/useRetailer";
import { useBranches, useCreateBranch } from "@/hooks/useBranch";
import type { Branch, CreateBranchRequest } from "@/types/retailer";

function CreateBranchDialog({
  retailerId,
  open,
  onClose,
}: {
  retailerId: string;
  open: boolean;
  onClose: () => void;
}) {
  const mutation = useCreateBranch();
  const [businessName, setBusinessName] = useState("");
  const [businessEmail, setBusinessEmail] = useState("");
  const [businessPhoneNumber, setBusinessPhoneNumber] = useState("");
  const [address, setAddress] = useState("");
  const [city, setCity] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const body: CreateBranchRequest = {
      retailerId,
      businessName: businessName.trim() || undefined,
      businessPhoneNumber: businessPhoneNumber.trim() || undefined,
      businessEmail: businessEmail.trim() || undefined,
      address: address.trim() || undefined,
      city: city.trim() || undefined,
    };
    mutation.mutate(body, {
      onSuccess: () => {
        setBusinessName("");
        setBusinessEmail("");
        setBusinessPhoneNumber("");
        setAddress("");
        setCity("");
        onClose();
      },
    });
  };

  return (
    <Dialog open={open} onClose={onClose} title="Create New Branch">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1.5 block text-sm font-medium">
            Branch Name
          </label>
          <Input
            value={businessName}
            onChange={(e) => setBusinessName(e.target.value)}
            placeholder="e.g. Downtown Branch"
            required
          />
        </div>
        <div className="grid gap-4 grid-cols-2">
          <div>
            <label className="mb-1.5 block text-sm font-medium">Email</label>
            <Input
              type="email"
              value={businessEmail}
              onChange={(e) => setBusinessEmail(e.target.value)}
              placeholder="branch@example.com"
            />
          </div>
          <div>
            <label className="mb-1.5 block text-sm font-medium">Phone</label>
            <Input
              type="tel"
              value={businessPhoneNumber}
              onChange={(e) => setBusinessPhoneNumber(e.target.value)}
              placeholder="+233..."
            />
          </div>
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium">Address</label>
          <Input
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            placeholder="123 Main Street"
          />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium">City</label>
          <Input
            value={city}
            onChange={(e) => setCity(e.target.value)}
            placeholder="Accra"
          />
        </div>

        {mutation.isError && (
          <p className="text-sm text-destructive">
            Failed to create branch. Please try again.
          </p>
        )}

        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending && <Spinner className="h-4 w-4" />}
            Create Branch
          </Button>
        </div>
      </form>
    </Dialog>
  );
}

function BranchCard({
  branch,
  selected,
  onClick,
}: {
  branch: Branch;
  selected: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`w-full text-left rounded-lg border bg-background p-4 space-y-2 transition-colors hover:border-foreground/30 cursor-pointer ${
        selected ? "ring-2 ring-primary border-primary" : ""
      }`}
    >
      <div className="flex items-center justify-between">
        <h3 className="font-medium">
          {branch.businessName || "Unnamed Branch"}
        </h3>
        {branch.status && (
          <Badge
            className={
              branch.status === "Active" || branch.status === "Approved"
                ? "bg-emerald-100 text-emerald-800"
                : "bg-amber-100 text-amber-800"
            }
          >
            {branch.status}
          </Badge>
        )}
      </div>
      <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
        {branch.address && (
          <span className="flex items-center gap-1">
            <MapPin className="h-3.5 w-3.5" />
            {branch.address}
            {branch.city ? `, ${branch.city}` : ""}
          </span>
        )}
        {branch.businessPhoneNumber && (
          <span className="flex items-center gap-1">
            <Phone className="h-3.5 w-3.5" />
            {branch.businessPhoneNumber}
          </span>
        )}
        {branch.businessEmail && (
          <span className="flex items-center gap-1">
            <Mail className="h-3.5 w-3.5" />
            {branch.businessEmail}
          </span>
        )}
      </div>
      <div className="flex gap-2 text-xs text-muted-foreground">
        <code className="font-mono">{branch.id}</code>
      </div>
    </button>
  );
}

function BranchDetailRow({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null;
  return (
    <div className="flex flex-col gap-0.5">
      <dt className="text-xs font-medium text-muted-foreground">{label}</dt>
      <dd className="text-sm">{value}</dd>
    </div>
  );
}

function BranchDetailPanel({
  branch,
  onClose,
}: {
  branch: Branch;
  onClose: () => void;
}) {
  const [copied, setCopied] = useState(false);

  const copyId = () => {
    navigator.clipboard.writeText(branch.id);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  return (
    <Sheet
      open
      onClose={onClose}
      title={branch.businessName || "Branch Details"}
    >
      <div className="space-y-6">
        {/* Status & Type */}
        <div className="flex flex-wrap items-center gap-2">
          {branch.status && (
            <Badge
              className={
                branch.status === "Active" || branch.status === "Approved"
                  ? "bg-emerald-100 text-emerald-800"
                  : "bg-amber-100 text-amber-800"
              }
            >
              {branch.status}
            </Badge>
          )}
          <Badge className="bg-slate-100 text-slate-800 capitalize">
            {branch.retailerType}
          </Badge>
        </div>

        {/* ID */}
        <div className="flex items-center gap-2">
          <code className="flex-1 truncate rounded bg-muted px-2 py-1 text-xs font-mono">
            {branch.id}
          </code>
          <button
            onClick={copyId}
            className="rounded p-1 text-muted-foreground hover:text-foreground transition-colors"
            title="Copy ID"
          >
            <Copy className="h-3.5 w-3.5" />
          </button>
          {copied && (
            <span className="text-xs text-emerald-600">Copied!</span>
          )}
        </div>

        {/* Contact */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold">Contact</h3>
          <dl className="grid gap-3">
            <BranchDetailRow label="Business Name" value={branch.businessName} />
            <BranchDetailRow label="Email" value={branch.businessEmail} />
            <BranchDetailRow label="Phone" value={branch.businessPhoneNumber} />
          </dl>
        </div>

        {/* Location */}
        {(branch.address || branch.city || branch.locationName) && (
          <div className="space-y-3">
            <h3 className="text-sm font-semibold flex items-center gap-1.5">
              <MapPin className="h-4 w-4" />
              Location
            </h3>
            <dl className="grid gap-3">
              <BranchDetailRow label="Address" value={branch.address} />
              <BranchDetailRow label="City" value={branch.city} />
              <BranchDetailRow label="Location Name" value={branch.locationName} />
            </dl>
          </div>
        )}

        {/* Flags */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold">Settings</h3>
          <div className="grid gap-2">
            <div className="flex items-center gap-2 text-sm">
              {branch.isReadyToServe ? (
                <CheckCircle2 className="h-4 w-4 text-emerald-600" />
              ) : (
                <XCircle className="h-4 w-4 text-muted-foreground" />
              )}
              <span className={branch.isReadyToServe ? "" : "text-muted-foreground"}>
                Ready to Serve
              </span>
            </div>
            <div className="flex items-center gap-2 text-sm">
              {branch.isSetupOnPortal ? (
                <CheckCircle2 className="h-4 w-4 text-emerald-600" />
              ) : (
                <XCircle className="h-4 w-4 text-muted-foreground" />
              )}
              <span className={branch.isSetupOnPortal ? "" : "text-muted-foreground"}>
                Setup on Portal
              </span>
            </div>
          </div>
        </div>

        {/* Timestamps */}
        <div className="space-y-3 border-t pt-4">
          <h3 className="text-sm font-semibold">Timeline</h3>
          <dl className="grid gap-3">
            <BranchDetailRow
              label="Created"
              value={new Date(branch.createdAt).toLocaleString()}
            />
            {branch.updatedAt && (
              <BranchDetailRow
                label="Last Updated"
                value={new Date(branch.updatedAt).toLocaleString()}
              />
            )}
          </dl>
        </div>
      </div>
    </Sheet>
  );
}

export default function RetailerDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: retailer, isLoading, isError } = useRetailer(id!);
  const {
    data: branches,
    isLoading: branchesLoading,
  } = useBranches(id!);
  const [showCreateBranch, setShowCreateBranch] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);

  if (isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (isError || !retailer) {
    return (
      <div className="mx-auto max-w-4xl space-y-4">
        <Link
          to="/retailers"
          className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Retailers
        </Link>
        <p className="py-12 text-center text-sm text-muted-foreground">
          Retailer not found or failed to load.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <Link
        to="/retailers"
        className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Retailers
      </Link>

      {/* Retailer Summary */}
      <div className="rounded-lg border bg-background p-6 space-y-4">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            {retailer.displayImage ? (
              <img
                src={retailer.displayImage}
                alt=""
                className="h-12 w-12 rounded-full object-cover"
              />
            ) : (
              <span className="flex h-12 w-12 items-center justify-center rounded-full bg-muted">
                <Store className="h-6 w-6 text-muted-foreground" />
              </span>
            )}
            <div>
              <h1 className="text-xl font-bold">
                {retailer.businessName || "Unnamed"}
              </h1>
              <div className="flex items-center gap-2 mt-0.5">
                <Badge className="bg-slate-100 text-slate-800 capitalize">
                  {retailer.retailerType}
                </Badge>
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
              </div>
            </div>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 text-sm">
          {retailer.businessEmail && (
            <div className="flex items-center gap-2 text-muted-foreground">
              <Mail className="h-4 w-4 shrink-0" />
              {retailer.businessEmail}
            </div>
          )}
          {retailer.businessPhoneNumber && (
            <div className="flex items-center gap-2 text-muted-foreground">
              <Phone className="h-4 w-4 shrink-0" />
              {retailer.businessPhoneNumber}
            </div>
          )}
          <div className="flex items-center gap-2 text-muted-foreground">
            <span className={retailer.isReadyToServe ? "text-emerald-600" : ""}>
              {retailer.isReadyToServe ? "Ready to Serve" : "Not Ready"}
            </span>
            <span>·</span>
            <span className={retailer.isSetupOnPortal ? "text-emerald-600" : ""}>
              {retailer.isSetupOnPortal ? "On Portal" : "Not on Portal"}
            </span>
          </div>
        </div>

        {/* Payment Methods */}
        {retailer.paymentMethods && retailer.paymentMethods.length > 0 && (
          <div>
            <h3 className="text-sm font-medium mb-2">Payment Methods</h3>
            <div className="flex flex-wrap gap-2">
              {retailer.paymentMethods.map((pm, idx) => (
                <Badge key={idx} className="bg-slate-100 text-slate-700">
                  {pm.providerType}
                  {retailer.preferredPaymentMethods?.providerType ===
                    pm.providerType && (
                    <span className="ml-1 text-emerald-600 text-xs">
                      (preferred)
                    </span>
                  )}
                </Badge>
              ))}
            </div>
          </div>
        )}

        {/* Operating Hours */}
        {retailer.openingDayHours && retailer.openingDayHours.length > 0 && (
          <div>
            <h3 className="text-sm font-medium mb-2 flex items-center gap-1.5">
              <Clock className="h-4 w-4" />
              Operating Hours
            </h3>
            <div className="grid gap-1 sm:grid-cols-2 lg:grid-cols-4 text-sm">
              {retailer.openingDayHours.map((h) => (
                <div
                  key={h.day}
                  className={
                    h.isAvailable
                      ? "text-foreground"
                      : "text-muted-foreground line-through"
                  }
                >
                  <span className="font-medium">{h.day.slice(0, 3)}</span>{" "}
                  {h.isAvailable
                    ? `${h.openingTime} – ${h.closingTime}`
                    : "Closed"}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Branches */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <GitBranch className="h-5 w-5" />
            Branches
            {branches && (
              <span className="text-sm font-normal text-muted-foreground">
                ({branches.length})
              </span>
            )}
          </h2>
          <Button onClick={() => setShowCreateBranch(true)}>
            <Plus className="h-4 w-4" />
            Add Branch
          </Button>
        </div>

        {branchesLoading && (
          <div className="flex justify-center py-8">
            <Spinner />
          </div>
        )}

        {branches && branches.length === 0 && (
          <div className="flex flex-col items-center justify-center py-12 text-center rounded-lg border bg-background">
            <GitBranch className="mb-3 h-8 w-8 text-muted-foreground/50" />
            <p className="text-sm text-muted-foreground">
              No branches yet. Add one to get started.
            </p>
          </div>
        )}

        {branches && branches.length > 0 && (
          <div className="grid gap-3">
            {branches.map((b) => (
              <BranchCard
                key={b.id}
                branch={b}
                selected={selectedBranch?.id === b.id}
                onClick={() => setSelectedBranch(b)}
              />
            ))}
          </div>
        )}
      </div>

      <CreateBranchDialog
        retailerId={id!}
        open={showCreateBranch}
        onClose={() => setShowCreateBranch(false)}
      />

      {selectedBranch && (
        <BranchDetailPanel
          branch={selectedBranch}
          onClose={() => setSelectedBranch(null)}
        />
      )}
    </div>
  );
}
