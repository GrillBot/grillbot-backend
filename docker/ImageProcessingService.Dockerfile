# syntax=docker/dockerfile:1.7-labs
#
# ImageProcessingService needs a runtime image with fontconfig, libgdiplus and a
# set of fonts, so it cannot use docker/dotnet-service.Dockerfile. The build
# stage is deliberately kept identical to the shared one so both images reuse
# the same restore layer.

ARG DOTNET_VERSION=10.0

# --------------------------------------------------------------------------- #
# Build                                                                        #
# --------------------------------------------------------------------------- #
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /repo

COPY Directory.Build.props Directory.Packages.props GrillBot.Backend.slnx ./
COPY --parents src/**/*.csproj tests/**/*.csproj ./
RUN dotnet restore GrillBot.Backend.slnx -r linux-x64

ARG PROJECT=src/Services/ImageProcessingService
COPY src/ ./src/
RUN dotnet publish "${PROJECT}" -c Release -o /publish \
    --no-restore -r linux-x64 --self-contained false

# --------------------------------------------------------------------------- #
# Runtime                                                                      #
# --------------------------------------------------------------------------- #
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final_image
LABEL org.opencontainers.image.source=https://github.com/GrillBot/grillbot-backend

ARG ASSEMBLY=ImageProcessingService.dll
ARG PORT=5213

WORKDIR /app
EXPOSE ${PORT}
ENV TZ=Europe/Prague
ENV ASPNETCORE_URLS="http://+:${PORT}"
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false
ENV FONTCONFIG_PATH=/etc/fonts
ENV FONTCONFIG_FILE=/etc/fonts/fonts.conf

RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tzdata \
        fontconfig \
        fontconfig-config \
        fonts-dejavu-core \
        fonts-dejavu-extra \
        fonts-liberation \
        fonts-open-sans \
        libgdiplus \
        libx11-6 \
        libc6-dev \
 && rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
RUN mkdir -p /etc/fonts/conf.d && fc-cache -fv
RUN ln -snf "/usr/share/zoneinfo/${TZ}" /etc/localtime && echo "${TZ}" > /etc/timezone

COPY --from=build /publish .

# The assembly name differs per image and the exec form of ENTRYPOINT cannot
# expand a build argument, but a shell wrapper must not be used to expand one
# here: /bin/sh is dash, and dash discards every inherited environment variable
# whose name is not a valid shell identifier. That is exactly the shape of an
# ASP.NET Core setting passed as "Section:Key" (ConnectionStrings:Default,
# RabbitMQ:Hostname, ...), so a shell entrypoint silently starts the app with
# those settings unset.
#
# Symlink the published apphost to a fixed name instead and exec it directly.
# The apphost resolves its .dll relative to its own real path, so the link works
# from the same directory, the container environment reaches the process
# untouched, and the app is PID 1 for Swarm's stop signals. `test -f` fails the
# build rather than the deployment if the apphost was not published (it needs
# `dotnet publish -r <rid>`, which is what the build stage above does).
RUN set -eu; \
    apphost="/app/$(basename "${ASSEMBLY}" .dll)"; \
    test -f "${apphost}"; \
    chmod +x "${apphost}"; \
    ln -s "${apphost}" /app/entrypoint

ENTRYPOINT ["/app/entrypoint"]
