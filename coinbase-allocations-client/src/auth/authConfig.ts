import { Configuration, RedirectRequest } from "@azure/msal-browser";

// Azure B2C configuration
export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_CLIENT_ID as string,
    authority: `https://${import.meta.env.VITE_B2C_TENANT_NAME}.b2clogin.com/${import.meta.env.VITE_B2C_TENANT_NAME}.onmicrosoft.com/B2C_1_signin`,
    knownAuthorities: [`${import.meta.env.VITE_B2C_TENANT_NAME}.b2clogin.com`],
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
};

// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest: RedirectRequest = {
  scopes: ["openid", "profile", import.meta.env.VITE_API_SCOPE as string],
};

// Password reset authority
export const b2cPolicies = {
  names: {
    signIn: "B2C_1_signin",
    passwordReset: "B2C_1_password_reset",
  },
  authorities: {
    signIn: {
      authority: `https://${import.meta.env.VITE_B2C_TENANT_NAME}.b2clogin.com/${import.meta.env.VITE_B2C_TENANT_NAME}.onmicrosoft.com/B2C_1_signin`,
    },
    passwordReset: {
      authority: `https://${import.meta.env.VITE_B2C_TENANT_NAME}.b2clogin.com/${import.meta.env.VITE_B2C_TENANT_NAME}.onmicrosoft.com/B2C_1_password_reset`,
    },
  },
  authorityDomain: `${import.meta.env.VITE_B2C_TENANT_NAME}.b2clogin.com`,
};

// Add the endpoints here for Microsoft Graph API services you'd like to use.
export const apiConfig = {
  allocationsFetchEndpoint: `${import.meta.env.VITE_FUNCTION_APP_URL}/api/GetAllocations`,
  allocationsUpdateEndpoint: `${import.meta.env.VITE_FUNCTION_APP_URL}/api/UpdateAllocations`,
  functionKey: import.meta.env.VITE_FUNCTION_KEY as string,
}; 
