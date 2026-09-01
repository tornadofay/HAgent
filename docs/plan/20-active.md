# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.10 Workspaces, Routing + Chat

### Objective
Introduce a provider-neutral workspace and routing foundation so a host can place users and runtime-agent participants in one bounded collaboration context without coupling Core to WinForms, a provider, HWorld, or a host application's business database.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace participant model with explicit participant lifecycle state.
- [x] Provider-neutral workspace message metadata with sender, recipient, correlation, causation, sequence, and timestamp.
- [x] Provider-neutral routing contract and implementation enforcing default-recipient and explicit-recipient rules.
- [ ] Deterministic Example verification for workspace routing.
- [ ] Direct workspace routing into runtime execution.
- [ ] Addressed agent-to-agent delegation lifecycle.
- [ ] Workspace message history/persistence policy.
- [ ] Collaboration budgets and loop protection.
- [ ] WinForms chat surface.

### Routing rules

An unaddressed user message routes only to the workspace's active default agent participant. Explicit user-to-agent messages name an active agent participant. Agent-to-agent delegation also names an active agent participant. Broadcast is not part of the base routing operation and must be an explicit later policy.

Routing is separate from execution. `IWorkspaceRouter` creates an authoritative routing result but does not invoke providers, mutate agent profiles, perform host side effects, or bypass permissions.

### Participant identity

A workspace participant has its own participant ID. An agent participant may reference both a persistent profile ID and a live runtime instance ID. Workspace membership does not mutate the persistent `AiAgent` profile or take ownership of the runtime instance lifecycle.

### Message identity

`WorkspaceMessage` preserves workspace ID, message ID, sender, recipient, message kind, correlation ID, causation ID, monotonic sequence, content, and creation time. Correlation identifies the broader operation; causation links a message to the message or event that caused it.

### HWorld boundary

HWorld is not a dependency of this milestone. HWorld can reference HAgent normally and use the public runtime/workspace APIs. HAgent contains no HWorld-specific adapter, world type, physics, simulation scheduling, or action authority.

### Deferred work

Skills, Wiki/content integration, and any remaining 0.8 internal-repository parity remain explicitly deferred and must not be implemented as part of this workspace slice until the runtime/workspace contracts are stable.
