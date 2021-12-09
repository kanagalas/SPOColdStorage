import React from "react";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

function handleLogin(instance: any) {
    instance.loginPopup(loginRequest).catch((e : Error) => {
        console.error(e);
    });
}

/**
 * Renders a button which, when selected, will open a popup for login
 */
export const SignInButton = () => {
    const { instance } = useMsal();

    return (
        <button className="ml-auto" onClick={() => handleLogin(instance)}>Sign in to Azure AD</button>
    );
}