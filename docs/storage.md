# Storage model

## File

`HAgent.Storage.File` uses JSON for structured settings because nested provider/agent relationships are clearer and safer to evolve than an INI file.

Secrets are separate files and protected with Windows DPAPI `CurrentUser`.

The architecture is intentionally split:

```text
IAiStore      -> providers + agents
ISecretStore  -> credentials/secrets
```

## SQL Server

Tables:

- `HAgentProviders`
- `HAgentAgents`

Use `SqlServerAiStore.EnsureSchemaAsync(connectionString)` at application setup.

## MySQL

Tables:

- `HAgentProviders`
- `HAgentAgents`

Use `MySqlAiStore.EnsureSchemaAsync(connectionString)` at application setup.

Neither database implementation stores the secret itself. The `SecretId` column identifies the corresponding external secret.
