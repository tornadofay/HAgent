# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.8 Data Access + Authorization

### Objective
Turn the verified structured-query contracts into safe application and database access. No arbitrary SQL, unrestricted reflection, or implicit authorization.

### Current slices

- [x] Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
- [x] Authoritative schema/field allow-list independent of model requests.
- [x] Data permissions separated into discovery, projection/query, export, and write operations.
- [x] Host authorization callback contract.
- [x] Query limits, cancellation, timeout, and resource budgets.
- [x] Restricted SQL Server adapter using generated parameterized commands only.
- [ ] Restricted MySQL adapter using generated parameterized commands only.
- [ ] Read-only database tools and result/audit metadata before database writes.

### Current slice: restricted SQL Server structured reads

`HAgent.Storage.SqlServer` now provides `SqlServerDataQuerySource`, a read-only `IDataQuerySource` implementation over SQL Server. It requires a host-owned `DataQuerySchema`, `DataAccessPermissions`, request-specific `IDataAccessAuthorizer`, and `DataQueryExecutionPolicy`.

The adapter generates only structured `SELECT` statements. Projected/filter/sort identifiers come from the authoritative schema and validated table identifiers; scalar filter values and paging values are SQL parameters. `StartsWith`, `Contains`, and `EndsWith` values are escaped for SQL `LIKE` matching. The adapter has no raw SQL input and does not perform writes.

`HAgent.Example` now exposes a dedicated SQL Server Data Query tab with runtime-only Server Name, User Name, Password, and Database fields. It targets a disposable/read-only `dbo.HAgentExampleCustomers` table and verifies successful structured reads, bounded paging, schema rejection, host authorization denial, and parameterized handling of injection-shaped values.

The implementation and Example surface are committed. Live SQL Server verification must be performed locally before the SQL Server roadmap slice and live integration are considered verified.

### Live Example

The SQL integration Example provides runtime-only connection fields:

```text
Server Name
User Name
Password
Database
```

It targets an explicitly disposable/read-only test database and verifies connection, authorized schema/fields, structured queries, bounded results, cancellation/timeout, and unauthorized-operation rejection. Connection values must never become persistent agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- SQL fragments embedded in `DataQueryRequest`.
- Implicit permission to every table or column.
- Treating UI binding, `TableInfo`, object provenance, or model instructions as authorization.
- Persisting test database passwords as ordinary configuration.

## Definition of done

0.8 is complete only after the restricted application/database path is implemented and the matching `HAgent.Example` verification passes locally.
