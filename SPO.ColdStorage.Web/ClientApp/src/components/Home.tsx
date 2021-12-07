import { BlobItem, BlobServiceClient, ContainerClient } from '@azure/storage-blob';
import { Component } from 'react';
import {BlobFileList} from './BlobFileList';
import './NavMenu.css';

interface HomeState {
  client: ContainerClient | null,
  blobItems: BlobItem[],
  currentDirs: string[],
  storagePrefix: string,
  loading: boolean
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
      loading: true
    };
  }

  componentDidMount() {
    // Create a new BlobServiceClient
    const blobServiceClient = new BlobServiceClient('https://spocoldstoragedev.blob.core.windows.net/?sv=2020-08-04&ss=b&srt=co&sp=rwdlacitfx&se=2022-01-01T18:21:35Z&st=2021-12-07T10:21:35Z&spr=https&sig=%2FbjI%2FfgmVegqwsKdUwlPCvjdHqFu24h2gOA4HUEKrGU%3D');

    const containerName = "spexports";

    // Get a container client from the BlobServiceClient
    const client = blobServiceClient.getContainerClient(containerName);

    this.setState({ client: client });

    this.listFiles(client, "")
      .then(() => this.setState({ loading: false }))
      .catch(() => alert('Failed to load blob info'));
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

    const dirs = this.state.storagePrefix.split("/");

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
                  {dirs.map((dir, dirIdx) => {
                    if (dir) {
                      return <span>&gt;
                        <button onClick={() => this.breadcrumbDirClick(dirIdx, dirs)} className="link-button">{dir}</button>
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
