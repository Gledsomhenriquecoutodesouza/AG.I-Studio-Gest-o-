# syntax=docker/dockerfile:1.7
FROM node:24-alpine AS deps
WORKDIR /app
ENV NODE_USE_SYSTEM_CA=1
COPY package.json package-lock.json ./
RUN --mount=type=secret,id=local_ca,required=false \
    if [ -s /run/secrets/local_ca ]; then if [ -s /etc/ssl/cert.pem ]; then cat /etc/ssl/cert.pem /run/secrets/local_ca > /tmp/ca.pem && cp /tmp/ca.pem /etc/ssl/cert.pem; else cp /run/secrets/local_ca /etc/ssl/cert.pem; fi; fi && \
    apk add --no-cache ca-certificates && \
    if [ -s /run/secrets/local_ca ]; then cp /run/secrets/local_ca /usr/local/share/ca-certificates/local-build-ca.crt && update-ca-certificates; fi && npm ci
FROM node:24-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build
FROM node:24-alpine
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME=0.0.0.0
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
EXPOSE 3000
CMD ["node","server.js"]
