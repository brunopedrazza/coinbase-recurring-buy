import { MsalProvider, AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { PublicClientApplication, EventType, EventMessage, AuthError } from "@azure/msal-browser";
import { ThemeProvider, CssBaseline, Box, IconButton, useMediaQuery, useTheme, Paper, Typography, Button } from "@mui/material";
import { Brightness4, Brightness7 } from '@mui/icons-material';
import { AllocationManager } from "./components/AllocationManager";
import { Header } from "./components/Header";
import { msalConfig, b2cPolicies, loginRequest } from "./auth/authConfig";
import { useState, useEffect, createContext, useContext, useMemo } from "react";
import { PaletteMode } from "@mui/material";
import { getTheme } from "./theme/theme";

interface ColorModeContextType {
  toggleColorMode: () => void;
}

const ColorModeContext = createContext<ColorModeContextType>({ toggleColorMode: () => {} });

const msalInstance = new PublicClientApplication(msalConfig);

// Account selection logic is app dependent. Adjust as needed for different use cases.
const accounts = msalInstance.getAllAccounts();
if (accounts.length > 0) {
  msalInstance.setActiveAccount(accounts[0]);
}

msalInstance.addEventCallback((event: EventMessage) => {
  if (event.eventType === EventType.LOGIN_FAILURE) {
    const error = event.error as AuthError;
    
    // Check if the error is due to the user clicking the "Forgot Password" link
    if (error && error.errorCode === "AADB2C90118") {
      // Initiate the password reset flow
      msalInstance.loginPopup({
        authority: b2cPolicies.authorities.passwordReset.authority,
        scopes: loginRequest.scopes
      }).then(() => {
        // After password reset, sign the user in with the sign-in policy
        return msalInstance.loginPopup({
          authority: b2cPolicies.authorities.signIn.authority,
          scopes: loginRequest.scopes
        });
      }).catch(err => {
        console.error("Password reset error:", err);
      });
    }
  }
});

function SignInButton() {
  const { instance } = useMsal();
  const colorMode = useContext(ColorModeContext);
  const theme = useTheme();

  const handleLogin = () => {
    instance.loginPopup().catch(error => {
      // This is handled by the event callback above
      console.log("Login error:", error);
    });
  };

  return (
    <Box sx={{ 
      height: '100vh', 
      width: '100%',
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      px: { xs: 2, sm: 3, md: 4 }
    }}>
      <Paper 
        elevation={0} 
        sx={{ 
          p: 5, 
          borderRadius: 3,
          backgroundColor: theme.palette.mode === 'dark' ? 'rgba(30, 33, 38, 0.8)' : 'rgba(255, 255, 255, 0.8)',
          backdropFilter: 'blur(10px)',
          border: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.05)' : 'rgba(0, 0, 0, 0.05)'}`,
          width: '100%',
          maxWidth: '500px',
          textAlign: 'center',
          position: 'relative',
        }}
      >
        <IconButton 
          onClick={colorMode.toggleColorMode} 
          sx={{ 
            position: 'absolute', 
            top: 16, 
            right: 16,
            color: theme.palette.mode === 'dark' ? 'white' : 'rgba(0, 0, 0, 0.7)'
          }}
        >
          {theme.palette.mode === 'dark' ? <Brightness7 /> : <Brightness4 />}
        </IconButton>
        
        <Box sx={{ mb: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <Box 
            component="img"
            src="/coinbase-logo.svg"
            alt="Coinbase Logo"
            sx={{ width: 80, height: 80, mb: 3 }}
          />
          <Typography variant="h4" gutterBottom fontWeight="bold">
            Coinbase Allocations
          </Typography>
          <Typography variant="subtitle1" color="text.secondary">
            Manage your recurring buy allocations
          </Typography>
        </Box>
        
        <Button
          variant="contained"
          size="large"
          onClick={handleLogin}
          sx={{
            py: 1.5,
            px: 4,
            borderRadius: 2,
            fontWeight: 600,
            fontSize: '1rem',
          }}
        >
          Sign In
        </Button>
      </Paper>
    </Box>
  );
}

function App() {
  const prefersDarkMode = useMediaQuery('(prefers-color-scheme: dark)');
  const [mode, setMode] = useState<PaletteMode>(prefersDarkMode ? 'dark' : 'light');

  const colorMode = useMemo(
    () => ({
      toggleColorMode: () => {
        setMode((prevMode) => (prevMode === 'light' ? 'dark' : 'light'));
      },
    }),
    []
  );

  const theme = useMemo(() => getTheme(mode), [mode]);

  useEffect(() => {
    document.body.style.backgroundColor = theme.palette.background.default;
  }, [theme]);

  return (
    <ColorModeContext.Provider value={colorMode}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <MsalProvider instance={msalInstance}>
          <Box 
            sx={{ 
              minHeight: '100vh',
              width: '100%',
              background: theme.palette.mode === 'dark' 
                ? 'linear-gradient(135deg, rgba(22, 26, 30, 0.8) 0%, rgba(30, 33, 38, 0.8) 100%)' 
                : 'linear-gradient(135deg, rgba(248, 249, 250, 0.8) 0%, rgba(255, 255, 255, 0.8) 100%)',
              position: 'relative',
            }}
          >
            <AuthenticatedTemplate>
              <Header toggleColorMode={colorMode.toggleColorMode} mode={mode} />
              <Box sx={{ pt: { xs: 10, sm: 12 } }}>
                <AllocationManager />
              </Box>
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
              <SignInButton />
            </UnauthenticatedTemplate>
          </Box>
        </MsalProvider>
      </ThemeProvider>
    </ColorModeContext.Provider>
  );
}

export default App;
