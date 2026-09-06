# Phase 0.951 — Identity, Tenancy, and User Context

## Status

**Completed — verified in HAgent.Example.**

## Goal

Define provider-neutral identity and context contracts that allow HAgent to distinguish deployment, tenant, user, session, workspace, agent profile, runtime instance, execution, and related principals without implementing authentication itself.

## Requirements

1. [x] Define a provider-neutral identity context suitable for authorization, audit, evaluation, memory, knowledge, and runtime context. A separate `Principal` object is not required when the shared context is sufficient.
2. [x] Distinguish deployment/application identity from tenant, user, session, workspace, agent profile, runtime-instance, and execution identity through a shared `AgentIdentityContext` and existing runtime/execution identities.
3. [x] Define optional tenancy so single-tenant hosts remain simple while multi-tenant hosts can isolate HAgent resources through an explicit `TenantId` boundary.
4. [x] Propagate the shared identity context through execution snapshots and public execution results, with tool-execution and audit projections now carrying the same identity context. Extend the same context to memory, knowledge, learning, policy, events, tracing, and evaluation in their respective phases.
5. [x] Keep authentication and credential verification outside HAgent; the identity contract is host-supplied context only.
6. [x] Define stable resource scope semantics for Global, Tenant, User, Workspace, Agent, Runtime, and Execution through `AgentResourceScope`.
7. [x] Ensure private runtime memory and other private resources can be isolated by explicit owner identity through the canonical `AgentResourceOwnership` contract. The redesign rule requires subsystems to consume the canonical ownership model directly rather than preserving obsolete parallel mechanisms.
8. [x] Make identity context immutable within an execution snapshot by cloning the host-supplied identity when the snapshot is created.
9. [x] Define safe behavior when host identity information is absent; identity fields are optional and default to empty values rather than fabricated identities.
10. [x] Add deterministic Example verification for single-user, multi-user, and multi-tenant identity propagation and isolation.

## Initial implementation

The first implementation slice introduced:

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

`AgentIdentityContext` is provider-neutral and carries:

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

HAgent defines a canonical `AgentResourceScope` and `AgentResourceOwnership.GetOwnerId(...)` contract for HAgent-owned resource partitioning:

```text
Global
Tenant
User
Workspace
Agent
Runtime
Execution
```

The canonical owner key preserves deployment and, where applicable, tenant context. Therefore the same `UserId` in two tenants cannot produce the same user owner key.

Private runtime memory ownership remains distinct from execution ownership. Runtime instances use their runtime identity, while execution-scoped resources use the execution identity. These identities must not be collapsed.

Storage partitioning is not authorization. Authorization remains a policy decision using the supplied identity, operation, resource scope, and applicable policy.

## Verification

`HAgent.Example` verifies:

```text
Identity Snapshot
Identity Execution
Identity Tool
Identity Isolation
```

The verified isolation scenario covers:

- separate owner keys for different users;
- tenant-qualified user ownership;
- isolation of the same user ID across tenants;
- private memory visibility by owner;
- distinct runtime and execution resource scopes;
- preservation of unrestricted-store behavior as distinct from authorization.

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

HAgent consumes identity context; the host remains responsible for authentication and authoritative user/account lifecycle.
