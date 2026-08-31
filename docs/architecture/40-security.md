# Security and authorization

HAgent treats the model as an untrusted requester, not an authority.

## Permission

Capabilities such as discovery, read, write, invoke, query, and export must be separately controllable. Permission state belongs to the host/runtime boundary rather than the prompt.

Data access uses separate coarse-grained permissions for `Discovery`, `ProjectionQuery`, `Export`, and `Write`. A source may enable a subset of these operations without treating one permission as implicit authorization for another. Permission policy is a host/runtime control and remains distinct from schema membership and request-specific authorization.

## Authorization

Authorization answers whether a specific requested operation is allowed for a specific runtime identity and context. `IDataAccessAuthorizer` receives a provider-neutral `DataAuthorizationRequest` containing the operation class, source identity, runtime identity/context, and relevant structured query. Discovery metadata, object provenance, UI bindings, agent instructions, and role names never grant authorization by themselves.

Authorization callbacks are runtime-owned and executable. They are not persisted as ordinary agent/tool configuration.

## Approval

Sensitive operations may require explicit host or human approval. Approval is a lifecycle object, not a Boolean hidden in the system prompt.

## Data access

Structured data access uses a host-owned `DataQuerySchema` as an authoritative field allow-list independent of model requests. A `DataQueryRequest` is validated against that schema before the source performs filtering, sorting, or projection. Schema membership describes which fields the source intentionally exposes; it does not replace operation permissions or host authorization.

An application-owned source must enforce the relevant data permission and then obtain request-specific authorization before executing the operation. The current structured-query path requires `ProjectionQuery` and a positive `IDataAccessAuthorizer` decision. Export and write permissions are defined separately so they cannot be inferred from query access.

Database access must use an allow-listed schema and structured query model. Credentials use the secret subsystem and must not be persisted as ordinary agent/tool configuration.

## Isolation

Runtime agents may have private memory. Shared workspace/application memory requires explicit policy. Runtime identities must remain isolated across users, processes, and workspaces when persistence is enabled.

## Observability

Execution, tool, and authorization events should use correlation IDs and configurable redaction. Secrets and sensitive payloads must not be logged by default.
