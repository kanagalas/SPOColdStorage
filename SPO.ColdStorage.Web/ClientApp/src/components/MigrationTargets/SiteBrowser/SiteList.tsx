import React from 'react';
import { TargetMigrationSite } from '../TargetSitesInterfaces';
import { TreeView } from '@mui/lab';
import { SiteNode } from "./SiteNode";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { SPAuthInfo, SPList, SPListResponse } from './SPDefs';

interface Props {
    spoAuthInfo: SPAuthInfo,
    targetSite: TargetMigrationSite
}

export const SiteList: React.FC<Props> = (props) => {
    const [lists, setLists] = React.useState<SPList[] | null>(null);

    const getSiteLists = React.useCallback(async () => {
        return await fetch(`${props.targetSite.rootURL}/_api/web/lists`, {
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
                const data: SPListResponse = JSON.parse(responseText);

                if (data.d?.results) {
                    setLists(data.d.results);
                    return Promise.resolve(data);
                }
                else {
                    alert('Unexpected response from SharePoint for lists: ' + responseText);
                    return Promise.reject();
                }
            });
    }, []);

    React.useEffect(() => {
        getSiteLists();
    }, []);

    const renderTree = (nodes: SPList[]) => (
        nodes.map((node: SPList) => 
        (
            <SiteNode list={node} spoAuthInfo={props.spoAuthInfo} targetSite={props.targetSite} />
        ))
    );

    const onNodeToggle = (e: any, nodeId: string[]) => 
    {

    }

    return (
        <div>
            {lists === null ?
                (
                    <div>Loading lists...</div>
                )
                :
                (
                    <TreeView onNodeToggle={onNodeToggle}
                        defaultCollapseIcon={<ExpandMoreIcon />}
                        defaultExpandIcon={<ChevronRightIcon />}
                    >
                        {renderTree(lists)}
                    </TreeView>
                )
            }
        </div>
    );
}
