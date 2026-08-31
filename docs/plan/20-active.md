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
- [x] Storage changes that affect the active runtime are identified as restart-required.
- [x] Persistent-session Example test uses the selected HAgent AI storage backend instead of hardcoding File storage for agent lookup.
- [x] Storage backend persistence uses explicit enum names and verifies the saved backend immediately.
- [x] SQL Server internal memory store and Example routing for the selected backend.
- [x] MySQL internal memory store and Example routing for the selected backend.
- [x] Task/event memory Example test uses the selected internal memory backend for File, SQL Server, and MySQL.
- [x] Independent SQL Server/MySQL connection profiles with separate protected password secrets.
- [x] Derived HAgent database name only; database name is not editable in Storage UI.
- [x] Selected database profile is treated as authoritative by runtime storage resolution.
- [x] Persisted database profiles are validated after serialization/reload, and legacy shared settings migrate into the active profile when needed.
- [ ] Wire all remaining internal repositories to the selected storage backend.
- [ ] Versioned schema migrations beyond the initial bootstrap version.
- [ ] HAgent internal storage credentials/secret lifecycle and connection testing UI.
- [ ] Read-only HAgent internal data tools and result/audit metadata before any writes.

### Current slice: independent database connection profiles

HAgent stores independent SQL Server and MySQL connection profiles so changing the selected backend does not overwrite or erase the other backend's server, port, username, or secret identity. Switching the Storage type swaps the visible profile in the settings UI.

Database passwords use backend-specific secret IDs. Password values are never displayed in the settings UI and never stored in ordinary storage configuration.

The editable database-name field was removed. HAgent always derives its internal database name from the host application identity using the `<application>-ai` naming rule; database names are not user-selectable in the Storage UI.

Legacy shared connection settings remain available only for compatibility with older storage.json files. They are migrated into the active backend profile when that profile has not yet been configured; runtime resolution does not use the legacy fields as a source of truth.

The settings UI persists a valid database profile when the user switches away from that backend, including a newly entered password. An incomplete database profile does not block switching; it remains unconfigured until valid connection details are supplied and saved.

Database settings persistence now includes a serialize/reload verification. Saving fails rather than reporting success if the selected backend or its server, port, username, or secret identity does not survive persistence.

### Current slice: internal memory backend parity

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` implement `IMemoryStore` against the HAgent-owned `HAgentMemoryEntries` table. Entries retain scope, kind, owner, task, content, metadata, and timestamps. Core filtering and bounded result counts are performed by each relational provider, while the existing provider-independent memory scoring behavior is preserved after retrieval.

The Example automatic-memory, episodic-memory, and task/event-memory verification paths obtain their memory store from the selected HAgent storage backend. The task/event test also reports the validated configured backend in its result, making File/SQL Server/MySQL routing visible during manual verification.

The older standalone `[MEMORY]` Example tab still contains a historical File-only persistence path and has not yet been migrated to the configured memory resolver. It must not be used as proof of SQL Server/MySQL memory backend selection until that cleanup slice is complete.

### Configured storage backend resolution

The Example resolves its `IAiStore`, tool-definition store, and memory store from `HAgentStorageOptions` rather than hardcoding the File backend. File, SQL Server, and MySQL are distinct runtime storage choices.

The selected backend is used consistently for agent/provider loading, provider-system-prompt resolution, configuration display, client creation, automatic memory, episodic memory, and task/event memory. This prevents the UI from displaying one backend's agents while runtime execution uses another backend.

The Example persistent-session verification also uses the selected HAgent AI store for both client instances so an agent loaded from SQL Server or MySQL is not looked up again in the legacy File store. The conversation persistence portion of that test remains explicitly File-based until conversation repositories are wired to all configured storage backends.

The storage configuration file persists the backend as an explicit enum name (`File`, `SqlServer`, or `MySql`) rather than an opaque numeric value. Saving immediately re-reads the configuration and rejects the save if the selected backend was not persisted correctly.

SQL Server and MySQL resolution bootstraps the HAgent-owned database before creating the corresponding internal repositories. No host application database is used by this resolution path.

Runtime storage resolution uses only the selected backend profile's server, port, username, and password secret. It no longer falls back to the legacy shared connection properties after the profile model has been established.

Storage settings that change the backend, application identity/path, server, port, username, or related connection identity are persisted as configuration for the next process lifetime. The Storage UI informs the user that an application restart is required after such a change so the running HAgent instance does not silently mix storage backends.

Database storage exposes an explicit TCP port with protocol defaults of 1433 for SQL Server and 3306 for MySQL. The selected port is persisted and used by both provider-specific connection builders.

The Example startup path does not terminate when the configured HAgent storage backend cannot be opened. It keeps the configuration surface available, reports the backend-unavailable state in the UI, and exposes the underlying exception through the HAgent message helper so storage settings can be corrected and the application restarted.

The Example's Configuration action also has a recovery path that opens the Storage settings directly when the active backend cannot be opened. It therefore never requires a successful database connection merely to repair database settings.

### Storage foundation

`HAgent.Core` provides `HAgentStorageOptions` with `File`, `SqlServer`, and `MySql` backends, host application naming, application-specific database naming, database port, non-secret connection metadata, and independent database profiles. Database passwords remain outside this ordinary configuration model.

`HAgent.Storage.File` provides an application-specific `HAgentData` directory layout for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime state, cache, and logs.

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` provide HAgent-owned database bootstrappers. They connect to the configured server and port, create the derived HAgent database when absent, then create only HAgent-owned tables and a schema-version record. The initial schema covers providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and future migration metadata.

The previously implemented SQL Server `IDataQuerySource` path against arbitrary host tables was removed because it violated the internal-storage boundary. The provider-neutral structured-query contract remains independent and must not be interpreted as permission to use a host application's business database.

### Live verification

The manual Example must verify the selected internal storage backend. For File storage it should verify the application-specific directory structure and internal repositories. For SQL Server/MySQL it should verify connection, database creation when absent, schema initialization, idempotent re-open, persistence through the configured repositories, and safe refusal to operate against unrelated host application tables.

Database credentials must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- Implicit access to the host application's business database.
- Treating a provider connection as permission to inspect arbitrary host tables.
- Persisting database passwords as ordinary configuration.
- Treating UI discovery, provenance, or model instructions as authorization.

## Definition of done

0.8 is complete only after HAgent internal persistence is selectable and operational across the supported storage backends, schema upgrades are deterministic, and the Example verifies that HAgent storage remains isolated from host application data.
