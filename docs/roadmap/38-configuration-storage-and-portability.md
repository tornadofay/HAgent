# Phase 0.96.x — Configuration, Storage, and Portability Evolution

## Status

**Cross-cutting foundation required before completion of 0.96 and before 0.97 consumes long-lived configuration.**

## Purpose

Provide one authoritative persistence/configuration model for providers, models, concrete execution targets, Agent selection policy, resources, learning configuration, permissions, and global defaults across the supported File, SQL Server, and MySQL backends.

This phase is infrastructure. It does not perform provider/model routing and does not become a second configuration architecture.

## V1 storage model

```text
General settings
Providers
Logical models (where identity can be established)
Concrete execution targets
Capabilities / evidence
Constraints
Operational quota/rate/capacity state
Cost state
Agents / selection policy
Skills
Knowledge / Wiki
Memory / learning configuration
Tools
Permissions / policy
Resource relationships
```

The same logical configuration model must exist regardless of storage backend.

## Delivery slices

### Slice 1 — Authoritative configuration model

- Replace obsolete permanent Agent provider/model binding with selection preferences and requirements.
- Keep Provider, logical Model, and concrete Execution Target distinct.
- Persist global defaults such as cost policy, default selection mode, fallback policy, learning defaults, and discovery/refresh defaults.
- Preserve explicit inherit/override semantics.
- Version configuration records so active runtime snapshots can detect relevant changes.

### Slice 2 — Provider credentials

- Persist provider API keys with provider configuration where applicable.
- Encrypt credentials at rest in File, SQL Server, and MySQL storage.
- Redact credentials from logs, diagnostics, tracing, audit records, Examples, and UI diagnostic output.
- Support credential replacement/removal and configuration refresh.
- Keep storage-server passwords outside ordinary provider configuration.
- Do not introduce a separate secret-vault architecture.

### Slice 3 — Resource and relationship persistence

- Persist Skills, Knowledge/Wiki, Memory policy, Learning configuration, Tools, Permissions, and explicit resource relationships.
- Preserve canonical scope/ownership, version, provenance, lifecycle, and relationship identity.
- Persist learning candidate state/provenance once the 0.9575 lifecycle requires it.
- Keep executable handlers and live runtime state out of persistence.

### Slice 4 — Runtime snapshots and invalidation

- Persist revisions/version metadata sufficient for long-lived execution snapshots.
- Avoid reloading unchanged configuration from persistence on every execution when a valid snapshot exists.
- Define lightweight refresh/invalidation suitable for File, SQL Server, and MySQL.
- Ensure revoked/changed configuration cannot remain effective indefinitely.
- Preserve immutable execution snapshots even when persistence changes during execution.

### Slice 5 — Configuration portability

- Define one versioned export/import package independent of the physical storage backend.
- Export HAgent-owned configuration, not process-local state.
- Exclude live runtimes, active executions, synchronization primitives, provider sessions, and executable handlers.
- Normal export excludes credentials.
- Optional credential-bearing export contains encrypted credentials protected by the package mechanism.
- Validate package compatibility and produce deterministic import-conflict results.
- Preserve IDs and relationships where possible and explicitly remap only when required.

### Slice 6 — Backend parity and multi-process behavior

- Keep File, SQL Server, and MySQL behavior logically aligned.
- Add ordered schema migrations where required.
- Support authorized processes sharing database-backed HAgent configuration.
- Ensure configuration revision/refresh semantics prevent one process from using revoked configuration forever.
- Keep HAgent storage isolated from host business databases.

### Slice 7 — Verification

Verify:

- backend round-trip parity;
- encrypted-at-rest credentials and redaction;
- configuration revision invalidation;
- shared-database visibility;
- Auto/Preferred/Fixed selection persistence;
- cost/fallback policy persistence;
- resource relationships;
- export/import with and without credentials;
- deterministic import conflicts;
- immutable active execution snapshots after configuration edits.

## Architectural rules

1. There is one authoritative HAgent configuration model.
2. Persistence is an implementation boundary, not a second domain model.
3. File, SQL Server, and MySQL are interchangeable storage implementations of the same logical contracts.
4. Active executions use immutable snapshots rather than mutable database records.
5. Provider credentials are encrypted at rest and never become diagnostic data.
6. Export/import never exports executable handlers or live runtime state.
7. HAgent storage never becomes an implicit gateway to a host application's business database.
8. Configuration changes invalidate or supersede affected snapshots deterministically.

## Dependency relationship

```text
0.9592 provider/adapters
        ↓
0.9593 reasoning requirement / boundary foundation
        ↓
0.96.x configuration + storage
        ↓
0.96 execution planning
        ↓
0.97 persistent cognition
```

0.9593 establishes the provider-neutral reasoning contract that 0.96 and 0.97 will consume; 0.96.x remains responsible for persisting and versioning the resulting configuration and execution-related state.

## Exit criterion

All configuration required by 0.96 and 0.97 can be represented in one authoritative model, persisted consistently by supported backends, refreshed without corrupting active snapshots, shared safely where database deployment is used, and exported/imported without carrying transient execution state.
