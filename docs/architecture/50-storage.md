# Storage architecture

HAgent separates persistent definitions, runtime state, executable handlers, and secrets.

## Definitions

Persistent stores may contain reusable provider, agent, and tool definitions. These are configuration, not live execution objects.

## Runtime state

Runtime agent instances, workspaces, and other live collaboration state are separate from reusable definitions. Hosts may keep this state in memory or persist it when recovery, collaboration, auditing, or multi-process visibility requires it.

## Secrets

Credentials are owned by `ISecretStore`. They are not ordinary provider, agent, or tool properties and must not appear in diagnostics by default.

## Handlers

Executable tool handlers remain runtime registrations and are never serialized.

## Backends

File, SQL Server, and MySQL stores provide persistence implementations. Database-backed stores must preserve user/process/workspace/runtime identity when persisting live state.
