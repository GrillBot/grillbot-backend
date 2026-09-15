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
| `src/Core/` | Shared libraries (`GrillBot.Core`, `.AsyncMessaging`, `.HealthCheck`, `.Metrics`, `.Redis`, `.Services`) |
| `src/Contracts/` | `GrillBot.Contracts` — the integration events, requests and responses exchanged between services, one folder per bounded context |
| `src/Bot/` | Discord bot host (`GrillBot.App` + `Cache`, `Common`, `Data`, `Database`) |
| `src/Services/` | `GrillBot.Services.Common`, 12 .NET microservices and the Node/TypeScript `Graphics` service |
| `tests/` | MSTest projects for the core libraries |
| `docker/` | Dockerfiles and `deployables.json`, the manifest driving CI image builds |
| `build/` | Build/CI helper scripts |

The libraries in `src/Core/` are consumed through **project references**. They are
no longer published as NuGet packages, so no private feed, personal access token
or `read:packages` scope is needed to build this repository.

Every contract has **exactly one** definition, in `src/Contracts/`. A service must
not keep a private copy of a type it exchanges with another service — that is what
the old per-repo split forced and what the `CS0436` suppressions used to hide.
`GrillBot.Core.Services` holds only the Refit client interfaces; the payloads they
carry come from `GrillBot.Contracts`. Contracts carry data, validation attributes
and self-contained invariants; anything that needs service state (options, the
database) belongs in a `ModelValidator<T>` in the owning service, which
`ModelValidationFilter` resolves and runs.

## Async messaging

Services talk to each other asynchronously through **WolverineFx over RabbitMQ**
(`src/Core/GrillBot.Core.AsyncMessaging`). The topology is:

- one **direct exchange per bounded context** — `points`, `audit-log`, `emote`,
  `unverify`, `grillbot`, …;
- one **queue per deployable**, named after its key in `docker/deployables.json`
  (`points_service`, `bot`, …);
- a **routing key per message**, bound from the owning exchange to the queue of
  the service that handles it.

`Topology/MessageTopology.cs` is the single table mapping a message type to its
exchange and routing key, built from the `MessagingConstants` class of each
bounded context. Contracts themselves carry no transport: an integration event is
a plain `record` that knows nothing about queues, and a test asserts that every
event in `GrillBot.Contracts` has a route.

Writing a handler needs no registration — Wolverine discovers any public class
whose name ends in `Handler` with a `Handle`/`HandleAsync` method whose first
argument is the message. Publishing is `IMessageBus.PublishAsync(payload)`, or
`PublishAsync(payload, currentUser)` to forward the caller's `Authorization`
header so the handler can act on their behalf.

A handler that throws gets the retry cooldowns from `AsyncMessaging:RetryCooldowns`,
then a Discord error notification and an audit log entry, then Wolverine's error
queue. Throw `TransientMessageException` when a retry is expected and the
notification should be skipped.

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

Configuration is split by sensitivity:

- **Non-sensitive** settings (URLs, logging levels, feature toggles) live in the
  committed `appsettings.json` and/or environment variables.
- **Sensitive** settings (connection strings, API keys, Discord/RabbitMQ
  credentials, JWT signing material, …) are supplied out-of-band and never
  committed.

**Development** runs need `ASPNETCORE_ENVIRONMENT=Development` and a filled
`appsettings.Development.json` (git-ignored — `appsettings.*.json` is excluded).
Put local sensitive values there; it is loaded automatically after
`appsettings.json` and overrides it.

**Production** reads sensitive values from Docker (Swarm) **secrets** mounted at
`/run/secrets`. Every host (the bot and all services) loads them via the shared
`IConfigurationBuilder.AddDockerSecrets()` extension in `GrillBot.Core`. Loading
is optional — if `/run/secrets` is absent (e.g. local runs) it is a no-op. Two
secret file layouts are supported, and you may name the keys whatever you like:

- **Key-per-file** — the file name is the configuration key and the raw file
  content is its value (e.g. a secret named `ConnectionStrings__Default`).
- **JSON files** (`*.json`) — appsettings-shaped documents merged over the base
  configuration.

Precedence, lowest to highest:
`appsettings.json` < `appsettings.{Environment}.json` < `/run/secrets` <
environment variables < command-line arguments. `AddDockerSecrets()` inserts its
sources below the host's environment-variable provider rather than appending
them, so the rest of the ASP.NET Core defaults are left exactly as they are. A
startup log line lists the configuration sources that were actually loaded, so
you can confirm secrets were picked up.

> **Separate configuration keys with `__` in environment variables, never `:`.**
> The keys below are written in canonical `Section:Key` notation, but as an
> environment variable the separator must be a double underscore —
> `ConnectionStrings__Default`, `RabbitMQ__Hostname`, `Services__AuditLog__Api`.
> `Section:Key` happens to work on Windows and when a process is `exec`'d
> directly, but a name containing a colon is not a valid shell identifier, so any
> shell in between (`sh -c …`, an entrypoint wrapper, `su`, some CI runners)
> drops the variable outright and the setting silently reads as empty. Docker
> secret file names follow the same rule.

### Startup validation

Sensitive keys ship as **empty placeholders** in the committed `appsettings.json`,
so a missing override does not read as missing — it reads as an empty string. Left
unchecked, a host would start on those placeholders, report healthy and only fail
much later, on the first Discord login or database call.

