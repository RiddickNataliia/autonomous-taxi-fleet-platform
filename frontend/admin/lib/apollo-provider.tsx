"use client";

import { ApolloProvider } from "@apollo/client/react";
import { Auth0Provider } from "@auth0/auth0-react";
import { apolloClient } from "./apollo-client";

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <Auth0Provider
      domain={process.env.NEXT_PUBLIC_AUTH0_DOMAIN!}
      clientId={process.env.NEXT_PUBLIC_AUTH0_CLIENT_ID!}
      authorizationParams={{
        redirect_uri: typeof window !== "undefined" ? window.location.origin + "/callback" : "",
        audience: process.env.NEXT_PUBLIC_AUTH0_AUDIENCE,
        scope: "openid profile email",
        prompt: "login",
      }}
    >
      <ApolloProvider client={apolloClient}>
        {children}
      </ApolloProvider>
    </Auth0Provider>
  );
}