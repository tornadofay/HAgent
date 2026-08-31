# Active implementation plan

Only the current implementation milestone belongs here. Completed work is recorded in `10-completed.md`; future work belongs under `docs/roadmap/`.

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
