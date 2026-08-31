# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.8 Data Access + Authorization + Internal Storage

### Objective
Provide bounded structured data contracts while making HAgent's persistence an explicitly HAgent-owned storage boundary. HAgent must never use its internal storage connection as an implicit gateway to a host application's business database.

### Current slices

- [x] Application-owned structured-query contract and authoritative field schema.
- [x] Data permissions separated into discovery, projection/query, export, and write operations.
- [x] Host authorization callback contract.
- [x] Query limits, cancellation, timeout, and resource budgets.
- [x] HAgent storage backend configuration for File, SQL Server, and MySQL.
- [x] Application-specific File storage layout.
- [x] SQL Server HAgent database creation and schema bootstrap foundation.
- [x] MySQL HAgent database creation and schema bootstrap foundation.
- [x] Example agent/provider/prompt loading follows the selected internal storage backend.
- [x] Storage changes that affect the active runtime can be applied live when the host supports storage rebinding.
- [x] Persistent-session Example test uses the selected HAgent AI storage backend instead of hardcoding File storage for agent lookup.
- [x] Storage backend persistence uses explicit enum names and verifies the saved backend immediately.
- [x] SQL Server internal memory store and Example routing for the selected backend.
- [x] MySQL internal memory store and Example routing for the selected backend.
- [x] Task/event memory Example test uses the selected internal memory backend for File, SQL Server, and MySQL.
- [x] Standalone memory persistence Example test uses the selected internal memory backend for File, SQL Server, and MySQL.
- [x] Conversation persistence repositories exist for File, SQL Server, and MySQL.
- [x] Persistent-session Example test verifies the selected conversation backend and concrete persistence location.
- [x] SQL Server schema upgrades are ordered and advance only after a migration succeeds.
- [x] MySQL schema upgrades are ordered and advance only after a migration succeeds.
- [ ] Wire all remaining internal repositories to the selected storage backend.
- [x] HAgent internal storage connection testing APIs and WinForms UI.
- [x] Bounded HAgent internal inventory read-tool foundation for providers, agents, and persisted tool definitions.
- [x] Bounded HAgent internal memory read-tool foundation with explicit scope/owner filtering and sensitive metadata redaction.
- [x] Bounded HAgent internal conversation read-tool foundation with explicit session targeting and agent-identity isolation.
- [x] Agent execution correlation ID and audit-safe execution projection foundation.
- [x] Bounded persistent execution audit store for File, SQL Server, and MySQL.
- [x] Bounded HAgent internal execution-audit read-tool foundation with agent isolation.
- [ ] Wire automatic terminal execution auditing and retention policy into the runtime.
- [ ] Read-only HAgent internal data tools and result/audit metadata before any writes.

### Independent database connection profiles

HAgent stores independent SQL Server and MySQL connection profiles so changing the selected backend does not overwrite or erase the other backend's server, port, username, or secret identity. Switching the Storage type swaps the visible profile in the settings UI.

Database passwords use backend-specific secret IDs. Password values are never displayed in the settings UI and never stored in ordinary storage configuration.

The editable database-name field was removed. HAgent always derives its internal database name from the host application identity using the `<application>-ai` naming rule; database names are not user-selectable in the Storage UI.

Legacy shared connection settings remain readable only as a compatibility migration path into the appropriate database profile.

The settings UI persists a valid database profile when the user switches away from that backend, including a newly entered password. An incomplete database profile does not block switching; it remains unconfigured until valid connection details are supplied and saved.

A saved database profile is treated as durable only after the configuration file has been written successfully and reloaded from disk. The selected SQL Server/MySQL profile remains the authoritative runtime source for server, port, username, and secret identity; legacy shared fields are not used for new runtime resolution.

