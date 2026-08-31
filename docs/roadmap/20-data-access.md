# Phase 0.8 — Data Access + Authorization + Internal Storage

## Goal
Provide bounded structured data contracts and establish HAgent-owned persistence across File, SQL Server, and MySQL backends without ever using HAgent storage as access to a host application's business database.

## Steps

1. [x] Application-owned structured-query contract and authoritative field schema.
2. [x] Separate data-operation permissions and request-specific host authorization contracts.
3. [x] Query/result limits, cancellation, timeout, and resource budgets.
4. [x] HAgent internal storage backend configuration for File, SQL Server, and MySQL.
5. [x] Application-specific File storage layout.
6. [x] SQL Server HAgent database creation and initial schema bootstrap.
7. [x] MySQL HAgent database creation and initial schema bootstrap.
8. [ ] Wire providers, agents, tools, memory, conversations, skills, wiki/content, and runtime repositories to the selected backend.
9. [x] Versioned schema migrations beyond the initial bootstrap version.
10. [ ] Read-only HAgent internal data tools, audit/correlation metadata, and live Example verification before any internal writes beyond repository persistence.

The first read-tool foundation is now present as `HAgentInternalInventoryTool`: bounded provider/agent/tool inventory metadata with cancellation and secret exclusion. The complete read-tool suite, persistent audit layer, and live Example verification remain open work under this step.

## Internal database naming

The default HAgent database name is derived from the host application name using `<application-name>-ai`, for example `nap-ai` or `hworld-ai`. The host can override the database name explicitly.

## File backend

File storage is application-specific and rooted beneath the host executable directory in `HAgentData`, with dedicated areas for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime, cache, and logs.

## Database backends

SQL Server and MySQL storage providers receive server name and username as persisted configuration metadata and a password through the secret/runtime boundary. They connect to the server, create the HAgent-owned database if it does not exist, and initialize only HAgent-owned tables. Schema version metadata supports deterministic future migrations.

The relational bootstrappers use `HAgentSchemaInfo` as the migration boundary. They establish a baseline schema version, read the persisted version, apply ordered provider-specific migrations until the current version is reached, and update the version only after each migration succeeds. Unknown future schema versions are rejected rather than silently skipped.

Current relational schema versions are SQL Server `2` and MySQL `3`. The current migrations add idempotent indexes for bounded memory/conversation retrieval; the MySQL v1→v2 step also preserves the previously implemented `HAgentTools.ToolType` compatibility migration for older databases.

The initial internal database schema contains provider, agent, tool, memory, conversation, skill, wiki document/chunk, and schema metadata tables. It must never inspect, alter, or query unrelated host application tables.

## Live Example

The Example storage verification will exercise File, SQL Server, and MySQL initialization where the corresponding backend is configured. It will verify database creation when absent, idempotent initialization when present, schema version reporting, persistence through the HAgent repositories, and strict separation from host application data. Live backend switching is expected to work without restarting when the host supports storage rebinding.

Connection values must never become persisted agent/tool configuration or normal logs.

## Boundaries

- No raw SQL from model input.
- No implicit access to the host application's business database.
- HAgent storage providers are internal persistence providers, not host database adapters.
- Database passwords remain in the secret/runtime boundary.
- UI discovery, object provenance, and model instructions do not grant database authorization.

## Exit criterion

A host can select an HAgent-owned storage backend, initialize or upgrade it deterministically, and use HAgent repositories against it without HAgent gaining access to the host application's business database.
