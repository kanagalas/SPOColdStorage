import { BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { BlobFileList } from './BlobFileList';
import './NavMenu.css';
import React from 'react';
import { SignInButton } from "./SignInButton";
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";

interface StorageInfo {
  sharedAccessToken: string,
  accountURI: string,
  containerName: string
}

export const Home : React.FC<{token:string}> = (props) => {

  const [client, setClient] = React.useState<ContainerClient | null>(null);
  const [loading, setLoading] = React.useState<boolean>(false);

  const isAuthenticated = useIsAuthenticated();
  const { accounts } = useMsal();

  const getStorageConfig = React.useCallback(async (token) => 
  {
    return await fetch('migrationrecord/GetStorageInfo', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token,
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
  }, []);

  
  React.useEffect(() => {

    if (props.token) {

      // Load storage config first
      getStorageConfig(props.token)
      .then((storageConfigInfo: any) => {
        console.log('Got storage config from site API')

        // Create a new BlobServiceClient based on config loaded from our own API
        const blobServiceClient = new BlobServiceClient(`${storageConfigInfo.accountURI}${storageConfigInfo.sharedAccessToken}`);

        const containerName = storageConfigInfo.containerName;
        const blobStorageClient = blobServiceClient.getContainerClient(containerName);

        setClient(blobStorageClient);
      });
    }
  }, [getStorageConfig, isAuthenticated, props]);

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
                <BlobFileList client={client!} accessToken={props.token} />
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
};
