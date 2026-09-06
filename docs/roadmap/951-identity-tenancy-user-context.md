# Phase 0.951 — Identity, Tenancy, and User Context

## Status

**Planned architectural foundation before Phase 0.96.**

## Goal

Define provider-neutral identity and context contracts that allow HAgent to distinguish deployment, tenant, user, session, workspace, agent profile, runtime instance, execution, and related principals without implementing authentication itself.

## Requirements

1. [ ] Define a provider-neutral `Principal`/identity contract suitable for authorization, audit, evaluation, memory, knowledge, and runtime context.
2. [ ] Distinguish deployment/application identity from tenant, user, session, workspace, agent profile, runtime-instance, and execution identity.
3. [ ] Define optional tenancy so single-tenant hosts remain simple while multi-tenant hosts can isolate HAgent resources.
4. [ ] Define identity propagation through execution, tools, memory, knowledge, learning, policy, events, audit, tracing, and evaluation where applicable.
5. [ ] Keep authentication and credential verification outside HAgent unless exposed through generic host-owned contracts.
6. [ ] Define stable scope semantics for global, tenant, user, workspace, agent, runtime, and execution resources.
7. [ ] Ensure private runtime memory and other private resources can be isolated by explicit owner identity.
8. [ ] Make identity context immutable within an execution snapshot.
9. [ ] Define safe behavior when host identity information is absent.
10. [ ] Add deterministic Example verification for single-user, multi-user, and multi-tenant identity propagation and isolation.

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