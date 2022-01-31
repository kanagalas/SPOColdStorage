import React from 'react';
import { TargetMigrationSite } from '../TargetSitesInterfaces';

import { SPList, SPFolder, SPFolderResponse, SPAuthInfo } from './SPDefs';
import { TreeItem } from '@mui/lab';
import { Checkbox, FormControlLabel } from "@mui/material";

interface Props {
    spoAuthInfo: SPAuthInfo,
    list: SPList,
    targetSite: TargetMigrationSite
}

export const SiteNode: React.FC<Props> = (props) => {

    const [checked, setChecked] = React.useState<boolean>(false);
    const [error, setError] = React.useState<string | null>(null);
    const [folders, setFolders] = React.useState<SPFolder[] | null>(null);

    const checkChange = (checked: boolean) => {
        setChecked(checked);
    }

    const loadTree = () => {

        if (folders === null) {
            
            var viewXml = "<View Scope='RecursiveAll'><BeginsWith><FieldRef Name='ContentTypeId' />" + 
                "<Value Type='ContentTypeId'>0x0120</Value></BeginsWith>" + 
                "<Query><OrderBy><FieldRef Name='ID' Ascending='TRUE'/></OrderBy></Query>" + 
                "<RowLimit Paged='TRUE'>100</RowLimit></View>";
            
            var camlQueryBody = {
                "query": {
                    "__metadata": {
                        "type": "SP.CamlQuery"
                    },
                    "ViewXml": viewXml
                }
            };

            const url = `${props.targetSite.rootURL}/_api/web/lists/getByTitle('${props.list.Title}')/GetItems`;
            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; odata=verbose',
                    Accept: "application/json;odata=verbose",
                    "X-RequestDigest": props.spoAuthInfo.digest,
                    'Authorization': 'Bearer ' + props.spoAuthInfo.bearer,
                },
                body: JSON.stringify(camlQueryBody)
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
