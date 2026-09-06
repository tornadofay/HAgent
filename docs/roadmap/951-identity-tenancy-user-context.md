# Phase 0.951 — Identity, Tenancy, and User Context

## Status

**In progress — identity propagation, resource ownership, and tenant isolation contracts implemented; final Example verification remains.**

## Goal

Define provider-neutral identity and context contracts that allow HAgent to distinguish deployment, tenant, user, session, workspace, agent profile, runtime instance, execution, and related principals without implementing authentication itself.

## Requirements

1. [x] Define a provider-neutral identity context suitable for authorization, audit, evaluation, memory, knowledge, and runtime context. A separate `Principal` object is not required when the shared identity context is sufficient.
2. [x] Distinguish deployment/application identity from tenant, user, session, workspace, agent profile, runtime-instance, and execution identity through a shared `AgentIdentityContext` and existing runtime/execution identities.
3. [x] Define optional tenancy so single-tenant hosts remain simple while multi-tenant hosts can isolate HAgent resources through an explicit `TenantId` boundary.
4. [x] Propagate the shared identity context through execution snapshots and public execution results, with tool-execution and audit projections now carrying the same identity context. Extend the same context to memory, knowledge, learning, policy, events, tracing, and evaluation in their respective phases.
5. [x] Keep authentication and credential verification outside HAgent; the identity contract is host-supplied context only.
6. [x] Define stable resource scope semantics for Global, Tenant, User, Workspace, Agent, Runtime, and Execution through `AgentResourceScope`.
7. [x] Ensure private runtime memory and other private resources can be isolated by explicit owner identity through the canonical `AgentResourceOwnership` owner-key contract and the existing `OwnerId` storage boundary.
8. [x] Make identity context immutable within an execution snapshot by cloning the host-supplied identity when the snapshot is created.
9. [x] Define safe behavior when host identity information is absent; identity fields are optional and default to empty values rather than fabricated identities.
10. [ ] Add deterministic Example verification for single-user, multi-user, and multi-tenant identity propagation and isolation.

## Initial implementation

The first implementation slice introduces:

```text
AgentExecutionRequest.Identity
        ↓
AgentExecutionSnapshot.Identity
        ↓
AgentExecution.Identity
        ├── ToolExecutionContext.Identity
        ├── ToolExecutionResult.Identity
        └── AgentExecutionAuditRecord identity projection
```

`AgentIdentityContext` is provider-neutral and currently carries:

```text
DeploymentId
TenantId
PrincipalId
DisplayName
UserId
SessionId
WorkspaceId
```

The execution snapshot keeps its own copy so caller-owned request state cannot mutate the identity associated with an active execution. Tool execution receives the same captured context, and audit projections retain identity dimensions without storing sensitive payloads.

## Resource ownership implementation

HAgent now defines a canonical `AgentResourceScope` and `AgentResourceOwnership.GetOwnerId(...)` contract for HAgent-owned resource partitioning:

```text
Global
Tenant
User
Workspace
Agent
Runtime
Execution
```

Existing memory and other stores may continue using their string `OwnerId` field. The ownership helper derives deterministic keys that preserve deployment context and, where applicable, tenant context. In particular, the same `UserId` in two different tenants cannot produce the same user owner key.

Private runtime memory continues to use the runtime instance identity (`AgentRuntimeInstance.MemoryOwnerId`), while resource-scope ownership can additionally preserve deployment and tenant partition context. Storage filtering remains a partitioning mechanism, not an authorization decision; callers must combine owner selection with the applicable policy.

This deliberately keeps single-user hosts simple: `TenantId` can remain empty. Multi-tenant hosts supply `TenantId` and use tenant/user/workspace/resource ownership keys as required.

## Architectural outcome

```text
Deployment
  -> Tenant (optional)
      -> User / Principal
          -> Session
              -> Workspace (optional)
                  -> Agent Profile
                      -> Runtime Instance
                          -> Execution
                              -> Resource Scope + Owner Key
                                  -> Tools / Memory / Knowledge / Audit / downstream subsystems
```

HAgent consumes identity context; the host remains responsible for authentication and authoritative user/account lifecycle. Final completion of this phase requires deterministic Example verification of the ownership and isolation behavior.
