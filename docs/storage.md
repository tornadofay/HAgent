# Storage model

HAgent storage is an internal persistence boundary. The selected backend stores HAgent-owned configuration and runtime data only. HAgent database storage must never be used as a connection gateway to the host application's business database.

## Storage backends

The host selects one HAgent storage backend:

- `File` — application-specific files beneath the host executable directory, under `HAgentData`.
- `SqlServer` — an HAgent-owned SQL Server database.
- `MySql` — an HAgent-owned MySQL database.

The database name defaults from the host application name using the pattern `<application-name>-ai`, for example `nap-ai` or `hworld-ai`.

## Configuration

`HAgentStorageOptions` contains the storage backend and non-secret connection metadata. Server name, username, database name, and application name are configuration values. Database passwords are part of the runtime connection configuration and should not be written into ordinary HAgent configuration records.

The configured backend is intended to become the backing store for HAgent's internal repositories: global settings, providers, models, execution targets, discovery metadata, capability evidence/overrides, quota/rate/capacity state, agents, tools, memory, conversations, skills, knowledge/wiki, learning candidates/review state, permissions, runtime metadata, execution audit data, configuration-portability metadata, and future HAgent-owned records.

SQL Server and MySQL retain independent connection profiles. Switching the selected backend does not overwrite the other backend's server, port, username, or connection secret.

The database name is derived by HAgent from the host application identity and is not an editable storage setting.

## Provider credentials

Provider API keys are intentionally stored as part of the persisted provider configuration rather than through a separate secret-reference subsystem.

A provider record conceptually contains:

```text
Provider
    Id
    Name
    Kind
    BaseUrl
    ApiKey
    Enabled
    ...discovery/configuration fields...
```

API keys must be **encrypted at rest** before being persisted to the database or configuration file. HAgent should use one simple, documented encryption mechanism for stored credentials. The implementation must not require a separate vault, external secret service, secret-reference graph, or centralized secret-provider architecture.

The same persisted provider configuration is therefore usable by all HAgent processes that are intentionally connected to the same HAgent database. This supports a normal network deployment in which multiple machines consume the same provider/model configuration without requiring every machine to configure the provider again.

Decrypted API keys are used only at the provider execution boundary and must remain excluded from normal diagnostics, audit records, planner diagnostics, and other non-secret output.

Credential replacement/revocation is configuration management: updating or removing the stored API key is sufficient to stop future use of the old credential once the active configuration snapshot is refreshed.

## File backend

The File backend uses an application-specific root beneath the executable directory:

```text
HAgentData/
  configuration/
    general/
    providers/
    models/
    agents/
    tools/
    skills/
    knowledge/
    permissions/
  memory/
  conversations/
  wiki/
  learning/
  runtime/
  cache/
  logs/
  audit/
  export/
```

The layout is created on demand. Existing individual file stores can continue to use focused repository files while the common storage configuration establishes one consistent application-local root.

## Database backend lifecycle

SQL Server and MySQL providers receive server credentials at runtime, connect to the server, derive the HAgent database name, create the database when it does not exist, then create or upgrade only HAgent-owned tables.

Database schema changes are versioned through `HAgentSchemaInfo`. Bootstrap establishes the base HAgent-owned objects and a baseline schema version, then reads the persisted version and applies ordered provider-specific migrations until the current version is reached. A schema version is advanced only after its migration succeeds. Unknown future versions are rejected rather than silently skipped.

The MySQL bootstrap executes schema statements and migrations as separate commands rather than depending on multi-statement execution. This keeps the bootstrap compatible with MariaDB deployments as well as MySQL Connector implementations.

The relational schema must evolve to persist the new configuration model introduced by capability-aware execution and persistent cognition. At minimum this includes global settings, normalized model/execution-target records, discovery/capability evidence, operational limits and state, new agent selection policies, resource relationships, and configuration-portability metadata. Provider-specific migrations remain separate where SQL syntax differs.

All migrations operate only on HAgent-owned tables and indexes.

Initial internal schema areas include providers, models, execution targets, agents, tools, memory entries, conversations, skills, knowledge/wiki documents and chunks, execution audits, settings, permissions, and schema metadata. Additional HAgent-owned tables are introduced through ordered migrations.

