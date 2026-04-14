import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(amount: number, currency = "GHS") {
  return `${currency} ${amount.toFixed(2)}`;
}

export function formatDate(date: string | Date) {
  return new Date(date).toLocaleDateString("en-GB", {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function itemTypeBadgeColor(type: string) {
  switch (type) {
    case "restaurant":
      return "bg-orange-100 text-orange-800";
    case "pharmacy":
      return "bg-blue-100 text-blue-800";
    case "shop":
      return "bg-purple-100 text-purple-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
}
