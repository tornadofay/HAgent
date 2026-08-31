# Storage model

Storage separates reusable configuration, live runtime state, executable handlers, and secrets.

## Configuration

`IAiStore` persists reusable providers and agent profiles. `IToolStore` persists tool definitions and schemas. These records describe what can be configured; they are not live runtime agent instances.

## Runtime state

Runtime agent instances, workspaces, and other live collaboration state are not configuration by default. A host may keep them in memory or persist them when recovery, collaboration, audit, or multi-process visibility requires it.

When persisted, runtime records must distinguish the host instance, user/session, workspace, agent profile ID, and runtime instance ID.

## Secrets

`ISecretStore` owns credentials/secrets. Secrets must not be stored in ordinary provider, agent, or tool records and must not appear in normal diagnostics.

`HAgent.Storage.File` currently keeps secrets separate and protects local secrets with Windows DPAPI `CurrentUser`.

## Tool handlers

Tool definitions may be persisted. Executable delegates/handlers are runtime registrations and are never serialized.

## SQL Server

Current foundation tables contain reusable provider, agent, and tool-definition state. The future SQL Server tool layer is a restricted data-access capability, not a raw SQL execution surface.

## MySQL

Current foundation tables mirror the configuration foundation used for SQL Server. The future MySQL tool layer follows the same restricted-query principle.

## Permissions

WinForms UI permissions are owned by the WinForms layer. Database permissions will be separate. A form attachment, object provenance, agent role, or model instruction never grants data authorization.
