# Security and authorization

HAgent treats the model as an untrusted requester, not an authority.

## Permission

Capabilities such as discovery, read, write, invoke, query, and export must be separately controllable. Permission state belongs to the host/runtime boundary rather than the prompt.

## Authorization

Authorization answers whether a specific requested operation is allowed for a specific runtime identity and context. Discovery metadata, object provenance, UI bindings, agent instructions, and role names never grant authorization by themselves.

## Approval

Sensitive operations may require explicit host or human approval. Approval is a lifecycle object, not a Boolean hidden in the system prompt.

## Data access

Database access must use an allow-listed schema and structured query model. Credentials use the secret subsystem and must not be persisted as ordinary agent/tool configuration.

## Isolation

Runtime agents may have private memory. Shared workspace/application memory requires explicit policy. Runtime identities must remain isolated across users, processes, and workspaces when persistence is enabled.

## Observability

Execution, tool, and authorization events should use correlation IDs and configurable redaction. Secrets and sensitive payloads must not be logged by default.
