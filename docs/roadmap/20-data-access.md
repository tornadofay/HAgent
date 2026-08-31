# Phase 0.8 — Data Access + Authorization

## Goal
Turn the verified data-discovery and structured-query contracts into safe real application/database access.

## Steps

1. [x] Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
2. [x] Authoritative schema/field allow-list independent of model requests.
3. [x] Separate permissions for discovery, projection/query, export, and write operations.
4. [x] Host authorization callback contract.
5. [x] Query/result limits, cancellation, timeout, and resource budgets.
6. [x] Restricted SQL Server read adapter using generated parameterized commands only.
7. [ ] Restricted MySQL read adapter using generated parameterized commands only.
8. [ ] Database audit/correlation metadata.
9. [ ] Read-only database tools before database writes.
10. [ ] Live `HAgent.Example` integration against a disposable/read-only test database.

## Live Example

The SQL integration Example provides runtime-only connection fields:

```text
Server Name
User Name
Password
Database
```

It targets an explicitly disposable/read-only `dbo.HAgentExampleCustomers` table and verifies connection, authorization, schema/field allow-listing, structured queries, bounded results, cancellation/timeout, and rejection of unauthorized operations. Connection values must never become persistent agent/tool configuration or normal logs.

The SQL Server adapter generates only restricted structured `SELECT` statements. Projected, filtered, and sorted identifiers come from validated schema/table identifiers; scalar filter and paging values are parameters. No raw SQL request surface or database writes are exposed.

## Boundaries

- No raw SQL tool.
- No arbitrary SQL fragments in the structured query contract.
- No implicit access to every table or field.
- UI discovery, `TableInfo`-style metadata, provenance, or model instructions do not grant authorization.
- Data-operation permission policy and request-specific host authorization are separate controls.
- Database passwords remain in the secret/connection boundary.

## Exit criterion

A host can authorize and execute a bounded structured read against its application or database source through the public HAgent abstractions, and the Example verifies both successful access and denied access cases.
