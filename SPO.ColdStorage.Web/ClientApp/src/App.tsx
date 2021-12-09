import React, { useState } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { FileBrowser } from './components/FileBrowser/FileBrowser';
import { Login } from './components/Login';
import { FindLog } from './components/MigrationLogs/FindLog';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useIsAuthenticated, useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";

import './custom.css'
import { MigrationTargetsConfig } from './components/MigrationTargets/MigrationTargetsConfig';

export default function App() {

    const [accessToken, setAccessToken] = useState<string>();
    const isAuthenticated = useIsAuthenticated();
    const { instance, accounts } = useMsal();

    const RequestAccessToken = React.useCallback(() => {
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
    }, [accounts, instance]);

    React.useEffect(() => {

        // Get OAuth token
        if (isAuthenticated && !accessToken) {
            RequestAccessToken();
        }
    }, [accessToken, RequestAccessToken, isAuthenticated]);


    return (
        <Layout>
            <AuthenticatedTemplate>
                <Route exact path='/' render={() => <FileBrowser {... { token: accessToken! }} />} />
                <Route path='/FindLog' render={() => <FindLog {... { token: accessToken! }} />} />
                <Route path='/MigrationTargets' render={() => <MigrationTargetsConfig {... { token: accessToken! }} />} />
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                <Route exact path='/' component={Login} />
            </UnauthenticatedTemplate>
        </Layout>
    );

}