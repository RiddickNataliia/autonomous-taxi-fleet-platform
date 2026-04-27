import { ApolloClient, InMemoryCache, HttpLink, ApolloLink } from "@apollo/client/core";

const httpLink = new HttpLink({
  uri: process.env.NEXT_PUBLIC_GRAPHQL_URL,
});

const authLink = new ApolloLink((operation, forward) => {
  const token = typeof window !== "undefined"
    ? localStorage.getItem("admin_token")
    : null;

  operation.setContext(({ headers = {} }: { headers: Record<string, string> }) => ({
    headers: {
      ...headers,
      authorization: token ? `Bearer ${token}` : "",
    },
  }));

  return forward(operation);
});

export const apolloClient = new ApolloClient({
  link: authLink.concat(httpLink),
  cache: new InMemoryCache(),
});