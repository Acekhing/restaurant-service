import { Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";

interface AuditFiltersProps {
  aggregateId: string;
  onAggregateIdChange: (v: string) => void;
  eventType: string;
  onEventTypeChange: (v: string) => void;
  aggregateType: string;
  onAggregateTypeChange: (v: string) => void;
}

const eventTypes = [
  "",
  "ItemCreated",
  "ItemUpdated",
  "ItemDeleted",
  "PromotionCreated",
  "PromotionUpdated",
  "PromotionDeleted",
];

const aggregateTypes = [
  "",
  "InventoryItem",
  "InventoryItemPromotion",
];

export default function AuditFilters({
  aggregateId,
  onAggregateIdChange,
  eventType,
  onEventTypeChange,
  aggregateType,
  onAggregateTypeChange,
}: AuditFiltersProps) {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Filter by Aggregate ID..."
          value={aggregateId}
          onChange={(e) => onAggregateIdChange(e.target.value)}
          className="pl-9"
        />
      </div>
      <Select
        value={aggregateType}
        onChange={(e) => onAggregateTypeChange(e.target.value)}
        className="w-auto sm:w-48"
      >
        <option value="">All Types</option>
        {aggregateTypes
          .filter(Boolean)
          .map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
      </Select>
      <Select
        value={eventType}
        onChange={(e) => onEventTypeChange(e.target.value)}
        className="w-auto sm:w-48"
      >
        <option value="">All Events</option>
        {eventTypes
          .filter(Boolean)
          .map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
      </Select>
    </div>
  );
}
