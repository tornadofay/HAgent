# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.10 Workspaces, Routing + Chat

### Objective
Provide an optional shared conversation where a user and multiple runtime agents can visibly work together while every model request is routed deliberately. The foundation must remain provider-neutral, host-controlled, bounded, and independent of any specific domain.

### Current slices

- [x] Introduce provider-neutral workspace identity and lifecycle.
- [x] Register users and runtime-agent participants with explicit lifecycle state.
- [x] Define one workspace default recipient for unaddressed user messages.
- [x] Define direct user-to-agent addressing.
- [x] Define addressed agent-to-agent delegation and responses.
- [x] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
- [ ] Define coordinator/specialist roles as policy over generic runtime agents.
- [ ] Allow specialists to represent whole domains, tables, subsystems, capabilities, or other host responsibilities.
- [ ] Execute routed workspace messages through runtime agents.
- [ ] Make agent-to-agent work visible in the workspace when the host enables it.
- [ ] Add configurable addressing syntax at the host/UI layer.
- [ ] Add loop protection and collaboration budgets.
- [ ] Add optional workspace persistence and shared-memory policy.
- [ ] Add the WinForms chat surface and global agent selection.

### Workspace foundation

`AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter` provide the provider-neutral communication foundation. Unaddressed user messages target only the active workspace default recipient. Explicit user addressing and explicit agent delegation target only the requested participant. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

The `WORKSPACE ROUTING` Example verification is complete and confirms participant identity/lifecycle, default recipient behavior, direct addressing, delegation, and preservation of sender/recipient/correlation/causation/sequence metadata.

### Coordinator/specialist rule

Coordinator and specialist are roles/policies over the same generic runtime-agent model. HAgent must not introduce separate coordinator or specialist agent classes. A role policy may describe routing eligibility, delegation permissions, preferred capabilities, or collaboration limits while preserving normal runtime-instance identity and lifecycle.

### HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public workspace/runtime APIs. HAgent does not add HWorld-specific dependencies, world types, physics, rendering, simulation scheduling, or action authority.

## Verification rule

A 0.10 slice becomes complete only after its implementation exists, its matching Example verification passes locally, and the authoritative documentation reflects the result.
