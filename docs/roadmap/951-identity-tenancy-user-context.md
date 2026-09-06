# Phase 0.951 — Identity, Tenancy, and User Context

## Status

**In progress — core identity context and execution propagation implemented first.**

## Goal

Define provider-neutral identity and context contracts that allow HAgent to distinguish deployment, tenant, user, session, workspace, agent profile, runtime instance, execution, and related principals without implementing authentication itself.

## Requirements

1. [ ] Define a provider-neutral `Principal`/identity contract suitable for authorization, audit, evaluation, memory, knowledge, and runtime context.
2. [x] Distinguish deployment/application identity from tenant, user, session, workspace, agent profile, runtime-instance, and execution identity through a shared `AgentIdentityContext` and existing runtime/execution identities.
3. [ ] Define optional tenancy so single-tenant hosts remain simple while multi-tenant hosts can isolate HAgent resources.
4. [x] Begin identity propagation through execution snapshots and public execution results; extend the same context to tools, memory, knowledge, learning, policy, events, audit, tracing, and evaluation in their respective phases.
5. [x] Keep authentication and credential verification outside HAgent; the identity contract is host-supplied context only.
6. [ ] Define stable scope semantics for global, tenant, user, workspace, agent, runtime, and execution resources.
7. [ ] Ensure private runtime memory and other private resources can be isolated by explicit owner identity.
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

The identity object is immutable after construction. The execution snapshot keeps its own copy so caller-owned request state cannot mutate the identity of an active execution.

This is deliberately a foundation, not a requirement that every host implement tenants, users, or workspaces.

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
```

HAgent consumes identity context; the host remains responsible for authentication and authoritative user/account lifecycle.
