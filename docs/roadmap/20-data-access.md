# Phase 0.8 — Data Access + Authorization

## Goal
Turn the verified data-discovery/query contracts into safe application and database access.

## Sequence

- [ ] Application-owned `IDataQuerySource` adapter.
- [ ] Authoritative schema/field allow-list.
- [ ] Separate discovery, query, export, and write permissions.
- [ ] Host authorization callbacks.
- [ ] Limits, cancellation, timeout, and resource budgets.
- [ ] Restricted SQL Server read adapter.
- [ ] Restricted MySQL read adapter.
- [ ] Database audit/correlation metadata.
- [ ] Live Example with runtime-only connection fields.

## Boundaries

No raw SQL tool. No implicit access to every table/field. Database credentials remain in the secret system. UI discovery and application-object metadata never grant database authorization.

## Exit criterion

A host can authorize a structured read against its application/database source, execute it through a restricted adapter, and observe bounded results and failures through the public Example verification surface.
