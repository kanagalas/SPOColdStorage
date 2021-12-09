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
  const [storageInfo, setStorageInfo] = React.useState<StorageInfo | null>();
  const [blobItems, setBlobItems] = React.useState<BlobItem[]>([]);
  const [currentDirs, setCurrentDirs] = React.useState<string[]>([]);
  const [storagePrefix, setStoragePrefix] = React.useState<string>("");

  
  const isAuthenticated = useIsAuthenticated();
  const { instance, accounts, inProgress } = useMsal();
  const [accessToken, setAccessToken] = useState<string>();

  React.useEffect(() => {

    if (isAuthenticated && !accessToken) {
      RequestAccessToken();
    }

    if (accessToken) {
      
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
  })

  async function getStorageConfig(): Promise<StorageInfo> {
    return await fetch('migrationrecord/GetStorageInfo', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + accessToken,
        }}
      )
      .then(async response => {

        const data: StorageInfo = await response.json();
        console.log(data);
        setStorageInfo(data);
        return Promise.resolve(data);
      })
      .catch(err => {
        // alert('Loading storage data failed');
        setStorageInfo(null);
        setLoading(false);

        return Promise.reject();
      });
  }

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

  function RequestAccessToken() {
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
  }

  const name = accounts[0] && accounts[0].name;
  if (loading) {
    return <div>Loading</div>;
  }
  else {
    const breadcumbDirs = storagePrefix!.split("/");

    return (
      <div>
        <h1>Cold Storage Access Web</h1>
        {isAuthenticated ? <span>Signed In: {name}</span> : <SignInButton />}

        <p>This application is for finding files moved into Azure Blob cold storage.</p>
        <p><b>Files in Storage Account:</b></p>


        <AuthenticatedTemplate>
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
        </UnauthenticatedTemplate>
      </div>
    );
  }


}
