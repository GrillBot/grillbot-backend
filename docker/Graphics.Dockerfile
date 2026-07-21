# The only non-.NET service in the repository: a Node/TypeScript renderer built
# on node-canvas, hence the native cairo/pango/giflib toolchain in both stages.
# Build context is the repository root.

ARG NODE_VERSION="22-alpine"

# Build phase
FROM node:${NODE_VERSION} AS build
WORKDIR /usr

RUN apk add build-base g++ cairo-dev jpeg-dev pango-dev giflib-dev
COPY src/Services/Graphics/package*.json ./
RUN npm ci
COPY src/Services/Graphics/ .
RUN npm run build

# Production phase
FROM node:${NODE_VERSION} AS production

# Variables
WORKDIR /usr/src/app
EXPOSE 3000
ENV NODE_ENV=production
LABEL org.opencontainers.image.source=https://github.com/GrillBot/grillbot-backend

# Dependencies
RUN apk add build-base g++ cairo-dev jpeg-dev pango-dev giflib-dev
RUN apk add terminus-font ttf-inconsolata ttf-dejavu font-noto font-noto-cjk ttf-font-awesome font-noto-extra

# Final build
COPY src/Services/Graphics/package*.json ./
RUN npm ci --omit=dev
COPY --from=build /usr/dist .

CMD ["node", "index.js"]
