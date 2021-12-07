import { BlobItem, BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { Component } from 'react';
import {BlobFileList} from './BlobFileList';
import './NavMenu.css';

interface HomeState {
  client: ContainerClient | null,
  blobItems: BlobItem[],
  currentDirs: string[],
  storagePrefix: string,
  loading: boolean,
  storageInfo: StorageInfo | null
}

interface StorageInfo
{
  sharedAccessToken : string,
  accountURI: string,
  containerName: string
}

export class Home extends Component<{}, HomeState> {

  constructor(props: any) {
    super(props);
    this.state =
    {
      blobItems: [],
      client: null,
      storagePrefix: "",
      currentDirs: [],
      loading: true,
      storageInfo: null
    };
  }

  componentDidMount() {

    // Load storage config first
    this.getStorageConfig()
      .then(storageConfigInfo => {
        // Create a new BlobServiceClient based on config loaded from our own API
        const blobServiceClient = new BlobServiceClient(`${storageConfigInfo.accountURI}${storageConfigInfo.sharedAccessToken}`);

        const containerName = storageConfigInfo.containerName;
        const client = blobServiceClient.getContainerClient(containerName);

        this.setState({ client: client });

        // Get blobs for root folder
        this.listFiles(client, "")
          .then(() => this.setState({ loading: false }))
          .catch(() => alert('Failed to load blob info from Azure blob'));
      });
  }

  async getStorageConfig() : Promise<StorageInfo>
  {
    return await fetch('migrationrecord/GetStorageInfo')
      .then(async response => {

          const data : StorageInfo = await response.json();
          console.log(data);
          this.setState({ storageInfo: data});
          return Promise.resolve(data);
      })
      .catch(err => {
          alert('Loading storage data failed');
          this.setState({ storageInfo: null, loading: false });

          return Promise.reject();
      });
  }

  async listFiles(client: ContainerClient, prefix: string) {

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

      this.setState({ blobItems: blobs, currentDirs: dirs, storagePrefix: prefix });

      return Promise.resolve();
    } catch (error) {
      return Promise.reject(error);
    }
  };

  breadcrumbDirClick(dirIdx: number, allDirs: string[]) {
    let fullPath: string = "";

    for (let index = 0; index <= dirIdx; index++) {
      const thisDir = allDirs[index];
      fullPath += `${thisDir}/`;
    }

    this.listFiles(this.state.client!, fullPath);
  }

  render() {
    const breadcumbDirs = this.state.storagePrefix.split("/");

    return (
      <div>
        <h1>Cold Storage Access Web</h1>
        <p>This application is for finding files moved into Azure Blob cold storage.</p>
        <p><b>Files in Storage Account:</b></p>
        {this.state.loading === false ?
          (
            <div>
              <div id="breadcrumb-file-nav">
                <span>
                  <span>Home</span>
                  {breadcumbDirs.map((breadcumbDir, dirIdx) => {
                    if (breadcumbDir) {
                      return <span>&gt;
                        <button onClick={() => this.breadcrumbDirClick(dirIdx, breadcumbDirs)} className="link-button">
                          {breadcumbDir}
                        </button>
                      </span>
                    }
                    else
                      return <span />
                  })}
                </span>
              </div>

            <BlobFileList navToFolderCallback={ (dir : string) => this.listFiles(this.state.client!, dir)} 
              storagePrefix={this.state.storagePrefix} blobItems={this.state.blobItems} 
              currentDirs={this.state.currentDirs}/>
            </div>
          )
          : <div>Loading</div>
        }

      </div>
    );
  }
}
