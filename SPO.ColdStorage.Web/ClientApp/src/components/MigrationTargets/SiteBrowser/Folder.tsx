import React from 'react';
import { TargetMigrationSite } from '../TargetSitesInterfaces';

import { SPList, SPFolder, SPFolderResponse, SPAuthInfo } from './SPDefs';
import { TreeItem } from '@mui/lab';
import { Checkbox, FormControlLabel } from "@mui/material";

interface Props {
    spoAuthInfo: SPAuthInfo,
    parentFolder: SPFolder,    
    list: SPList,
    targetSite: TargetMigrationSite
}


export const Folder: React.FC<Props> = (props) => {
    const [error, setError] = React.useState<string | null>(null);
    const [checked, setChecked] = React.useState<boolean>(false);
    const [folders, setFolders] = React.useState<SPFolder[] | null>(null);

    const checkChange = (checked: boolean) => {
        setChecked(checked);
    }

    const loadTree = () => {

        if (folders === null) {
            const url = `${props.targetSite.rootURL}/_api/web/GetFolderByServerRelativeUrl('${props.parentFolder.ServerRelativeUrl}')/items`;
            fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    Accept: "application/json;odata=verbose",
                    'Authorization': 'Bearer ' + props.spoAuthInfo.bearer,
                }
            }
            )
                .then(async response => {

                    var responseText = await response.text();
                    const data: SPFolderResponse = JSON.parse(responseText);

                    if (data.d?.results) {
                        setFolders(data.d.results);
                        return Promise.resolve(data);
                    }
                    else {
                        setError(responseText);
                        //alert('Unexpected response from SharePoint for list folders: ' + responseText);
                        return Promise.reject();
                    }
                });
        }
    }


    if (error === null) {
        return (
            <TreeItem
                key={props.list.Id}
                nodeId={props.list.Id}
                label={
                    <FormControlLabel
                        control={
                            <Checkbox checked={checked}
                                onChange={event => checkChange(event.currentTarget.checked)}
                                onClick={e => e.stopPropagation()}
                            />
                        }
                        label={<>{props.list.Title}</>}
                        key={props.list.Id}
                    />
                }
            >
                <div>
                    {folders === null ?
                        (
                            <div>Loading list folders...
                                {loadTree()}
                            </div>
                        ) :
                        (
                            folders.map((folder: SPFolder) => 
                                <TreeItem
                                    nodeId={folder.ServerRelativeUrl}
                                    label={
                                        <FormControlLabel
                                            control={
                                                <Checkbox checked={checked}
                                                    onChange={event => checkChange(event.currentTarget.checked)}
                                                    onClick={e => e.stopPropagation()}
                                                />
                                            }
                                            label={<>{folder.Name}</>}
                                            key={folder.ServerRelativeUrl}
                                        />
                                    }>
                                </TreeItem>
                            )
                        )
                    }
                </div>
    
            </TreeItem>
        );
    }
    else
        return (
            <TreeItem
                key={props.list.Id}
                nodeId={props.list.Id}
                label={error}
            />
        );
}