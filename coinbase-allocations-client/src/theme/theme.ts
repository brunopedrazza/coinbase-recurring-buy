import { PaletteMode } from '@mui/material';
import { createTheme } from '@mui/material/styles';

export const getTheme = (mode: PaletteMode) => createTheme({
  palette: {
    mode,
    primary: {
      main: '#3861FB',
      light: '#6B8AFB',
      dark: '#2D4CC8',
    },
    secondary: {
      main: '#14F195',
      light: '#47F4AE',
      dark: '#0FBF75',
    },
    background: {
      default: mode === 'light' ? '#F8F9FA' : '#161A1E',
      paper: mode === 'light' ? '#FFFFFF' : '#1E2126',
    },
    text: {
      primary: mode === 'light' ? '#1E2126' : '#FFFFFF',
      secondary: mode === 'light' ? '#6E7881' : '#B4B9BD',
    },
  },
  shape: {
    borderRadius: 12,
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: mode === 'light' 
            ? '0px 2px 4px rgba(0, 0, 0, 0.05)' 
            : '0px 2px 4px rgba(0, 0, 0, 0.2)',
          transition: 'transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out',
          '&:hover': {
            transform: 'translateY(-2px)',
            boxShadow: mode === 'light'
              ? '0px 4px 8px rgba(0, 0, 0, 0.1)'
              : '0px 4px 8px rgba(0, 0, 0, 0.3)',
          },
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          padding: '8px 16px',
        },
        contained: {
          boxShadow: 'none',
          '&:hover': {
            boxShadow: 'none',
          },
        },
      },
    },
    MuiSwitch: {
      styleOverrides: {
        root: {
          width: 42,
          height: 26,
          padding: 0,
        },
        switchBase: {
          padding: 1,
          '&.Mui-checked': {
            transform: 'translateX(16px)',
            color: '#fff',
            '& + .MuiSwitch-track': {
              backgroundColor: '#14F195',
              opacity: 1,
            },
          },
        },
        thumb: {
          width: 24,
          height: 24,
        },
        track: {
          borderRadius: 13,
          border: 'none',
          opacity: 1,
          backgroundColor: mode === 'light' ? '#E9ECEF' : '#343A3F',
        },
      },
    },
  },
}); 