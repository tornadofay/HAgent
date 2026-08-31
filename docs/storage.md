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

SQL Server and MySQL providers receive server credentials at runtime, connect to the server, derive or use the configured HAgent database name, create the database when it does not exist, then create or upgrade only HAgent-owned tables.

Database schema changes are versioned through an HAgent schema-info record. Future upgrades should use ordered migrations and must never silently recreate or modify unrelated application tables.

Initial internal schema areas include providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and schema metadata. Additional HAgent-owned tables may be added through later migrations.

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
