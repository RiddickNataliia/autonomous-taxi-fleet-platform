FROM node:22-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ENV NEXT_PUBLIC_API_URL=http://localhost:8080
ENV NEXT_PUBLIC_GRAPHQL_URL=http://localhost:8080/graphql
ENV NEXT_PUBLIC_AUTH0_DOMAIN=dev-wcshgsensn2evwsg.us.auth0.com
ENV NEXT_PUBLIC_AUTH0_CLIENT_ID=fdT2DaRIfCV4hTvPVaBURzeyGDT0mHhf
ENV NEXT_PUBLIC_AUTH0_AUDIENCE=https://novadrive-api
RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]