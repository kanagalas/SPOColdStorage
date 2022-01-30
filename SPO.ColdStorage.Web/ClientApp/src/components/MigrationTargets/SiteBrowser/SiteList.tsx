import React from 'react';
import ListItemText from '@mui/material/ListItemText';
import ListItem from '@mui/material/ListItem';
import List from '@mui/material/List';
import Divider from '@mui/material/Divider';
import { TargetMigrationSite } from '../TargetSitesInterfaces';

interface SPListResponse {
    d: SPListResponseData
}
interface SPListResponseData {
    results: SPList[]
}
interface SPList {
    Title: string,
    Description: string,
    Id: string,
    Hidden: boolean,
    NoCrawl: boolean
}
interface Props {
    spoToken: string,
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
                'Authorization': 'Bearer ' + props.spoToken,
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

    return (
        <div>
            {lists === null ?
                (
                    <div>Loading lists...</div>
                )
                :
                (
                    <List>
                        {lists.map((list: SPList) =>
                        (
                            <div>
                                <ListItem button>
                                    <ListItemText primary={list.Title} secondary={list.Description} />
                                </ListItem>

                                <Divider />
                            </div>
                        ))
                        }
                    </List>
                )
            }
        </div>
    );
}
