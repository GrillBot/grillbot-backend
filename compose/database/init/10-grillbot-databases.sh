#!/bin/sh
# Run by the postgres image on the first start with an empty data volume (files
# in /docker-entrypoint-initdb.d). Creates the application role and one database
# per GrillBot deployable, owned by that role - the layout of the development
# server. Schemas are created later by each application's EF Core migrations.
#
# Idempotent, so it can also be re-run by hand against a running container:
#   docker compose -f compose/database.yml exec postgres sh /docker-entrypoint-initdb.d/10-grillbot-databases.sh
#
# Keep the list in sync with the ConnectionStrings__Default databases in
# compose/grillbot.yml (and GrillBotDev for the bot).

psql -v ON_ERROR_STOP=1 -X -q \
    --username "${POSTGRES_USER:-postgres}" --dbname postgres \
    -v app_user="${GRILLBOT_DB_USERNAME:?GRILLBOT_DB_USERNAME is not set}" \
    -v app_password="${GRILLBOT_DB_PASSWORD:?GRILLBOT_DB_PASSWORD is not set}" <<'EOSQL'
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L', :'app_user', :'app_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = :'app_user')
\gexec

SELECT format('CREATE DATABASE %I OWNER %I', db, :'app_user')
FROM unnest(ARRAY[
    'GrillBotDev',
    'AuditLogService',
    'EmoteService',
    'InviteService',
    'MessageService',
    'PointsService',
    'RemindService',
    'RubbergodService',
    'SearchingService',
    'UnverifyService',
    'UserManagementService',
    'UserMeasuresService'
]) AS db
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = db)
\gexec

\echo
SELECT format('%s databases owned by %s', count(*), :'app_user') AS grillbot_init
FROM pg_database WHERE datdba = (SELECT oid FROM pg_roles WHERE rolname = :'app_user');
EOSQL
