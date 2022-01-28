import React from 'react';
import { TargetMigrationSite, ListFolderConfig } from './TargetSitesInterfaces';
import Button from '@mui/material/Button';

interface Props {
    token: string,
    targetSite: TargetMigrationSite,
    removeSiteUrl: Function
}

export const MigrationTarget: React.FC<Props> = (props) => {

    return (
        <div>
            <span>{props.targetSite.rootURL}</span>
            <span><Button onClick={() => props.removeSiteUrl(props.targetSite)}>Remove</Button></span>
            <ul>

                {props.targetSite.siteFilterConfig && props.targetSite.siteFilterConfig!.listFilterConfig.length === 0 ?
                    <li>Include all lists</li>
                    :
                    (
                        <div className='siteLists'>
                            {props.targetSite.siteFilterConfig!.listFilterConfig.map((listFolderConfig: ListFolderConfig) => (
                                <li>{listFolderConfig.listTitle}</li>
                            ))}

                        </div>
                    )
                }
                <li><Button onClick={() => props.removeSiteUrl(props.targetSite)}>Add folder to whitelist</Button></li>
            </ul>
        </div>
    );
}
