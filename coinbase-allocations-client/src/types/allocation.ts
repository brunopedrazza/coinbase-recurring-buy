export interface Allocation {
  symbol: string;
  usdcAmount: number;
  isActive: boolean;
}

export interface AllocationSettings {
  minimumUsdcBalance: number;
  allocations: Allocation[];
  currentUsdcBalance?: number | null;
}
