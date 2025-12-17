import axios from 'axios';
import { AllocationResponse, AllocationUpdateRequest, Allocation } from '../types/allocation';
import { apiConfig } from '../auth/authConfig';

const getAllocations = async (accessToken: string): Promise<AllocationResponse> => {
  const response = await axios.get(apiConfig.allocationsFetchEndpoint, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'x-functions-key': apiConfig.functionKey,
    },
  });
  const data = response.data;

  if (Array.isArray(data)) {
    return {
      allocations: data as Allocation[],
      minimumUsdcBalance: 0,
    };
  }

  return {
    allocations: data.allocations ?? [],
    minimumUsdcBalance: data.minimumUsdcBalance ?? 0,
  };
};

const updateAllocations = async (accessToken: string, allocations: AllocationUpdateRequest): Promise<void> => {
  await axios.post(apiConfig.allocationsUpdateEndpoint, allocations, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
      'x-functions-key': apiConfig.functionKey,
    },
  });
};

export const allocationService = {
  getAllocations,
  updateAllocations,
}; 
