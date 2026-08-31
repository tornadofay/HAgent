# Storage model

HAgent storage is an internal persistence boundary. The selected backend stores HAgent-owned configuration and runtime data only. HAgent database storage must never be used as a connection gateway to the host application's business database.

## Storage backends

The host selects one HAgent storage backend:

- `File` — application-specific files beneath the host executable directory, under `HAgentData`.
- `SqlServer` — an HAgent-owned SQL Server database.
- `MySql` — an HAgent-owned MySQL database.

The database name defaults from the host application name using the pattern `<application-name>-ai`, for example `nap-ai` or `hworld-ai`.

The host application name is the storage identity shared by the configuration UI and runtime. Both derive the default application identity from the running host process so they resolve the same `HAgentData` root.

## Configuration

`HAgentStorageOptions` contains the storage backend and non-secret connection metadata. Server name, username, database name, and application name are configuration values. Database passwords are not part of ordinary persisted configuration and remain in the secret/runtime connection boundary.

The configured backend is intended to become the backing store for HAgent's internal repositories: providers, agents, tools, memory, conversations, skills, wiki/content, runtime metadata, and future internal data introduced by HAgent.

SQL Server and MySQL retain independent connection profiles. Switching the selected backend does not overwrite the other backend's server, port, username, or secret identity.

The database name is derived by HAgent from the host application identity and is not an editable storage setting.

## File backend

The File backend uses an application-specific root beneath the executable directory:

```text
HAgentData/
  configuration/
    providers/
    agents/
    tools/
    skills/
  memory/
  conversations/
  wiki/
  runtime/
  cache/
  logs/
```

The layout is created on demand. Existing individual file stores can continue to use their focused repository files while the common storage configuration establishes one consistent application-local root.

## Database backend lifecycle

SQL Server and MySQL providers receive server credentials at runtime, connect to the server, derive the HAgent database name, create the database when it does not exist, then create or upgrade only HAgent-owned tables.

Database schema changes are versioned through `HAgentSchemaInfo`. Bootstrap establishes the base HAgent-owned objects and a baseline schema version, then reads the persisted version and applies ordered provider-specific migrations until the current version is reached. A schema version is advanced only after its migration succeeds. Unknown future versions are rejected rather than silently skipped.

The MySQL bootstrap executes schema statements and migrations as separate commands rather than depending on multi-statement execution. This keeps the bootstrap compatible with MariaDB deployments as well as MySQL Connector implementations.

Current relational schema versions are:

- SQL Server: version `2`; v1→v2 adds idempotent indexes for bounded memory and conversation retrieval.
- MySQL: version `3`; v1→v2 preserves the legacy `HAgentTools.ToolType` compatibility migration, and v2→v3 adds idempotent indexes for bounded memory and conversation retrieval.

All migrations operate only on HAgent-owned tables and indexes.

Initial internal schema areas include providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and schema metadata. Additional HAgent-owned tables may be added through later migrations.

Conversation snapshots are persisted through `IConversationStore`. File storage keeps one JSON file per session; SQL Server and MySQL store the serialized message list in the HAgent-owned `HAgentConversations` table. Session identity and agent identity remain part of the persisted snapshot.

## Connection testing

`SqlServerHAgentStorageBootstrapper.TestConnectionAsync` and `MySqlHAgentStorageBootstrapper.TestConnectionAsync` provide non-destructive endpoint and credential validation. They open a connection without selecting an HAgent database and do not create databases, create tables, run migrations, or modify persisted HAgent data.

The WinForms AI Settings navigation exposes this capability as **Storage Test**. It loads the saved selected backend profile, retrieves the password only for the connection attempt, and reports success or the underlying connection failure without exposing the password.

A connection test succeeds only when the database server accepts the supplied credentials. Successful connectivity does not imply that the HAgent database exists or that the account has permission to create it; those responsibilities remain with the separate storage bootstrap operation.

## Live backend switching

Storage configuration changes that affect the active backend can be applied without restarting the host when the host supports live storage rebinding. HAgent does not mutate an existing store while it is in use. Instead, the host creates a new configured store set and routes subsequent work to it.

In-flight operations continue using the store/client snapshots with which they started. New operations use the newly selected backend. This preserves the runtime snapshot invariant while allowing File, SQL Server, and MySQL to be selected during one process lifetime.

The WinForms Example closes any configuration surface that still owns stores for the previous backend, rebuilds the configured HAgent stores, reloads the agent list, and continues on the new backend. An unavailable new backend does not terminate the host; the configuration repair path remains available.

## Runtime state

Runtime agent instances, workspaces, and other live collaboration state are not configuration by default. A host may keep them in memory or persist them when recovery, collaboration, audit, or multi-process visibility requires it.

When persisted, runtime records must distinguish the host instance, user/session, workspace, agent profile ID, and runtime instance ID.

## Secrets

`ISecretStore` owns credentials/secrets. Secrets must not be stored in ordinary provider, agent, tool, or storage-option records and must not appear in normal diagnostics.

`HAgent.Storage.File` currently keeps secrets separate and protects local secrets with Windows DPAPI `CurrentUser`.

## Tool handlers

Tool definitions may be persisted. Executable delegates/handlers are runtime registrations and are never serialized.

## Database isolation

The HAgent storage connection is an internal persistence connection. HAgent's own database name is application-specific and controlled by HAgent configuration. It must not be pointed at the host application's existing business database as a way to obtain access to host tables.

Structured host/application data contracts, where exposed elsewhere in HAgent, remain separate from the internal persistence provider and do not change this storage boundary.
