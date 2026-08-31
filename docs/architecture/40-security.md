# Security and authorization

HAgent treats the model as an untrusted requester, not an authority.

## Permission

Capabilities such as discovery, read, write, invoke, query, and export must be separately controllable. Permission state belongs to the host/runtime boundary rather than the prompt.

Data-operation permissions may separately control discovery, projection/query, export, and write behavior for future HAgent capabilities. One permission must never imply another.

## Authorization

Authorization answers whether a specific requested operation is allowed for a specific runtime identity and context. `IDataAccessAuthorizer` is a provider-neutral callback contract for request-specific data operations; its execution is runtime-owned and is never persisted as ordinary agent/tool configuration.

Discovery metadata, object provenance, UI bindings, agent instructions, and role names never grant authorization by themselves.

## Approval

Sensitive operations may require explicit host or human approval. Approval is a lifecycle object, not a Boolean hidden in the system prompt.

## Internal storage

HAgent storage is an internal persistence boundary. Its selected backend (`File`, `SqlServer`, or `MySql`) stores HAgent-owned data such as providers, agents, tools, memory, conversations, skills, wiki/content, and future internal metadata.

A database storage backend must connect only to the HAgent-owned database derived from the host application name, normally `<application-name>-ai`. It must never use the host application's business database as an implicit data source and must never inspect or modify unrelated application tables.

The File backend uses an application-specific directory beneath the host executable location. SQL Server and MySQL backends create or upgrade only HAgent-owned databases/tables through versioned schema initialization and migration.

## Secrets

Database passwords, provider API keys, and other credentials remain in the secret/runtime boundary. They must not be persisted in ordinary provider, agent, tool, or storage-option records and must not appear in normal diagnostics.

## Structured data contracts

`DataQueryRequest` and the related schema/permission/authorization contracts describe bounded structured intent where HAgent exposes such a capability. They are not a license for database access. No model-provided SQL, executable expression, unrestricted reflection, or implicit host-application database access is allowed.

## Isolation

Runtime agents may have private memory. Shared workspace/application memory requires explicit scope and authorization. Runtime persistence must distinguish host instance, user/session, workspace, agent profile ID, and runtime instance ID.

## Observability

Execution, tool, authorization, and storage events should use correlation IDs and configurable redaction. Secrets and sensitive payloads must not be logged by default.
