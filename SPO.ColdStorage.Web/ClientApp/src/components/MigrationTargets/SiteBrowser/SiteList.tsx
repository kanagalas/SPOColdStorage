import React from 'react';
import ListItemText from '@mui/material/ListItemText';
import ListItem from '@mui/material/ListItem';
import List from '@mui/material/List';
import Divider from '@mui/material/Divider';

import { TargetMigrationSite } from '../TargetSitesInterfaces';

interface Props {
    spoToken: string,
    targetSite: TargetMigrationSite
}

export const SiteList: React.FC<Props> = (props) => {

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
                const data: string = await response.json();

                console.log(data);
                return Promise.resolve(data);
            });
    }, []);
    
    React.useEffect(() => {
        getSiteLists();
    }, []);

    return (

        <List>
            <ListItem button>
                <ListItemText primary="Phone ringtone" secondary="Titania" />
            </ListItem>
            <Divider />
            <ListItem button>
                <ListItemText
                    primary="Default notification ringtone"
                    secondary="Tethys"
                />
            </ListItem>
        </List>
    );
}
