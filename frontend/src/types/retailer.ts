export type RetailerType = "restaurant" | "pharmacy" | "shop";

export interface PaymentMethod {
  providerType: string;
  providerName?: string;
  accountNumber?: string;
  accountName?: string;
}

export interface RetailerOpeningDayHour {
  day: string;
  openingTime: string;
  closingTime: string;
  isAvailable: boolean;
}

export interface Retailer {
  id: string;
  retailerType: RetailerType;
  status?: string;
  businessName?: string;
  businessPhoneNumber?: string;
  businessEmail?: string;
  displayImage?: string;
  isReadyToServe: boolean;
  isSetupOnPortal: boolean;
  paymentMethods?: PaymentMethod[];
  preferredPaymentMethods?: PaymentMethod;
  openingDayHours: RetailerOpeningDayHour[];
  createdAt: string;
  updatedAt?: string;
}

export interface Branch {
  id: string;
  retailerId: string;
  retailerType: RetailerType;
  status?: string;
  businessName?: string;
  businessPhoneNumber?: string;
  businessEmail?: string;
  address?: string;
  city?: string;
  locationName?: string;
  isReadyToServe: boolean;
  isSetupOnPortal: boolean;
  displayImage?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateRetailerRequest {
  retailerType: RetailerType;
  businessName?: string;
  businessEmail?: string;
  businessPhoneNumber?: string;
  paymentMethods?: PaymentMethod[];
  preferredPaymentMethods?: PaymentMethod;
  openingDayHours?: RetailerOpeningDayHour[];
  isReadyToServe?: boolean;
  isSetupOnPortal?: boolean;
}

export interface UpdateRetailerRequest {
  businessName?: string;
  businessEmail?: string;
  businessPhoneNumber?: string;
  status?: string;
}

export interface CreateBranchRequest {
  retailerId: string;
  businessName?: string;
  businessPhoneNumber?: string;
  businessEmail?: string;
  address?: string;
  city?: string;
}
