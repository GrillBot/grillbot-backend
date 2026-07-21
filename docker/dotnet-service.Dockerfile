# syntax=docker/dockerfile:1.7-labs
#
# Shared image definition for every .NET deployable in this repository: the bot
# and all twelve .NET microservices. The per-image values come from
# docker/deployables.json and are passed in as build arguments.
#
# The build context is the repository root, because the projects are wired
# together with project references and share Directory.Build.props /
# Directory.Packages.props.
#
#   docker build -f docker/dotnet-service.Dockerfile \
#     --build-arg PROJECT=src/Services/AuditLogService \
#     --build-arg ASSEMBLY=AuditLogService.dll \
#     --build-arg PORT=5071 .

ARG DOTNET_VERSION=10.0

# --------------------------------------------------------------------------- #
# Build                                                                        #
# --------------------------------------------------------------------------- #
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /repo

# Restore is done from the project files alone, so this layer is identical for
# every image built from this repository and is reused until a dependency
# actually changes. No NuGet feed or token is needed - the former
# GrillBot.Core packages are project references now.
COPY Directory.Build.props Directory.Packages.props GrillBot.Backend.slnx ./
COPY --parents src/**/*.csproj tests/**/*.csproj ./
RUN dotnet restore GrillBot.Backend.slnx -r linux-x64

ARG PROJECT
COPY src/ ./src/
RUN dotnet publish "${PROJECT}" -c Release -o /publish \
    --no-restore -r linux-x64 --self-contained false

# dotnet-dump / dotnet-gcdump, for services that are profiled in production.
ARG WITH_DIAGNOSTICS=false
RUN if [ "${WITH_DIAGNOSTICS}" = "true" ]; then \
        dotnet tool install -g dotnet-dump && dotnet tool install -g dotnet-gcdump; \
    else \
        mkdir -p /root/.dotnet; \
    fi

# --------------------------------------------------------------------------- #
# Runtime                                                                      #
# --------------------------------------------------------------------------- #
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final_image
LABEL org.opencontainers.image.source=https://github.com/GrillBot/grillbot-backend

ARG ASSEMBLY
ARG PORT
ARG WITH_DIAGNOSTICS=false

WORKDIR /app
EXPOSE ${PORT}
ENV TZ=Europe/Prague
ENV ASPNETCORE_URLS="http://+:${PORT}"
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false
ENV ENTRY_ASSEMBLY=${ASSEMBLY}

RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        tzdata \
        libc6-dev \
        $(if [ "${WITH_DIAGNOSTICS}" = "true" ]; then echo procps; fi) \
 && rm -rf /var/lib/apt/lists/*
RUN ln -snf "/usr/share/zoneinfo/${TZ}" /etc/localtime && echo "${TZ}" > /etc/timezone

COPY --from=build /root/.dotnet /root/.dotnet
ENV PATH="/root/.dotnet/tools:${PATH}"

COPY --from=build /publish .

# The assembly name differs per image, and the exec form of ENTRYPOINT does not
# expand variables. `exec` hands PID 1 over to dotnet so it still receives the
# stop signals Docker Swarm sends.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet $ENTRY_ASSEMBLY"]
