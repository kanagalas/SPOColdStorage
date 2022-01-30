import React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import CloseIcon from '@mui/icons-material/Close';
import Slide from '@mui/material/Slide';
import { TransitionProps } from '@mui/material/transitions';


import { TargetMigrationSite } from '../TargetSitesInterfaces';
import { SiteList } from './SiteList';

interface Props {
    token: string,
    targetSite: TargetMigrationSite,
    open: boolean,
    onClose: Function
}


export const SiteBrowserDiag: React.FC<Props> = (props) => {

    const handleClose = () => {
        props.onClose();
    };
    const [spoToken, setSpoToken] = React.useState<string | null>(null);

    const getSpoToken = React.useCallback(async (token : string) => {
        return await fetch('AppConfiguration/GetSharePointToken', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token,
            }
        }
        )
            .then(async response => {
                const data: string = await response.text();
                setSpoToken(data);
                return Promise.resolve(data);
            })
            .catch(err => {

                alert('Loading SPO token failed');

                return Promise.reject();
            });
    }, []);


    React.useEffect(() => {
        if (props.token)
            getSpoToken(props.token);
    }, []);

    const Transition = React.forwardRef(function Transition(
        props: TransitionProps & {
            children: React.ReactElement;
        },
        ref: React.Ref<unknown>,
    ) {
        return <Slide direction="up" ref={ref} {...props} />;
    });

    return (
        <div>
            <Dialog
                fullScreen
                open={props.open}
                onClose={handleClose}
                TransitionComponent={Transition}>

                <AppBar sx={{ position: 'relative' }}>
                    <Toolbar>
                        <IconButton
                            edge="start"
                            color="inherit"
                            onClick={handleClose}
                            aria-label="close">
                            <CloseIcon />
                        </IconButton>
                        <Typography sx={{ ml: 2, flex: 1 }} variant="h6" component="div">
                            Select Contents to Migrate: {props.targetSite.rootURL}
                        </Typography>
                        <Button autoFocus color="inherit" onClick={handleClose}>
                            Save
                        </Button>
                    </Toolbar>
                </AppBar>
                {spoToken === null ?
                    (
                        <div>Loading</div>
                    ) :
                    (
                        <SiteList spoToken={spoToken!} targetSite={props.targetSite} />
                    )
                }
            </Dialog>
        </div>
    );
}
