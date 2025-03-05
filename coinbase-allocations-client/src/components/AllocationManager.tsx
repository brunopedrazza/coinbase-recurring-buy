import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Switch,
  TextField,
  Typography,
  Alert,
  Chip,
  useTheme,
  Tooltip,
  Paper,
  CircularProgress,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, Refresh as RefreshIcon } from '@mui/icons-material';
import { useMsal } from '@azure/msal-react';
import { Allocation } from '../types/allocation';
import { allocationService } from '../services/allocationService';
import { loginRequest } from '../auth/authConfig';

export const AllocationManager: React.FC = () => {
  const { instance, accounts } = useMsal();
  const [allocations, setAllocations] = useState<Allocation[]>([]);
  const [editingAllocation, setEditingAllocation] = useState<Allocation | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const theme = useTheme();

  const fetchAllocations = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const token = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const response = await allocationService.getAllocations(token.accessToken);
      setAllocations(response);
    } catch (err) {
      setError('Failed to fetch allocations. Please try again.');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchAllocations();
  }, []);

  const handleRefresh = () => {
    fetchAllocations();
  };

  const handleSave = async (allocation: Allocation) => {
    try {
      const token = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });

      const updatedAllocations = editingAllocation
        ? allocations.map((a) => (a.symbol === allocation.symbol ? allocation : a))
        : [...allocations, allocation];

      await allocationService.updateAllocations(token.accessToken, updatedAllocations);

      setAllocations(updatedAllocations);
      setIsDialogOpen(false);
      setEditingAllocation(null);
    } catch (err) {
      setError('Failed to save allocation. Please try again.');
      console.error(err);
    }
  };

  const handleDelete = async (symbol: string) => {
    try {
      const token = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });

      const updatedAllocations = allocations.filter((a) => a.symbol !== symbol);
      await allocationService.updateAllocations(token.accessToken, updatedAllocations);

      setAllocations(updatedAllocations);
    } catch (err) {
      setError('Failed to delete allocation. Please try again.');
      console.error(err);
    }
  };

  const handleToggle = async (symbol: string, isActive: boolean) => {
    try {
      const token = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });

      const updatedAllocations = allocations.map((a) =>
        a.symbol === symbol ? { ...a, isActive } : a
      );

      await allocationService.updateAllocations(token.accessToken, updatedAllocations);

      setAllocations(updatedAllocations);
    } catch (err) {
      setError('Failed to update allocation status. Please try again.');
      console.error(err);
    }
  };

  const totalActiveAmount = allocations
    .filter(a => a.isActive)
    .reduce((sum, a) => sum + a.usdcAmount, 0);

  return (
    <Box sx={{ width: '100%', px: { xs: 2, sm: 3, md: 4 }, py: 4 }}>
      <Box 
        sx={{ 
          maxWidth: '800px', 
          mx: 'auto',
          width: '100%',
        }}
      >
        <Paper 
          elevation={0} 
          sx={{ 
            p: 3, 
            borderRadius: 3,
            backgroundColor: theme.palette.mode === 'dark' ? 'rgba(30, 33, 38, 0.8)' : 'rgba(255, 255, 255, 0.8)',
            backdropFilter: 'blur(10px)',
            border: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.05)' : 'rgba(0, 0, 0, 0.05)'}`,
            width: '100%',
          }}
        >
          <Stack spacing={3}>
            <Box 
              display="flex" 
              flexDirection={{ xs: 'column', sm: 'row' }} 
              justifyContent="space-between" 
              alignItems={{ xs: 'flex-start', sm: 'center' }}
              gap={2}
            >
              <Box>
                <Typography variant="h4" gutterBottom>
                  Coinbase Allocations
                </Typography>
                <Typography variant="subtitle1" color="text.secondary">
                  Total Active Amount: ${totalActiveAmount.toFixed(2)} USDC
                </Typography>
              </Box>
              <Box display="flex" gap={2}>
                <Tooltip title="Refresh allocations">
                  <Button
                    variant="outlined"
                    startIcon={isLoading ? <CircularProgress size={20} /> : <RefreshIcon />}
                    onClick={handleRefresh}
                    disabled={isLoading}
                    sx={{ height: 'fit-content' }}
                  >
                    Refresh
                  </Button>
                </Tooltip>
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => {
                    setEditingAllocation(null);
                    setIsDialogOpen(true);
                  }}
                  sx={{ 
                    height: 'fit-content',
                    alignSelf: { xs: 'stretch', sm: 'auto' }
                  }}
                >
                  Add Allocation
                </Button>
              </Box>
            </Box>

            {error && (
              <Alert severity="error" onClose={() => setError(null)}>
                {error}
              </Alert>
            )}

            {isLoading && allocations.length === 0 ? (
              <Box 
                sx={{ 
                  textAlign: 'center', 
                  py: 6,
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 2
                }}
              >
                <CircularProgress />
                <Typography variant="body1">Loading allocations...</Typography>
              </Box>
            ) : allocations.length === 0 ? (
              <Box 
                sx={{ 
                  textAlign: 'center', 
                  py: 6,
                  color: 'text.secondary',
                  border: `1px dashed ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'}`,
                  borderRadius: 2,
                }}
              >
                <Typography variant="h6" gutterBottom>
                  No allocations yet
                </Typography>
                <Typography variant="body2">
                  Click "Add Allocation" to create your first allocation
                </Typography>
              </Box>
            ) : (
              allocations.map((allocation) => (
                <Card 
                  key={allocation.symbol}
                  sx={{
                    opacity: allocation.isActive ? 1 : 0.7,
                    transition: 'all 0.2s ease-in-out',
                  }}
                >
                  <CardContent sx={{ p: 3 }}>
                    <Box 
                      display="flex" 
                      flexDirection={{ xs: 'column', sm: 'row' }}
                      justifyContent="space-between" 
                      alignItems={{ xs: 'flex-start', sm: 'center' }}
                      gap={2}
                    >
                      <Box>
                        <Box display="flex" alignItems="center" gap={1} mb={1}>
                          <Typography variant="h6">{allocation.symbol}</Typography>
                          <Chip
                            label={allocation.isActive ? 'Active' : 'Inactive'}
                            size="small"
                            color={allocation.isActive ? 'success' : 'default'}
                            sx={{ height: 24 }}
                          />
                        </Box>
                        <Typography 
                          variant="h5" 
                          color="text.secondary" 
                          sx={{ 
                            fontWeight: 600,
                            color: theme.palette.mode === 'light' ? '#2D3436' : '#E4E7EB',
                          }}
                        >
                          ${allocation.usdcAmount.toFixed(2)}
                        </Typography>
                      </Box>
                      <Box 
                        display="flex" 
                        alignItems="center" 
                        gap={1}
                        sx={{ 
                          alignSelf: { xs: 'flex-end', sm: 'center' },
                          mt: { xs: 1, sm: 0 }
                        }}
                      >
                        <Tooltip title={allocation.isActive ? 'Disable' : 'Enable'}>
                          <Switch
                            checked={allocation.isActive}
                            onChange={(e) => handleToggle(allocation.symbol, e.target.checked)}
                            color="success"
                          />
                        </Tooltip>
                        <Tooltip title="Edit">
                          <IconButton
                            onClick={() => {
                              setEditingAllocation(allocation);
                              setIsDialogOpen(true);
                            }}
                            size="small"
                          >
                            <EditIcon />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton
                            onClick={() => handleDelete(allocation.symbol)}
                            color="error"
                            size="small"
                          >
                            <DeleteIcon />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </Box>
                  </CardContent>
                </Card>
              ))
            )}
          </Stack>
        </Paper>

        <AllocationDialog
          open={isDialogOpen}
          allocation={editingAllocation}
          onClose={() => {
            setIsDialogOpen(false);
            setEditingAllocation(null);
          }}
          onSave={handleSave}
        />
      </Box>
    </Box>
  );
};

