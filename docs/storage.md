# Storage model

HAgent storage is an internal persistence boundary. The selected backend stores HAgent-owned configuration and runtime data only. HAgent database storage must never be used as a connection gateway to the host application's business database.

## Storage backends

The host selects one HAgent storage backend:

- `File` — application-specific files beneath the host executable directory, under `HAgentData`.
- `SqlServer` — an HAgent-owned SQL Server database.
- `MySql` — an HAgent-owned MySQL database.

The database name defaults from the host application name using the pattern `<application-name>-ai`, for example `nap-ai` or `hworld-ai`.

## Configuration

`HAgentStorageOptions` contains the storage backend and non-secret connection metadata. Server name, username, database name, and application name are configuration values. Database passwords are not part of ordinary persisted configuration and remain in the secret/runtime connection boundary.

The configured backend is intended to become the backing store for HAgent's internal repositories: providers, agents, tools, memory, conversations, skills, wiki/content, learning candidates/review state, capability assignments/overrides, runtime metadata, execution audit data, and future HAgent-owned records.

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
  learning/
  runtime/
  cache/
  logs/
  audit/
```

The layout is created on demand. Existing individual file stores can continue to use focused repository files while the common storage configuration establishes one consistent application-local root.

## Database backend lifecycle

SQL Server and MySQL providers receive server credentials at runtime, connect to the server, derive the HAgent database name, create the database when it does not exist, then create or upgrade only HAgent-owned tables.

Database schema changes are versioned through `HAgentSchemaInfo`. Bootstrap establishes the base HAgent-owned objects and a baseline schema version, then reads the persisted version and applies ordered provider-specific migrations until the current version is reached. A schema version is advanced only after its migration succeeds. Unknown future versions are rejected rather than silently skipped.

The MySQL bootstrap executes schema statements and migrations as separate commands rather than depending on multi-statement execution. This keeps the bootstrap compatible with MariaDB deployments as well as MySQL Connector implementations.

Current relational schema versions are:

- SQL Server: version `3`; v1→v2 adds idempotent indexes for bounded memory and conversation retrieval, and v2→v3 adds execution-audit persistence.
- MySQL: version `4`; v1→v2 preserves the legacy `HAgentTools.ToolType` compatibility migration, v2→v3 adds idempotent indexes for bounded memory and conversation retrieval, and v3→v4 adds execution-audit persistence.

Phase 0.11 will add later migrations as required for learning candidates/review state, knowledge/skill relationships and versions, capability assignments/overrides, and extensible memory-type policy. These migrations remain HAgent-owned and provider-specific where SQL syntax differs.

All migrations operate only on HAgent-owned tables and indexes.

Initial internal schema areas include providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, execution audits, and schema metadata. Additional HAgent-owned tables are introduced through ordered migrations.

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

## Audit and observability

`AgentExecutionAuditRecord` is the provider-neutral, payload-free projection used for persistent execution audit metadata. Its correlation ID is distinct from execution ID and tool-call IDs and provides a stable runtime anchor across a terminal execution.

`IExecutionAuditStore` is intentionally append/search oriented. Search is bounded and may target an execution ID, correlation ID, or agent ID.

## Secrets

`ISecretStore` owns credentials/secrets. Secrets must not be stored in ordinary provider, agent, tool, learning, or storage-option records and must not appear in normal diagnostics.

`HAgent.Storage.File` currently keeps secrets separate and protects local secrets with Windows DPAPI `CurrentUser`.

## Tool handlers

Tool definitions may be persisted. Executable delegates/handlers are runtime registrations and are never serialized.

## Database isolation

The HAgent storage connection is an internal persistence connection. HAgent's own database name is application-specific and controlled by HAgent configuration. It must not be pointed at the host application's existing business database as a way to obtain access to host tables.

Structured host/application data contracts, where exposed elsewhere in HAgent, remain separate from the internal persistence provider and do not change this storage boundary.
