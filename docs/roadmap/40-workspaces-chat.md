# Phase 0.10 — Workspaces, Routing + Chat

## Goal
Provide an optional shared conversation where a user and multiple runtime agents can visibly work together while every model request is routed deliberately.

## Steps

1. [ ] Introduce a workspace abstraction independent of WinForms.
2. [ ] Register users and runtime-agent participants with explicit lifecycle state.
3. [ ] Define one workspace default recipient for unaddressed user messages.
4. [ ] Define direct user-to-agent addressing.
5. [ ] Define addressed agent-to-agent delegation and responses.
6. [ ] Make coordinator/specialist behavior a role/policy over generic runtime agents.
7. [ ] Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. [ ] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. [ ] Make agent-to-agent work visible in the workspace when the host enables it.
10. [ ] Add configurable addressing syntax at the host/UI layer.
11. [ ] Add loop protection and collaboration budgets.
12. [ ] Add optional workspace persistence and shared-memory policy.
13. [ ] Add the WinForms chat surface and global agent selection.

### Current foundation

The provider-neutral foundation now contains `AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter`. Participants are either users or runtime agents and have explicit Active/Suspended/Retired state. An active default recipient may be defined for unaddressed user messages. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

The `HAgent.Example` application contains deterministic `WORKSPACE ROUTING` verification. The implementation is pending local verification before Step 1 is marked complete.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit policy invokes others.
- Broadcast: explicit opt-in operation, never the default.

The user can observe the coordinator ask a specialist to work and the specialist return its result before the coordinator answers.

## Specialist context

A contextual specialist may be created automatically from a configured profile. Its runtime prompt/context can contain UI, data, application-object, task, or other host context according to authorization policy. The specialist can report what it knows, inferred information, unknowns, and authorization limits honestly.

## HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public runtime/workspace APIs. HAgent does not add an HWorld-specific dependency, adapter, world type, physics, simulation scheduling, or action authority.

## Exit criterion

A host can run a visible multi-agent conversation in which messages reach only their intended recipients, coordinator/specialist delegation works, and workspace execution remains bounded and traceable.
