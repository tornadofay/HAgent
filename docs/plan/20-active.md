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
- [ ] Wire all internal repositories to the selected storage backend.
- [ ] Versioned schema migrations beyond the initial bootstrap version.
- [ ] HAgent internal storage credentials/secret lifecycle and connection testing UI.
- [ ] Read-only HAgent internal data tools and result/audit metadata before any writes.

### Current slice: configured storage backend resolution

The Example now resolves its `IAiStore` and tool-definition store from `HAgentStorageOptions` rather than hardcoding the File backend. File, SQL Server, and MySQL are therefore distinct runtime storage choices.

The selected backend is used consistently for agent/provider loading, provider-system-prompt resolution, configuration display, and client creation. This prevents the UI from displaying one backend's agents while runtime execution uses another backend.

SQL Server and MySQL resolution bootstraps the HAgent-owned database before creating the corresponding internal repositories. No host application database is used by this resolution path.

### Storage foundation

`HAgent.Core` now provides `HAgentStorageOptions` with `File`, `SqlServer`, and `MySql` backends, host application naming, application-specific database naming, and non-secret connection metadata. Database passwords remain outside this ordinary configuration model.

`HAgent.Storage.File` now provides an application-specific `HAgentData` directory layout for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime state, cache, and logs.

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` now provide HAgent-owned database bootstrappers. They connect to the configured server, create the derived HAgent database when absent, then create only HAgent-owned tables and a schema-version record. The initial schema covers providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and future migration metadata.

The WinForms AI Configuration surface now includes a Storage page for selecting the backend and configuring application name, file root, database name, server name, and username. The password field is transient and is cleared after saving; it is not serialized as ordinary configuration. The default File paths now live beneath the host executable's application-specific `HAgentData` directory.

The previously implemented SQL Server `IDataQuerySource` path against arbitrary host tables was removed because it violated the internal-storage boundary. The provider-neutral structured-query contract remains independent and must not be interpreted as permission to use a host application's business database.

### Live verification

The manual Example must verify the selected internal storage backend. For File storage it should verify the application-specific directory structure and internal repositories. For SQL Server/MySQL it should verify connection, database creation when absent, schema initialization, idempotent re-open, and safe refusal to operate against unrelated host application tables.

Database credentials must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- Implicit access to the host application's business database.
- Treating a provider connection as permission to inspect arbitrary host tables.
- Persisting database passwords as ordinary configuration.
- Treating UI discovery, provenance, or model instructions as authorization.

## Definition of done

0.8 is complete only after HAgent internal persistence is selectable and operational across the supported storage backends, schema upgrades are deterministic, and the Example verifies that HAgent storage remains isolated from host application data.
