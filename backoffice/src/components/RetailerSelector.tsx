import { useRetailers } from "@/hooks/useRetailer";
import { Select } from "@/components/ui/select";

interface RetailerSelectorProps {
  value: string;
  onChange: (ownerId: string) => void;
  className?: string;
}

export default function RetailerSelector({
  value,
  onChange,
  className,
}: RetailerSelectorProps) {
  const { data: retailers } = useRetailers();

  return (
    <Select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className={className}
    >
      <option value="">All retailers</option>
      {retailers?.map((r) => (
        <option key={`${r.retailerType}-${r.id}`} value={r.id}>
          {r.businessName} ({r.retailerType})
        </option>
      ))}
    </Select>
  );
}
