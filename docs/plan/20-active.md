# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.8 Data Access + Authorization

### Objective
Turn the verified structured-query contracts into safe application and database access. No arbitrary SQL, unrestricted reflection, or implicit authorization.

### Current slices

- [ ] Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
- [ ] Authoritative schema/field allow-list independent of model requests.
- [ ] Data permissions separated into discovery, projection/query, export, and write operations.
- [ ] Host authorization callback contract.
- [ ] Query limits, cancellation, timeout, and resource budgets.
- [ ] Restricted SQL Server adapter using generated parameterized commands only.
- [ ] Restricted MySQL adapter using generated parameterized commands only.
- [ ] Read-only database tools and result/audit metadata before database writes.

### Current slice: application-owned structured reads

`HAgent.Core` now provides `DataQuerySchema`, a host-owned authoritative field allow-list. The Example application-owned `IDataQuerySource` validates each request against its schema before evaluating filters, sorting, or projection. Schema membership is intentionally narrower than full authorization; later 0.8 slices add operation permissions and host authorization.

`HAgent.Example` uses an in-memory application-owned `IDataQuerySource` with an explicit `Id`, `Name`, and `Amount` schema and verifies that an existing but non-approved `Secret` field is rejected before execution.

The implementation is committed, but local build/Example verification has not been run. These two roadmap slices remain pending until that verification passes locally.

### Current slice: data-operation permissions

`HAgent.Core` now provides `DataAccessPermissions` with separate `Discovery`, `ProjectionQuery`, `Export`, and `Write` controls. The structured query source must enforce `ProjectionQuery` before executing a query. The policy does not authorize a specific runtime identity or context; host authorization remains a separate 0.8 slice.

`HAgent.Example` also verifies that a source with `ProjectionQuery` disabled rejects the request with `UnauthorizedAccessException` before data execution.

The implementation is committed, but local build/Example verification has not been run.

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
