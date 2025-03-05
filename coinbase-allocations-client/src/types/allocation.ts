export interface Allocation {
  symbol: string;
  usdcAmount: number;
  isActive: boolean;
}

export interface AllocationResponse extends Array<Allocation> {}

export interface AllocationUpdateRequest extends Array<Allocation> {} 