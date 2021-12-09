import { BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { BlobFileList } from './BlobFileList';
import './NavMenu.css';
import React, { useState } from 'react';
import { SignInButton } from "./SignInButton";
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

interface StorageInfo {
  sharedAccessToken: string,
  accountURI: string,
  containerName: string
}

export function Home() {

  const [client, setClient] = React.useState<ContainerClient | null>(null);
  const [loading, setLoading] = React.useState<boolean>(false);

  const isAuthenticated = useIsAuthenticated();
  const { instance, accounts } = useMsal();
  const [accessToken, setAccessToken] = useState<string>();

  const getStorageConfig = React.useCallback(async () => 
  {
    return await fetch('migrationrecord/GetStorageInfo', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + accessToken,
      }}
    )
    .then(async response => {
      const data: StorageInfo = await response.json();
      return Promise.resolve(data);
    })
    .catch(err => {

      // alert('Loading storage data failed');
      setLoading(false);

      return Promise.reject();
    });
  }, [accessToken]);

  const RequestAccessToken = React.useCallback(() => {
    const request = {
      ...loginRequest,
      account: accounts[0]
    };

    // Silently acquires an access token which is then attached to a request for Microsoft Graph data
    instance.acquireTokenSilent(request).then((response) => {
      setAccessToken(response.accessToken);
    }).catch((e) => {
      instance.acquireTokenPopup(request).then((response) => {
        setAccessToken(response.accessToken);
      });
    });
  }, [accounts, instance]);

  
  React.useEffect(() => {

    if (isAuthenticated && !accessToken) {
      RequestAccessToken();
    }

    if (accessToken) {

      // Load storage config first
      getStorageConfig()
      .then((storageConfigInfo: any) => {
        console.log('Got storage config from site API')

        // Create a new BlobServiceClient based on config loaded from our own API
        const blobServiceClient = new BlobServiceClient(`${storageConfigInfo.accountURI}${storageConfigInfo.sharedAccessToken}`);

        const containerName = storageConfigInfo.containerName;
        const blobStorageClient = blobServiceClient.getContainerClient(containerName);

        setClient(blobStorageClient);
      });
    }
  }, [accessToken, RequestAccessToken, getStorageConfig, isAuthenticated]);

    const name = accounts[0] && accounts[0].name;
    return (
      <div>
        <h1>Cold Storage Access Web</h1>

        <p>This application is for finding files moved into Azure Blob cold storage.</p>

        <AuthenticatedTemplate>
        <span>Signed In: {name}</span>
          <p><b>Files in Storage Account:</b></p>
          
          {!loading && client ?
            (
              <div>
                <BlobFileList client={client} accessToken={accessToken!} />
              </div>
            )
            : <div>Loading</div>
          }
        </AuthenticatedTemplate>
        <UnauthenticatedTemplate>
          <p>You are not signed in! Please sign in.</p>
          <SignInButton />
        </UnauthenticatedTemplate>
      </div>
    );
}