interface AllocationDialogProps {
  open: boolean;
  allocation: Allocation | null;
  onClose: () => void;
  onSave: (allocation: Allocation) => void;
}

const AllocationDialog: React.FC<AllocationDialogProps> = ({
  open,
  allocation,
  onClose,
  onSave,
}) => {
  const [formData, setFormData] = useState<Partial<Allocation>>({
    symbol: '',
    usdcAmount: 0,
    isActive: true,
  });
  const [inputValue, setInputValue] = useState<string>('');

  useEffect(() => {
    if (allocation) {
      setFormData(allocation);
      setInputValue(formatUsdcInputValue(allocation.usdcAmount));
    } else {
      setFormData({
        symbol: '',
        usdcAmount: 0,
        isActive: true,
      });
      setInputValue('');
    }
  }, [allocation]);

  // Format USDC amount for display in the input field
  const formatUsdcInputValue = (amount: number): string => {
    return (amount * 100).toFixed(0);
  };

  // Convert input value to actual USDC amount
  const getUsdcAmountFromInput = (value: string): number => {
    const numericValue = value ? parseInt(value, 10) : 0;
    return numericValue / 100;
  };

  // Handle USDC input change
  const handleUsdcInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    
    // Only allow numeric input
    if (value === '' || /^\d+$/.test(value)) {
      setInputValue(value);
      setFormData({
        ...formData,
        usdcAmount: getUsdcAmountFromInput(value)
      });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData as Allocation);
  };

  // Format the display value for the USDC amount
  const getFormattedUsdcDisplay = (): string => {
    if (inputValue === '') return '0.00';
    
    const numericValue = parseInt(inputValue, 10);
    const dollars = Math.floor(numericValue / 100);
    const cents = numericValue % 100;
    
    return `${dollars}.${cents.toString().padStart(2, '0')}`;
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>
          {allocation ? 'Edit Allocation' : 'New Allocation'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Symbol"
              value={formData.symbol}
              onChange={(e) =>
                setFormData({ ...formData, symbol: e.target.value.toUpperCase() })
              }
              required
              fullWidth
              helperText="Trading symbol (e.g., BTC-USD, ETH-USD)"
            />
            <TextField
              label="USDC Amount"
              type="text"
              value={inputValue}
              onChange={handleUsdcInputChange}
              required
              fullWidth
              helperText={`Amount of USDC to spend on each purchase: $${getFormattedUsdcDisplay()}`}
              InputProps={{
                inputProps: { 
                  pattern: "\\d*",
                  inputMode: "numeric"
                }
              }}
            />
            <Box display="flex" alignItems="center">
              <Switch
                checked={formData.isActive ?? true}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                color="success"
              />
              <Typography variant="body2" sx={{ ml: 1 }}>
                {formData.isActive ? 'Active' : 'Inactive'}
              </Typography>
            </Box>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button onClick={onClose} variant="outlined">Cancel</Button>
          <Button type="submit" variant="contained">
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}; 