# Phase 0.10 — Workspaces, Routing + Chat

## Goal
Provide an optional shared conversation where one authenticated host user and multiple runtime agents can visibly work together while every model request is routed deliberately and the user's workspace state survives application restarts.

## Steps

1. [x] Introduce a workspace abstraction independent of WinForms.
2. [x] Register users and runtime-agent participants with explicit lifecycle state.
3. [x] Define one workspace default recipient for unaddressed user messages.
4. [x] Define direct user-to-agent addressing.
5. [x] Define addressed agent-to-agent delegation and responses.
6. [x] Define coordinator/specialist behavior as a role/policy over generic runtime agents. `WORKSPACE ROLES` Example verification complete.
7. [ ] Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. [x] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. [ ] Execute routed workspace messages through runtime agents and make agent-to-agent work visible in the workspace lobby when enabled.
10. [ ] Add configurable addressing syntax at the host/UI layer without making prompt text the authoritative routing mechanism.
11. [ ] Add loop protection and collaboration budgets.
12. [ ] Add optional persistent workspace state and explicit shared-memory policy.
13. [ ] Add the WinForms workspace surface and global agent selection.
14. [ ] Add stable host user identity input, including `UserId` and `IsAdmin`, with database-safe user/workspace partitioning.
15. [ ] Add create/open/show/hide/close workspace lifecycle APIs where UI close never destroys persisted workspace state.
16. [ ] Persist user-owned lobby chat, private-agent chat, participant membership/roles, approval state, safe statistics, and selected workspace UX state according to storage policy.
17. [ ] Add user-facing workspace configuration with a single `Enable Workspace` setting; workspace remains hidden until explicitly opened.
18. [ ] Add default manager/coordinator agent configuration.
19. [ ] Add default specialist agent configuration and specialist responsibility metadata.
20. [ ] Add workspace/private-chat provider and agent selection/override controls without mutating persistent agent profiles.
21. [ ] Add configurable default approval type/policy and integrate approval requests/resolution into workspace UI and conversation history.
22. [ ] Add professional modern WinForms UI with lobby chat, participant/agent list, private chats, approval presentation, and explicit open/close/show/hide controls.
23. [ ] Add Example controls and tests for create, open/show, hide, close UI, agent join/leave, lobby chat, private chat, approval flow, persistence across restart, and user/workspace restoration.
24. [ ] Verify SQL Server, MySQL, and File workspace persistence and user partitioning.

### Current foundation

The provider-neutral foundation contains `AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter`. Participants are either users or runtime agents and have explicit Active/Suspended/Retired state. An active default recipient may be defined for unaddressed user messages. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

`IWorkspaceRolePolicy` and `WorkspaceRolePolicy` allow coordinator/specialist behavior to be expressed as policy over ordinary agent participants. `WorkspaceAgentRoleAssignment` describes `Participant`, `Coordinator`, or `Specialist` role, optional responsibility metadata, user-message eligibility, and allowed delegation target roles. No separate coordinator/specialist agent classes are introduced. The `WORKSPACE ROLES` Example verification confirms the policy works.

The workspace user identity contract must carry a stable `UserId` and an `IsAdmin` flag supplied by the host. The identity is an input to workspace authorization and persistence partitioning; `IsAdmin` does not itself grant tool, memory, data, or host-business permissions.

Phase 0.10 initially provides one default persisted workspace per user. The model should remain extensible to multiple named workspaces later without forcing a second workspace into the first implementation.

## Workspace lifecycle

The workspace UI is hidden until explicitly opened by the host. There is no automatic-show workspace behavior in Phase 0.10.

`Create`/ensure obtains the user's default workspace. `Open`/`Show` makes the UI visible. `Hide` hides it without changing workspace state. `Close` closes the UI without deleting workspace state. Destructive archive/deletion is a separate explicit operation and is not implied by closing the UI.

Application shutdown or computer shutdown must not cause user work to disappear. Reopening the application with the same stable `UserId` restores the persisted workspace state from the selected HAgent storage backend.

## Conversations

The workspace contains a shared Lobby conversation where the user and joined agents can visibly communicate. It also contains distinct Private Chats between the user and selected agents. Private chat content is not automatically exposed to other workspace participants.

Visible messages identify their author and role clearly. System and approval events are first-class workspace-visible events alongside ordinary conversation messages.

## Agent configuration

The host/application administrator can configure default manager/coordinator and specialist agents for the workspace. A specialist has descriptive responsibility metadata that can represent a domain, table, subsystem, process, capability, or other host-owned responsibility without requiring HAgent-specific domain classes.

The workspace UI can let a user switch the active provider/model or selected agent for an allowed conversation or private chat. These are execution/runtime selections and do not silently mutate the stored `AiAgent` profile.

The default approval type/policy is a workspace/host policy default. Approval handling remains subject to HAgent authorization and does not bypass permission checks.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit role policy allows the sender's role to delegate to the recipient's role.
- Broadcast: explicit opt-in operation, never the default.
- Agent-to-agent routed work becomes visible in the Lobby when workspace execution policy permits it.

The authoritative routing decision is represented by workspace messages and routing APIs; human-friendly addressing syntax is a UI/host convenience.

## Persistence

Persisted workspace state is partitioned by host application identity and the stable `UserId`. File storage remains local to the host installation; SQL Server and MySQL must prevent users in the same host application from reading or mutating another user's workspace state unless an explicit host/admin policy permits it.

Persisted workspace state includes workspace metadata, participant membership/roles/lifecycle state, lobby and private-chat history, approval requests/resolution state, safe statistics/activity metadata, selected workspace UX state where appropriate, and explicit workspace/shared-memory records.

Provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, and temporary execution objects are not workspace persistence concerns; the generic runtime phase already establishes those boundaries.

Agent private memory remains private unless explicit shared-memory policy grants workspace visibility.

## Workspace UI

The WinForms workspace is a compact, professional collaboration surface rather than a large dashboard. It contains a Lobby, participant/agent selection, access to private chats, approval presentation, and message composition. It must provide clear authorship and state without exposing implementation details.

The host interacts with the workspace through a public HAgent workspace facade rather than direct manipulation of internal WinForms controls. The facade provides lifecycle and communication operations conceptually equivalent to create, open/show, hide, close, agent join/leave, lobby send, private-chat open/send, and workspace state observation.

## Example verification

The Example application will expose explicit controls for creating, showing/opening, hiding, and closing the workspace UI, plus controlled agent join/leave and communication tests. Verification must confirm that closing the UI and restarting the application preserve the user's lobby/private chats, membership, approvals, selected state, and safe statistics for File, SQL Server, and MySQL storage.

## HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public runtime/workspace APIs. HAgent does not add an HWorld-specific dependency, adapter, world type, physics, simulation scheduling, or action authority.

## Exit criterion

A host can identify a user, create/open the user's persisted default workspace, show and close the UI without losing work, join runtime agents, conduct visible lobby and private conversations, configure/select permitted manager/specialist agents and provider/model overrides, present and resolve approvals, and restore the same state after application restart while routing remains bounded and traceable.
