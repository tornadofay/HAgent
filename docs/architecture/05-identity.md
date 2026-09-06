# Identity, Tenancy, and User Context

## Purpose

HAgent consumes provider-neutral identity context so authorization, memory, knowledge, events, tracing, evaluation, runtime state, and future policy decisions can distinguish who and where an operation belongs. HAgent does not implement authentication or act as the authoritative identity provider.

## Identity layers

```text
Deployment
  -> Tenant (optional)
      -> Principal / User
          -> Session
              -> Workspace (optional)
                  -> Agent Profile
                      -> Runtime Instance
                          -> Execution
```

These identifiers have different meanings and must not be collapsed into one generic ID.

- `DeploymentId` identifies the host/deployment context.
- `TenantId` optionally identifies a tenant boundary.
- `PrincipalId` identifies the host-authenticated principal at the authorization boundary.
- `UserId` provides the host's stable user identity where the host distinguishes users separately from principals.
- `SessionId` identifies the host/user session when one exists.
- `WorkspaceId` identifies an optional HAgent workspace context.
- Agent profile, runtime-instance, and execution identities remain HAgent identities and are not replaced by user identity.

## Authentication boundary

The host remains responsible for authentication, identity verification, account lifecycle, and authoritative user information. HAgent receives identity context as trusted input from the host boundary and does not infer identity from model text, prompts, tools, or arbitrary context.

An HAgent host may omit identity information. Missing identity is valid for single-user/local scenarios, but features that require an identity boundary must fail closed or require the host to supply the required context rather than inventing an identity.

## Execution propagation

`AgentExecutionRequest.Identity` is the canonical host input for deployment, tenancy, principal, user, session, and workspace context.

At execution start the identity is copied into `AgentExecutionSnapshot.Identity`. The snapshot owns an independent immutable copy, so later mutation of caller-owned request state cannot change the identity associated with a running execution.

`AgentExecution.Identity` exposes the captured execution identity for downstream consumers such as policy, tracing, evaluation, tools, memory, knowledge, and event processing.

```text
Host
  -> AgentExecutionRequest.Identity
  -> Execution Snapshot
  -> AgentExecution.Identity
  -> downstream HAgent subsystems
```

Execution identity remains distinct from `AgentExecution.Id`, `AgentExecution.CorrelationId`, `HostCorrelationId`, and `AgentRuntimeInstance.InstanceId`.

## Scope model

Identity and resource scope are related but not interchangeable.

```text
Global
Tenant
User
Workspace
Agent
Runtime
Execution
```

A resource may be visible at one scope while its authorization decision uses another identity dimension. HAgent must not assume that `UserId == PrincipalId`, that every deployment is multi-tenant, or that every execution belongs to a workspace.

## Multi-tenant behavior

Tenancy is optional. Single-tenant applications should not need to create artificial tenants. Multi-tenant hosts may supply `TenantId` and use it to partition or authorize HAgent-owned resources through the applicable policy and storage contracts.

HAgent storage remains HAgent-owned. Tenant/user identity can partition HAgent records, but it must never turn HAgent storage into implicit access to the host application's business database.

## Runtime state

Runtime-state persistence already contains host/user/workspace/session metadata. The identity foundation provides the common semantic model for those fields and allows future persistence contracts to use the same identity definitions instead of introducing unrelated identity representations.

Runtime instance identity remains separate from user identity: one user may own multiple runtime instances, and runtime identity may continue across multiple executions.

## Security and authorization relationship

Identity provides context; it does not grant authority.

```text
Identity
    + requested operation
    + resource scope
    + policy
        -> authorization decision
```

A principal, user, tenant, or `IsAdmin`-style host attribute must not automatically grant tool invocation, host database access, memory access, learning promotion, or other capabilities unless the applicable HAgent/host policy explicitly permits it.

## Future propagation

Later phases must use this same identity context rather than creating subsystem-specific user/tenant objects. In particular, identity should propagate where applicable through:

```text
Tools
Memory
Knowledge / Skills
Learning
Policy
Events
Execution tracing
Evaluation
Runtime persistence
Workspaces
Human intervention / approval
```

Each subsystem should consume only the identity dimensions it actually needs.

## Architectural rule

Identity is context, not authentication, authorization, or application-domain state. HAgent provides the propagation contract; the host remains authoritative for identity verification and host-specific account semantics.
