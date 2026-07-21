# GrillBot Backend

Monorepo for the whole GrillBot backend: the Discord bot host, the shared core
libraries and all microservices. It replaces the former
[`grillbot`](https://github.com/grillbot/grillbot),
[`GrillBot.Core`](https://github.com/GrillBot/GrillBot.Core) and
[`GrillBot.Services`](https://github.com/GrillBot/GrillBot.Services) repositories,
which are archived.

GrillBot is a Discord bot for fun and management of the VUT FIT Discord server.

## Layout

| Path | Contents |
|---|---|
| `src/Core/` | Shared libraries (`GrillBot.Core`, `.HealthCheck`, `.Metrics`, `.RabbitMQ.V2`, `.Redis`, `.Services`) |
| `src/Bot/` | Discord bot host (`GrillBot.App` + `Cache`, `Common`, `Data`, `Database`) |
| `src/Services/` | `GrillBot.Services.Common`, 12 .NET microservices and the Node/TypeScript `Graphics` service |
| `tests/` | MSTest projects for the core libraries |
| `docker/` | Dockerfiles and `deployables.json`, the manifest driving CI image builds |
| `build/` | Build/CI helper scripts |

The libraries in `src/Core/` are consumed through **project references**. They are
no longer published as NuGet packages, so no private feed, personal access token
or `read:packages` scope is needed to build this repository.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org) (only for `src/Services/Graphics`)
- [PostgreSQL](https://www.postgresql.org/) 13+ — one database per service
- [RabbitMQ](https://rabbitmq.com/) and [Redis](https://redis.io/)
- An Azure Storage account (or emulator) for the bot
- Docker, if you want to build images locally

On Debian-based Linux the runtime also needs `tzdata` and `libc6-dev`. Other
distributions are untested.

For development you will want Visual Studio 2022, JetBrains Rider or another
.NET-capable IDE, plus the [`dotnet-ef`](https://learn.microsoft.com/ef/core/cli/dotnet)
tool for code-first migrations.

## Building

```bash
dotnet restore GrillBot.Backend.slnx
dotnet build -c Release GrillBot.Backend.slnx
dotnet test tests/
```

The `Graphics` service is built separately:

```bash
cd src/Services/Graphics && npm ci && npm run build
```

### Central configuration

- `Directory.Build.props` sets the target framework, language version, nullability
  and runtime identifier for every project. Do not repeat those in a `.csproj`.
- `Directory.Packages.props` pins every third-party NuGet version
  ([Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)).
  A `PackageReference` in a `.csproj` must **not** carry a `Version` attribute;
  add or bump the version in `Directory.Packages.props` instead.

## Configuration

Development runs need `ASPNETCORE_ENVIRONMENT=Development` and a filled
`appsettings.Development.json` (git-ignored). Production is configured entirely
through environment variables supplied by the Docker Swarm stack.

Mandatory for the bot:

- `ConnectionStrings:Default` — main database
- `ConnectionStrings:Cache` — cache database
- `ConnectionStrings:StorageAccount` — Azure Storage account or emulator
- `Discord:Token` — Discord authentication token
- `Auth:OAuth2:ClientId`, `Auth:OAuth2:ClientSecret` — admin login
- `RabbitMQ:Hostname`, `RabbitMQ:Username`, `RabbitMQ:Password`

Recommended:

- `Discord:Logging:GuildId`, `Discord:Logging:ChannelId` — error notifications
- `Birthday:Notifications:GuildId`, `Birthday:Notifications:ChannelId`

Services take `ConnectionStrings:Default`, the `RabbitMQ:*` block and, where
applicable, `Redis:Endpoint`/`Redis:Password` and `Services:<Name>:Api`.

When running the bot in Docker, bind `/GrillBotData` as a volume.

## Deployment

`docker/deployables.json` lists every deployable unit. Its key is used as the
container image tag, the Docker Swarm service name and the health-check path, all
at once — for example `audit_log_service` becomes
`ghcr.io/grillbot/grillbot.services:audit_log_service`, Swarm service
`grillbot_audit_log_service`, health endpoint
`https://health.grillbot.eu/audit_log_service`. The bot is the exception and keeps
`ghcr.io/grillbot/grillbot:latest`.

`.github/workflows/ci.yml` derives both its change-detection filters and its
build matrix from that manifest. The `shares` field on each entry records the
dependency edges that project references create — a change under `src/Core/`
rebuilds every .NET image, a change in `GrillBot.Services.Common` rebuilds every
.NET service — so only the affected images are built, pushed, deployed via
`deploy-grillbot.sh <name>` over SSH, and health-checked.

To add a new service: create the project under `src/Services/`, add it to
`GrillBot.Backend.slnx`, and add an entry to `docker/deployables.json`. The
workflow needs no changes.

## Licence

GrillBot is licensed as All Rights Reserved. The source code is available for
reading and contribution. Owner consent is required for use in a production
environment.
