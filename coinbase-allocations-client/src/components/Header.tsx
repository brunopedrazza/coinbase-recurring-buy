import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Box,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Divider,
  useTheme,
} from '@mui/material';
import { Brightness4, Brightness7, AccountCircle } from '@mui/icons-material';
import { useMsal } from '@azure/msal-react';

interface HeaderProps {
  toggleColorMode: () => void;
  mode: 'light' | 'dark';
}

export const Header: React.FC<HeaderProps> = ({ toggleColorMode, mode }) => {
  const { instance, accounts } = useMsal();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const theme = useTheme();
  
  const userName = accounts[0]?.name || 'User';
  const userEmail = accounts[0]?.username || '';
  
  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    instance.logoutPopup();
    handleClose();
  };

  // Get first letter of name for avatar
  const avatarLetter = userName.charAt(0).toUpperCase();

  return (
    <AppBar 
      position="fixed" 
      elevation={0}
      sx={{
        backgroundColor: theme.palette.mode === 'dark' 
          ? 'rgba(30, 33, 38, 0.8)' 
          : 'rgba(255, 255, 255, 0.8)',
        backdropFilter: 'blur(10px)',
        borderBottom: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.05)' : 'rgba(0, 0, 0, 0.05)'}`,
        width: '100%',
      }}
    >
      <Toolbar sx={{ justifyContent: 'space-between', px: { xs: 2, sm: 3, md: 4 } }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Box 
            component="img"
            src="/coinbase-logo.svg"
            alt="Coinbase Logo"
            sx={{ width: 32, height: 32, mr: 2 }}
          />
          <Typography variant="h6" color="textPrimary" sx={{ fontWeight: 600 }}>
            Coinbase Allocations
          </Typography>
        </Box>
        
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <IconButton 
            onClick={toggleColorMode} 
            color="inherit"
            sx={{ mr: 2 }}
          >
            {mode === 'dark' ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
          
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Box sx={{ display: { xs: 'none', sm: 'block' }, mr: 2, textAlign: 'right' }}>
              <Typography variant="body2" color="textPrimary" sx={{ fontWeight: 600 }}>
                {userName}
              </Typography>
              {userEmail && (
                <Typography variant="caption" color="textSecondary">
                  {userEmail}
                </Typography>
              )}
            </Box>
            
            <IconButton
              onClick={handleMenu}
              color="inherit"
              size="small"
              sx={{ 
                border: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'}`,
                p: 0.5,
              }}
            >
              {accounts[0]?.name ? (
                <Avatar 
                  sx={{ 
                    width: 32, 
                    height: 32, 
                    bgcolor: theme.palette.primary.main,
                    fontSize: '1rem',
                  }}
                >
                  {avatarLetter}
                </Avatar>
              ) : (
                <AccountCircle />
              )}
            </IconButton>
            
            <Menu
              id="menu-appbar"
              anchorEl={anchorEl}
              anchorOrigin={{
                vertical: 'bottom',
                horizontal: 'right',
              }}
              keepMounted
              transformOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
              open={Boolean(anchorEl)}
              onClose={handleClose}
              PaperProps={{
                elevation: 0,
                sx: {
                  overflow: 'visible',
                  filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.1))',
                  mt: 1.5,
                  borderRadius: 2,
                  minWidth: 180,
                  backgroundColor: theme.palette.mode === 'dark' 
                    ? 'rgba(30, 33, 38, 0.95)' 
                    : 'rgba(255, 255, 255, 0.95)',
                  backdropFilter: 'blur(10px)',
                  border: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.05)' : 'rgba(0, 0, 0, 0.05)'}`,
                  '& .MuiMenu-list': {
                    padding: '4px 0',
                  },
                },
              }}
            >
              <Box sx={{ px: 2, py: 1 }}>
                <Typography variant="body2" color="textPrimary" sx={{ fontWeight: 600 }}>
                  {userName}
                </Typography>
                {userEmail && (
                  <Typography variant="caption" color="textSecondary">
                    {userEmail}
                  </Typography>
                )}
              </Box>
              <Divider />
              <MenuItem onClick={handleLogout}>
                <Typography variant="body2">Logout</Typography>
              </MenuItem>
            </Menu>
          </Box>
        </Box>
      </Toolbar>
    </AppBar>
  );
}; 