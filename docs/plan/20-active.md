# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.10 Workspaces, Routing + Chat

### Objective
Provide an optional shared conversation where one host user and multiple runtime agents can visibly work together while every model request is routed deliberately. Workspace state must survive UI close, application shutdown, and later restart when the host supplies the same stable user identity.

### Current slices

- [x] Introduce provider-neutral workspace identity and lifecycle.
- [x] Register users and runtime-agent participants with explicit lifecycle state.
- [x] Define one workspace default recipient for unaddressed user messages.
- [x] Define direct user-to-agent addressing.
- [x] Define addressed agent-to-agent delegation and responses.
- [x] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
- [x] Define coordinator/specialist roles as policy over generic runtime agents; `WORKSPACE ROLES` verification passed.
- [ ] Allow specialists to represent whole domains, tables, subsystems, capabilities, or other host responsibilities.
- [ ] Execute routed workspace messages through runtime agents.
- [ ] Make agent-to-agent work visible in the workspace Lobby when enabled.
- [ ] Add configurable addressing syntax at the host/UI layer without making prompt syntax authoritative.
- [ ] Add loop protection and collaboration budgets.
- [ ] Add persistent workspace state and explicit shared-memory policy.
- [ ] Add stable host user identity (`UserId`, `IsAdmin`) and database-safe user/workspace partitioning.
- [ ] Add create/open/show/hide/close workspace lifecycle APIs where UI close never destroys persisted state.
- [ ] Add host/application configuration for a single `Enable Workspace` setting. Workspace is always hidden until explicitly opened.
- [ ] Add default manager/coordinator agent configuration.
- [ ] Add default specialist agent configuration and responsibility metadata.
- [ ] Add configurable default approval type/policy.
- [ ] Add allowed provider/agent/model selection and runtime overrides for workspace and private chats without mutating persistent profiles.
- [ ] Add Lobby and distinct user↔agent Private Chats.
- [ ] Add integrated approval requests and decisions to workspace UI and conversation history.
- [ ] Add safe workspace/agent statistics, activity, unread/last-seen state, and selected-channel persistence.
- [ ] Add the modern WinForms workspace surface and public workspace facade.
- [ ] Add Example controls/tests for create, show/open, hide, close UI, agent join/leave, lobby chat, private chat, approvals, persistence, and restart restoration.
- [ ] Verify File, SQL Server, and MySQL workspace persistence and per-user isolation.

### Workspace foundation

`AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, `WorkspaceRouter`, `WorkspaceAgentRoleAssignment`, and `IWorkspaceRolePolicy` provide the provider-neutral communication foundation. Unaddressed user messages target only the active workspace default recipient. Explicit user addressing and explicit agent delegation target only the requested participant unless role policy allows the delegation. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

### User identity

The host supplies a stable `UserId`, display identity, and `IsAdmin` flag. The identity is used for workspace ownership, persistence partitioning, and host authorization policy. `IsAdmin` is not itself permission to execute tools, access memory, or modify host business data.

Phase 0.10 initially provides one default workspace per user. The data model remains extensible to multiple named workspaces later.

### Workspace lifecycle

`Create` ensures the user's default workspace exists. `Open`/`Show` displays the UI. `Hide` hides it without changing state. `Close` closes the UI without deleting state. Destructive archive/deletion is a separate explicit operation.

The workspace is always hidden until explicitly opened by the host. There is no workspace auto-show behavior in this phase.

Closing the application or shutting down the computer must preserve explicitly persisted workspace state. Reopening with the same `UserId` restores the user's workspace from the configured HAgent storage backend.

### Conversations

The workspace has a shared Lobby and distinct private chats between the user and selected agents. Private chat history is not automatically visible to other agents. Visible messages identify their author and role. System and approval events are first-class workspace-visible events.

Unread/read and last-seen state is part of workspace UX state so a returning user can resume where they stopped.

### Agent/provider selection

The workspace/application may configure default manager/coordinator, default specialist, default provider/model, and default approval policy. Users may switch an allowed agent/provider/model for a workspace conversation or private chat. These choices are runtime/workspace selection state or execution overrides and must not silently mutate the stored `AiAgent` profile.

### Approval integration

Approval requests identify the requesting agent/execution, bounded operation description, available decision options, and lifecycle state. Approval requests and their resolutions are persisted as workspace state when persistence is enabled. Approval configuration is a default policy only and never bypasses HAgent authorization.

### Persistence boundary

Persist workspace metadata, stable user/workspace ownership, participant membership/roles/lifecycle state, Lobby and private-chat history, approval state, safe activity/statistics, unread/last-seen state, selected workspace UX state, and explicit shared-memory records according to policy.

Do not persist provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, or temporary execution objects. Those exclusions are already established by Phase 0.95.

Agent-private memory remains private unless explicit shared-memory policy grants workspace visibility.

### UI/API boundary

HAgent exposes a provider-neutral workspace facade for create/open/show/hide/close, agent join/leave, Lobby messaging, private-chat access, approval interaction, and state observation. Hosts do not manipulate internal WinForms controls directly.

The WinForms surface is an optional HAgent UI implementation. The target design is compact, modern, professional, and collaboration-focused rather than a dashboard. It includes Lobby, participant/agent selection, private-chat access, approvals, message composition, clear author identity, and explicit state/lifecycle controls.

### HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public workspace/runtime APIs. HAgent does not add HWorld-specific dependencies, world types, physics, rendering, simulation scheduling, or action authority.

## Verification rule

A 0.10 slice becomes complete only after its implementation exists, its matching Example verification passes locally, and the authoritative documentation reflects the result.
