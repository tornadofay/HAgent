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
- [ ] Wire all remaining internal repositories to the selected storage backend.
- [ ] Versioned schema migrations beyond the initial bootstrap version.
- [ ] HAgent internal storage credentials/secret lifecycle and connection testing UI.
- [ ] Read-only HAgent internal data tools and result/audit metadata before any writes.

### Current slice: internal memory backend parity

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` implement `IMemoryStore` against the HAgent-owned `HAgentMemoryEntries` table. Entries retain scope, kind, owner, task, content, metadata, and timestamps. Core filtering and bounded result counts are performed by each relational provider, while the existing provider-independent memory scoring behavior is preserved after retrieval.

The Example automatic-memory and episodic-memory verification paths obtain their memory store from the selected HAgent storage backend. Their File cleanup behavior was removed so verification cannot delete the real configured File memory store.

The older standalone `[MEMORY]` and `[TASK / EVENT MEMORY]` Example tabs still contain historical File-only verification paths and have not yet been migrated to the configured memory resolver. They must not be used as proof of SQL Server/MySQL memory backend selection until that cleanup slice is complete.

### Configured storage backend resolution

The Example resolves its `IAiStore`, tool-definition store, and memory store from `HAgentStorageOptions` rather than hardcoding the File backend. File, SQL Server, and MySQL are distinct runtime storage choices.

The selected backend is used consistently for agent/provider loading, provider-system-prompt resolution, configuration display, client creation, automatic memory, and episodic memory. This prevents the UI from displaying one backend's agents while runtime execution uses another backend.

The Example persistent-session verification also uses the selected HAgent AI store for both client instances so an agent loaded from SQL Server or MySQL is not looked up again in the legacy File store. The conversation persistence portion of that test remains explicitly File-based until conversation repositories are wired to all configured storage backends.

The storage configuration file persists the backend as an explicit enum name (`File`, `SqlServer`, or `MySql`) rather than an opaque numeric value. Saving immediately re-reads the configuration and rejects the save if the selected backend was not persisted correctly.

SQL Server and MySQL resolution bootstraps the HAgent-owned database before creating the corresponding internal repositories. No host application database is used by this resolution path.

Storage settings that change the backend, application identity/path, database target, server, username, or related connection identity are persisted as configuration for the next process lifetime. The Storage UI informs the user that an application restart is required after such a change so the running HAgent instance does not silently mix storage backends.

### Storage foundation

`HAgent.Core` provides `HAgentStorageOptions` with `File`, `SqlServer`, and `MySql` backends, host application naming, application-specific database naming, and non-secret connection metadata. Database passwords remain outside this ordinary configuration model.

`HAgent.Storage.File` provides an application-specific `HAgentData` directory layout for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime state, cache, and logs.

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` provide HAgent-owned database bootstrappers. They connect to the configured server, create the derived HAgent database when absent, then create only HAgent-owned tables and a schema-version record. The initial schema covers providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and future migration metadata.

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