Every host therefore validates the configuration it cannot run without **while it
starts**, using the standard options pipeline (`AddValidatedOptions(…)` in
`GrillBot.Core`, which binds the section, applies its `[Required]`/`IValidateOptions`
rules and calls `ValidateOnStart()`). A missing value stops the host with an
`OptionsValidationException` naming every key that was left empty, all at once.

What is checked is derived from what the application declares: a host that needs a
database or a Redis server ships the (empty) section, one that does not — for example
ImageProcessingService, which has neither — ships nothing and is left alone.

| Section | Validated | Where |
|---|---|---|
| `RabbitMQ` | `Hostname`, `Username`, `Password` | every host |
| `AsyncMessaging` | `QueueName` is present and is a key in `docker/deployables.json`; the listener counts, timeouts and retry cooldowns are positive | every host |
| `ConnectionStrings` | `Default` (`BotToken` stays optional — an empty one deliberately turns a service's Discord client off) | every host declaring the section |
| `Redis` | `Endpoint` (`Password` optional) | every host declaring the section |
| `Discord` | `Token` | the bot |
| `Auth:OAuth2` | `ClientId`, `ClientSecret` | the bot |

Mandatory for the bot:

- `ConnectionStrings:Default` — main database
- `Discord:Token` — Discord authentication token
- `Auth:OAuth2:ClientId`, `Auth:OAuth2:ClientSecret` — admin login and JWT signing
- `RabbitMQ:Hostname`, `RabbitMQ:Username`, `RabbitMQ:Password`
- `Redis:Endpoint` — distributed cache

Recommended:

- `Discord:Logging:GuildId`, `Discord:Logging:ChannelId` — error notifications
- `Birthday:Notifications:GuildId`, `Birthday:Notifications:ChannelId`

Services take `ConnectionStrings:Default`, the `RabbitMQ:*` block and, where
applicable, `Redis:Endpoint`/`Redis:Password` and `Services:<Name>:Api`.

Broker credentials stay in `RabbitMQ:*`. The Wolverine tuning lives next to it in
the `AsyncMessaging` block of each `appsettings.json` — queue name, listener count,
retry cooldowns, dead-letter expiration. `AsyncMessaging:QueueName` must equal the
application's key in `docker/deployables.json`; startup fails fast if it does not
(checked both while Wolverine is configured and by the startup validation above,
through the same validator).

When running the bot in Docker, bind `/GrillBotData` as a volume.

## Local development with Docker Compose

`compose/` holds two standalone Docker Compose stacks (plain `docker compose`, not
Swarm) for running parts of the backend on your machine:

| File | Project | Contents |
|---|---|---|
| `compose/infrastructure.yml` | `grillbot-infra` | RabbitMQ (`5672`, UI `15672`), `redis-main` (`6379`), `redis-ephemeral` (`6380`) |
| `compose/database.yml` | `grillbot-db` | Optional PostgreSQL 18 (`5432`) with the `grillbot` role and all databases |
| `compose/grillbot.yml` | `grillbot` | The 13 microservices, on the development server's host ports `3005`–`3019` |

The infrastructure stack creates the `grillbot-dev` network, which the other two
stacks join, so start it first.

```bash
cp compose/.env.example compose/.env
docker compose -f compose/infrastructure.yml up -d
docker compose -f compose/database.yml up -d --wait
docker compose -f compose/grillbot.yml up -d
```

- **Database:** `compose/database.yml` is only for developers without a PostgreSQL
  server. On its first start (empty volume) `compose/database/init/` creates the
  `DB_USERNAME`/`DB_PASSWORD` role — not a superuser — and one database per service
  plus `GrillBotDev`, all owned by that role. The applications create their tables
  through EF Core migrations on startup. `.env.example` points `DB_HOST` at it
  (`postgres`); from the IDE use `localhost:5432`. With your own server, skip that
  stack and set `DB_HOST` to it instead. `down -v` wipes the data.

- **Run a subset** by naming services:
  `docker compose -f compose/grillbot.yml up -d points_service emote_service`.
- **Health:** every container is health-checked against its `/health` endpoint,
  so `docker compose ... ps` shows `healthy`/`unhealthy` and `up -d --wait` blocks
  until the started services are healthy. Services do not `depends_on` each other,
  so starting one never starts another.
- **Images** are pulled from GHCR when missing. `up -d --build <service>` builds
  from your working tree instead; `pull` returns to the published image.
- **Bot in the IDE, services in containers:** start both stacks. The bot's
  `appsettings.json` already points at `127.0.0.1:30xx`; in your
  `appsettings.Development.json` use `localhost` for `RabbitMQ:Hostname` and
  `localhost:6379` for `Redis:Endpoint`.
- **One service in the IDE:** `stop` its container, run it from the IDE against
  `localhost` for RabbitMQ and Redis. If a container calls it (`GRAPHICS_API`,
  `USER_MEASURES_API`, `UNVERIFY_API`), point that variable at
  `http://host.docker.internal:<port>/` and re-run `up -d` for the caller.

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
dependency edges that project references create — a change under `src/Core/` or
`src/Contracts/` rebuilds every .NET image, a change in `GrillBot.Services.Common`
rebuilds every .NET service — so only the affected images are built, pushed, deployed via
`deploy-grillbot.sh <name>` over SSH, and health-checked.

To add a new service: create the project under `src/Services/`, add it to
`GrillBot.Backend.slnx`, and add an entry to `docker/deployables.json`. The
workflow needs no changes.

## Licence

GrillBot is licensed as All Rights Reserved. The source code is available for
reading and contribution. Owner consent is required for use in a production
environment.
