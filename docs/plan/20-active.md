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
- [ ] Restricted SQL Server adapter using generated parameterized commands only.
- [ ] Restricted MySQL adapter using generated parameterized commands only.
- [ ] Read-only database tools and result/audit metadata before database writes.

### Current slice: bounded structured-query execution

`HAgent.Core` now provides `DataQueryExecutionPolicy` for host-owned query-shape, result-size, and execution-time limits. An application-owned `IDataQuerySource` validates the request against the execution policy and authoritative schema, then creates a linked cancellation token with the configured timeout for authorization and physical execution.

The Example verifies that oversized pages are rejected, caller cancellation propagates through authorization, execution timeouts cancel the operation, and successful bounded queries still return the expected page.

The implementation is committed, but local build/Example verification has not been run. The execution-bounds slice remains pending until that verification passes locally.

### Live Example

When the SQL Server adapter is ready, `HAgent.Example` will provide runtime-only test fields:

```text
Server Name
User Name
Password
Database
```

The Example will target an explicitly disposable/read-only test database and verify connection, authorized schema/fields, structured queries, bounded results, cancellation/timeout, and unauthorized-operation rejection.

Connection values must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- SQL fragments embedded in `DataQueryRequest`.
- Implicit permission to every table or column.
- Treating UI binding, `TableInfo`, object provenance, or model instructions as authorization.
- Persisting test database passwords as ordinary configuration.

## Definition of done

0.8 is complete only after the restricted application/database path is implemented and the matching `HAgent.Example` verification passes locally.
