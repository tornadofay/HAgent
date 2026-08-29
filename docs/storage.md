# Storage model

HAgent separates persisted definitions/state from executable runtime handlers. Secrets are never persisted as part of provider, agent, or tool definitions.

## File

`HAgent.Storage.File` uses JSON for structured settings because nested provider/agent relationships are clearer and safer to evolve than a flat INI representation.

Secrets are separate files and protected with Windows DPAPI `CurrentUser`.

The current separation is:

```text
IAiStore       -> providers + agents
IToolStore     -> tool definitions/schemas/metadata
ISecretStore   -> credentials/secrets
UI policy      -> WinForms-owned permission settings
```

Tool definitions may persist:

- ID and name
- description
- tool type
- input JSON Schema
- category
- enabled state

Executable delegates/handlers are never serialized. They remain application-owned runtime registrations.

## SQL Server

Current foundation tables:

- `HAgentProviders`
- `HAgentAgents`
- `HAgentTools` (tool definitions)

Use the corresponding `EnsureSchemaAsync(connectionString)` methods during application setup.

The planned SQL Server tool layer must remain a restricted capability surface. Database credentials stay in the configured secret store, and arbitrary model-generated SQL is not treated as an authorization boundary.

## MySQL

Current foundation tables:

- `HAgentProviders`
- `HAgentAgents`
- `HAgentTools` (tool definitions)

Use the corresponding `EnsureSchemaAsync(connectionString)` methods during application setup.

The planned MySQL tool layer follows the same restricted-query principle as SQL Server.

## UI permissions

WinForms automatic UI/data policy is stored by the WinForms layer and controls convenience discovery/read/write/invoke behavior. It is separate from database permissions.

A form attachment, memory provenance, or model instruction does not grant authorization.
