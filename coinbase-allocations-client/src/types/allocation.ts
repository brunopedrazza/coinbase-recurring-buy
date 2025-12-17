export interface Allocation {
  symbol: string;
  usdcAmount: number;
  isActive: boolean;
}

export interface RecurringBuySettings {
  allocations: Allocation[];
  minimumUsdcBalance: number;
}

export type AllocationResponse = RecurringBuySettings;

export type AllocationUpdateRequest = RecurringBuySettings;