### Internal memory backend parity

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` implement `IMemoryStore` against the HAgent-owned `HAgentMemoryEntries` table. Entries retain scope, kind, owner, task, content, metadata, and timestamps. Core filtering and bounded result counts are performed by each relational provider, while the existing provider-independent memory scoring behavior is preserved after retrieval.

The Example automatic-memory, episodic-memory, task/event-memory, and standalone memory persistence verification paths obtain their memory store from the selected HAgent storage backend. The task/event and standalone memory tests report the validated configured backend and persistence location, making File/SQL Server/MySQL routing visible during manual verification.

### Conversation storage parity

`IConversationStore` is implemented by the File, SQL Server, and MySQL storage providers. File sessions remain one JSON file per session; SQL Server and MySQL persist serialized message lists in the HAgent-owned `HAgentConversations` table while preserving session ID, agent ID, and timestamps.

The Example persistent-session verification path obtains its conversation store from the selected HAgent storage backend. It reports the selected backend, concrete conversation-store implementation, and backend-specific persistence location, then verifies save, reopen, message retention, and deletion.

### Versioned schema migrations

The SQL Server and MySQL bootstrappers use `HAgentSchemaInfo` as the schema-version boundary. Bootstrap creates the base HAgent-owned tables, establishes a baseline schema version, reads the persisted version, applies ordered migrations until the provider's current version is reached, and advances the version only after each migration succeeds.

SQL Server is currently at schema version 3. Its v1-to-v2 migration adds idempotent indexes supporting bounded memory and conversation retrieval. Its v2-to-v3 migration creates the execution-audit table and supporting indexes.

MySQL is currently at schema version 4. Its v1-to-v2 migration preserves the legacy `HAgentTools.ToolType` compatibility upgrade, its v2-to-v3 migration adds idempotent indexes supporting bounded memory and conversation retrieval, and its v3-to-v4 migration creates the execution-audit table and supporting indexes. MySQL bootstrap and migrations execute statements separately so MariaDB deployments do not depend on multi-statement command execution.

Unknown future schema versions are rejected rather than silently downgraded or skipped. Migrations operate only on HAgent-owned objects.

### Configured storage backend resolution

The Example resolves its `IAiStore`, tool-definition store, memory store, conversation store, and execution-audit store from `HAgentStorageOptions` rather than hardcoding the File backend. File, SQL Server, and MySQL are distinct runtime storage choices.

The selected backend is used consistently for agent/provider loading, provider-system-prompt resolution, configuration display, client creation, automatic memory, episodic memory, task/event memory, standalone memory persistence, persistent conversation storage, and execution-audit persistence/inspection. This prevents the UI from displaying one backend's agents while runtime execution or persistence uses another backend.

The storage configuration file persists the backend as an explicit enum name (`File`, `SqlServer`, or `MySql`) rather than an opaque numeric value. Saving immediately re-reads the configuration and rejects the save if the selected backend was not persisted correctly.

SQL Server and MySQL resolution bootstraps the HAgent-owned database before creating the corresponding internal repositories. No host application database is used by this resolution path.

Storage settings are persisted immediately. When a host supports live rebinding, a successful active-backend change recreates the configured HAgent stores and refreshes subsequent work without restarting the application.

Live storage rebinding never mutates a store underneath in-flight work. Existing operations retain the stores/clients captured when they started; newly created operations resolve against the newly selected backend. Configuration surfaces owning the previous backend are closed before the new store path is rebuilt.

Database storage exposes an explicit TCP port with protocol defaults of 1433 for SQL Server and 3306 for MySQL. The selected port is persisted and used by both provider-specific connection builders.

The Example startup path does not terminate when the configured HAgent storage backend cannot be opened. It keeps the configuration surface available, reports the backend-unavailable state in the UI, and exposes the underlying exception through the HAgent message helper so storage settings can be corrected. Startup diagnostics report the non-secret backend target and full exception details without exposing the password.

The Example's Configuration action also has a recovery path that opens the Storage settings directly when the active backend cannot be opened. It therefore never requires a successful database connection merely to repair database settings.

The database bootstrappers consume the selected SQL Server/MySQL profile directly. They no longer read the legacy top-level connection fields during database creation or schema initialization.

### Tool execution correlation metadata

Tool execution context and results carry execution-local correlation metadata: correlation ID, agent ID, tool ID, model tool-call ID, start/completion timestamps, and derived duration. Validation, disabled-tool, and missing-tool failures use the same metadata shape so callers can correlate rejected attempts as well as successful executions. This is runtime metadata only; persistent audit storage remains a separate capability.

Agent executions now also receive an immutable `CorrelationId` at execution creation. This ID is distinct from provider-attempt and tool-call IDs and serves as the stable runtime-level correlation anchor for later audit and telemetry. The Example runtime verification requires this correlation ID and verifies execution timing remains monotonic.

`AgentExecutionAuditRecord` provides a secret-safe, payload-free projection of execution metadata for observability and audit persistence. It contains execution/correlation IDs, agent identity, model, selected provider identity, lifecycle timing, state, and classified failure metadata while excluding prompts, message contents, response text, secret values/IDs, raw connection strings, and raw exceptions.

`IExecutionAuditStore` persists these audit records through File, SQL Server, and MySQL implementations. Reads are explicitly bounded and can target execution ID, correlation ID, or agent ID. The relational implementations use the HAgent-owned `HAgentExecutionAudits` table; the File implementation uses an HAgent-owned audit JSONL file. Audit persistence remains explicit and does not automatically capture execution payloads.

`HAgentInternalExecutionAuditTool` exposes persisted audit metadata as a trusted read-only capability. Requests are bounded to 50 records, require an execution/correlation/agent target, and cannot use a model-supplied agent ID to redirect a request away from the requesting agent when an execution identity is present.

### Internal inventory read tool

`HAgentInternalInventoryTool` is the first HAgent-owned read-tool foundation. It reads only through `IAiStore` and `IToolStore`, returns provider/agent/tool inventory metadata without secrets, supports caller cancellation, and bounds each category to a default of 50 and a hard maximum of 100 items. It has no write operation and does not expose raw connection data, passwords, executable handlers, or arbitrary host application records.

The `HAgent.Example` application exposes an `Internal Inventory` verification tab. The manual check uses the currently selected HAgent storage backend, applies `maxItems = 1`, verifies category bounds and absence of sensitive metadata, and performs no writes.

### Internal memory read tool

`HAgentInternalMemoryTool` provides read-only bounded inspection of HAgent-owned memory for one explicit scope and owner. Optional kind, task, and text filters narrow the existing `IMemoryStore.SearchAsync` contract. Results are limited to 20 by default and 50 maximum, memory content is bounded, cancellation is propagated, and sensitive metadata keys are redacted.

The `HAgent.Example` application exposes an `Internal Memory` verification tab. The manual check creates temporary memory only as test setup, reads one explicit owner/scope, verifies another owner's entry is excluded, verifies sensitive metadata redaction, rejects an excessive result bound, and cleans up the temporary entries directly through the memory store.

### Internal conversation read tool

`HAgentInternalConversationTool` provides read-only bounded inspection of one HAgent-owned conversation identified by an explicit session ID. It uses `IConversationStore.LoadAsync` only, does not enumerate sessions, limits returned messages to 20 by default and 50 maximum, bounds each message content to 4000 characters, propagates cancellation, and rejects access when the stored conversation agent identity differs from the requesting agent identity. The Example Persistent Session verification uses this tool after reopening the persisted conversation and verifies that the stored message can be read through the trusted tool with the message bound applied.

### Storage foundation

`HAgent.Core` provides `HAgentStorageOptions` with `File`, `SqlServer`, and `MySql` backends, host application naming, application-specific database naming, database port, non-secret connection metadata, and independent database profiles. Database passwords remain outside this ordinary configuration model.

`HAgent.Storage.File` provides an application-specific `HAgentData` directory layout for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime state, cache, logs, and audit data.

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` provide HAgent-owned database bootstrappers. They connect to the configured server and port, create the derived HAgent database when absent, then create only HAgent-owned tables and schema metadata. The current schema includes providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, execution audits, and migration metadata.

The previously implemented SQL Server `IDataQuerySource` path against arbitrary host tables was removed because it violated the internal-storage boundary. The provider-neutral structured-query contract remains independent and must not be interpreted as permission to use a host application's business database.

### Live verification

The manual Example must verify the selected internal storage backend. For File storage it should verify the application-specific directory structure and internal repositories. For SQL Server/MySQL it should verify connection, database creation when absent, schema initialization, idempotent re-open, ordered schema migration from older HAgent versions, persistence through the configured repositories, execution-audit round trips, and safe refusal to operate against unrelated host application tables. Storage backend switching should also be verified live by switching File ↔ SQL Server ↔ MySQL without restarting, including failure recovery when the newly selected backend cannot be opened.

Database credentials must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- Implicit access to the host application's business database.
- Treating a provider connection as permission to inspect arbitrary host tables.
- Persisting database passwords as ordinary configuration.
- Treating UI discovery, provenance, or model instructions as authorization.

## Definition of done

0.8 is complete only after HAgent internal persistence is selectable and operational across the supported storage backends, schema upgrades are deterministic, live storage rebinding is safe, and the Example verifies that HAgent storage remains isolated from host application data.
