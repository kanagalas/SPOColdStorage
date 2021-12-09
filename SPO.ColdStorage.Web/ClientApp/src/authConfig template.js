export const msalConfig = {
    auth: {
      clientId: "8179f97c-bfd6-4ca0-9b69-a02fc2430121",
      authority: "https://login.microsoftonline.com/ffcdb539-892e-4eef-94f6-0d9851c479ba", // This is a URL (e.g. https://login.microsoftonline.com/{your tenant ID})
      redirectUri: "https://localhost:44443/",
    },
    cache: {
      cacheLocation: "sessionStorage", // This configures where your cache will be stored
      storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
    }
  };
  
  // Add scopes here for ID token to be used at Microsoft identity platform endpoints.
  export const loginRequest = {
   scopes: ["api://8179f97c-bfd6-4ca0-9b69-a02fc2430121/access"]
  };
  
  // Add the endpoints here for Microsoft Graph API services you'd like to use.
  export const graphConfig = {
      graphMeEndpoint: "https://localhost:44443/MigrationRecord/GetStorageInfo"
  };