Conversation snapshots are persisted through `IConversationStore`. File storage keeps one JSON file per session; SQL Server and MySQL store the serialized message list in the HAgent-owned `HAgentConversations` table. Session identity and agent identity remain part of the persisted snapshot.

## Execution audit persistence

`IExecutionAuditStore` persists secret-safe `AgentExecutionAuditRecord` metadata only. The File backend stores records under `HAgentData/audit/executions.jsonl`; SQL Server and MySQL use the HAgent-owned `HAgentExecutionAudits` table.

`DefaultAgentRuntime` accepts an optional audit store. When configured, every terminal execution result—success, failure, timeout, or cancellation—is projected and appended automatically. Audit persistence is non-fatal: an audit-store failure never changes the primary execution outcome and terminal audit persistence does not use the caller cancellation token.

`ExecutionAuditOptions` makes automatic capture and retention explicit. Capture remains enabled by default when an audit store is supplied, with a default retention limit of 5,000 records and a configurable maximum of 1,000,000 records. Older audit metadata is removed after successful append.

## Knowledge, skills, memory, and learning persistence

Persisted definitions are separated from runtime state:

```text
Skill definition/version       reusable resource
Wiki/knowledge resource       reusable managed information
Memory record                  scoped experience/state
Learning candidate             proposed change awaiting policy/review
Capability assignment          profile-level resource access
Runtime capability override    live instance override
```

A candidate must retain provenance, source execution/runtime identity, proposed scope, and evidence/confidence when available. Approval or automatic policy promotion writes through the appropriate target repository; rejection does not mutate the target.

Learning records and knowledge/skill management data must be bounded and secret-safe. The physical store may be shared by all runtime instances, but logical ownership/scope remains explicit.

## Runtime state

Runtime agent instances, workspaces, and other live collaboration state are not configuration by default. A host may keep them in memory or persist them when recovery, collaboration, audit, or multi-process visibility requires it.

When persisted, runtime records must distinguish the host instance, user/session, workspace, agent profile ID, and runtime instance ID. Persisted runtime capability overrides remain runtime metadata and never silently modify the profile.

## Configuration export and import

Configuration portability is a first-class HAgent capability. A configuration package represents HAgent-owned configuration independently from the selected storage backend so a configuration can be moved between File, SQL Server, and MySQL deployments.

The export/import contract should cover, as applicable:

```text
General/system settings
Providers and provider configuration
Models and discovered execution targets
Agents and their policies
Skills and skill versions
Knowledge / Wiki resources
Memory configuration and policy
Learning configuration/policy
Tools and tool definitions
Permissions and capability assignments
Other HAgent-owned configuration resources
```

Executable tool handlers, live runtime objects, active executions, synchronization primitives, transient HTTP state, and other process-local runtime objects are not serialized as portable configuration.

The package format must be versioned and must carry enough metadata for HAgent to validate compatibility before import. Import must have explicit conflict behavior for existing resources rather than silently overwriting unrelated configuration.

API keys may be included in an export when the user explicitly chooses a credential-bearing export. Such credentials remain encrypted inside the package and are protected by a basic export-package encryption/password mechanism. A normal export should omit credentials unless the user explicitly enables their inclusion.

Import of an encrypted credential-bearing package restores the provider API keys into the normal encrypted-at-rest provider configuration used by the selected storage backend.

Portability must not create a second configuration model: the package serializes the same authoritative HAgent configuration contracts that are persisted by the selected backend.

## Secrets and diagnostics

HAgent does not expose a separate secret-reference architecture. Provider credentials are ordinary provider configuration values with encryption-at-rest requirements and strict redaction rules.

API keys, database passwords, and export-package passwords must never be emitted in normal logs, execution audits, planner diagnostics, discovery evidence, exceptions, or UI diagnostic dumps.

## Tool handlers

Tool definitions may be persisted. Executable delegates/handlers are runtime registrations and are never serialized.

## Database isolation

The HAgent storage connection is an internal persistence connection. HAgent's own database name is application-specific and controlled by HAgent configuration. It must not be pointed at the host application's existing business database as a way to obtain access to host tables.

Structured host/application data contracts, where exposed elsewhere in HAgent, remain separate from the internal persistence provider and do not change this storage boundary.
