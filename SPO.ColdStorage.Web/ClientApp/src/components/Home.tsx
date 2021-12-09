import { BlobItem, BlobServiceClient, ContainerClient } from '@azure/storage-blob';
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
  const [blobItems, setBlobItems] = React.useState<BlobItem[] | null>(null);
  const [currentDirs, setCurrentDirs] = React.useState<string[]>([]);
  const [storagePrefix, setStoragePrefix] = React.useState<string>("");

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

  
  async function listFiles(client: ContainerClient, prefix: string) {

    let dirs: string[] = [];
    let blobs: BlobItem[] = [];

    console.log("Browsing blobs with prefix: " + prefix);

    try {
      let iter = client!.listBlobsByHierarchy("/", { prefix: prefix });

      for await (const item of iter) {
        if (item.kind === "prefix") {
          dirs.push(item.name);
        } else {
          blobs.push(item);
        }
      }

      setBlobItems(blobs);
      setCurrentDirs(dirs);
      setStoragePrefix(prefix);

      return Promise.resolve();
    } catch (error) {
      return Promise.reject(error);
    }
  };

  function breadcrumbDirClick(dirIdx: number, allDirs: string[]) {
    let fullPath: string = "";

    for (let index = 0; index <= dirIdx; index++) {
      const thisDir = allDirs[index];
      fullPath += `${thisDir}/`;
    }

    listFiles(client!, fullPath);
  }

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

    if (accessToken && !blobItems) {
      setLoading(true);

      // Load storage config first
      getStorageConfig()
      .then((storageConfigInfo: any) => {
        // Create a new BlobServiceClient based on config loaded from our own API
        const blobServiceClient = new BlobServiceClient(`${storageConfigInfo.accountURI}${storageConfigInfo.sharedAccessToken}`);

        const containerName = storageConfigInfo.containerName;
        const client = blobServiceClient.getContainerClient(containerName);

        setClient(client);

        // Get blobs for root folder
        listFiles(client, "")
          .then(() => setLoading(false));
      });
    }
  }, [accessToken, RequestAccessToken, blobItems, getStorageConfig, isAuthenticated]);



  const name = accounts[0] && accounts[0].name;

    const breadcumbDirs = storagePrefix?.split("/") ?? "";

    return (
      <div>
        <h1>Cold Storage Access Web</h1>

        <p>This application is for finding files moved into Azure Blob cold storage.</p>

        <AuthenticatedTemplate>
        <span>Signed In: {name}</span>
          <p><b>Files in Storage Account:</b></p>
          
          {loading === false ?
            (
              <div>
                <div id="breadcrumb-file-nav">
                  <span>
                    <span>Home</span>
                    {breadcumbDirs.map((breadcumbDir, dirIdx) => {
                      if (breadcumbDir) {
                        return <span>&gt;
                          <button onClick={() => breadcrumbDirClick(dirIdx, breadcumbDirs)} className="link-button">
                            {breadcumbDir}
                          </button>
                        </span>
                      }
                      else
                        return <span />
                    })}
                  </span>
                </div>

                <BlobFileList navToFolderCallback={(dir: string) => listFiles(client!, dir)}
                  storagePrefix={storagePrefix!} blobItems={blobItems!}
                  currentDirs={currentDirs!} />
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
