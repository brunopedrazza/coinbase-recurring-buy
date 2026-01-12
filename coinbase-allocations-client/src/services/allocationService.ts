import axios from 'axios';
import { AllocationSettings } from '../types/allocation';
import { apiConfig } from '../auth/authConfig';

const getAllocations = async (accessToken: string): Promise<AllocationSettings> => {
  const response = await axios.get(apiConfig.allocationsFetchEndpoint, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'x-functions-key': apiConfig.functionKey,
    },
  });
  return response.data;
};

const updateAllocations = async (accessToken: string, settings: AllocationSettings): Promise<void> => {
  await axios.post(apiConfig.allocationsUpdateEndpoint, settings, {
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
