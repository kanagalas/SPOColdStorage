import { BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { BlobFileList } from './BlobFileList';
import '../NavMenu.css';
import React from 'react';
import { useIsAuthenticated, useMsal } from "@azure/msal-react";

export interface StorageInfo {
  sharedAccessToken: string,
  accountURI: string,
  containerName: string
}

export const FileBrowser : React.FC<{token:string}> = (props) => {

  const [client, setClient] = React.useState<ContainerClient | null>(null);
  const [storageInfo, setStorageInfo] = React.useState<StorageInfo | null>(null);
  const [loading, setLoading] = React.useState<boolean>(false);

  const isAuthenticated = useIsAuthenticated();
  const { accounts } = useMsal();

  const getStorageConfig = React.useCallback(async (token) => 
  {
    return await fetch('AppConfiguration/GetStorageInfo', {
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
        console.log('Got storage config from site API');
        setStorageInfo(storageConfigInfo);

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

        <span>Signed In: {name}</span>
          <p><b>Files in Storage Account:</b></p>
          
          {!loading && client ?
            (
              <div>
                <BlobFileList client={client!} accessToken={props.token} storageInfo={storageInfo!} />
              </div>
            )
            : <div>Loading</div>
          }
      </div>
    );
};
