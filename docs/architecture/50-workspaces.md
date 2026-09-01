# Workspaces and routing

## Workspace

`AgentWorkspace` is a provider-neutral collaboration context. It has its own workspace identity and contains explicit participants; it is independent of WinForms, providers, and persistent agent profiles.

Phase 0.10 initially provides **one default workspace per host user**. The data model must remain capable of supporting multiple named workspaces later without changing the user-facing contract.

A workspace is user-owned state. For database-backed hosts, persisted workspace state is partitioned by the host application's identity and the supplied stable `UserId`. File storage remains naturally isolated to the host installation, while the same user/workspace identity rules still apply within that installation.

The host must provide a stable user identity and whether that user is an administrator (`IsAdmin`). `IsAdmin` is identity metadata used by workspace/host authorization policy; it is not itself permission to invoke tools, access memory, or mutate host data.

## Workspace lifecycle

Workspace visibility is separate from workspace lifetime.

- `Create` creates or ensures the user's default workspace exists.
- `Open`/`Show` displays the workspace UI.
- `Hide` makes the UI invisible without changing persisted workspace state.
- `Close` closes the workspace UI without deleting user state.
- A later explicit administrative/archive operation may remove or archive workspace state; closing the UI must never implicitly destroy it.

The default behavior is **hidden until explicitly opened by the host**. There is no configurable automatic-show behavior in Phase 0.10.

Closing the application or shutting down the computer must not lose persisted workspace conversations, private chats, participant membership, workspace state, approval state, or other explicitly persisted user-owned HAgent state. On a later application launch, the host supplies the same stable `UserId` and HAgent restores the user's workspace state from the selected persistence backend.

## Participants

A `WorkspaceParticipant` represents either a user or a runtime agent. Participant identity is separate from persistent agent profile identity and runtime instance identity.

Agent participants may reference a `ProfileId` and `RuntimeInstanceId`. Participant lifecycle is explicit through `Active`, `Suspended`, and `Retired` states.

Agents may join or leave a workspace through the public workspace API. Joining a workspace must not mutate the persistent `AiAgent` profile.

## Agent roles and defaults

Coordinator and specialist behavior is policy over ordinary runtime-agent participants, not separate agent classes.

Workspace configuration may designate:

- a default manager/coordinator agent for unaddressed lobby messages;
- one or more configured/default specialist agents and their responsibility metadata;
- default provider/model selection for workspace-created or host-selected runtime usage where the host permits it;
- a default approval policy/type used when an operation requires user approval.

The workspace may expose provider and agent selection controls to the user. Such selection changes runtime/workspace selection state or creates execution overrides; it must not mutate the persistent agent profile unless the host explicitly invokes configuration management.

A private chat with a specific specialist may have its own selected agent/provider/model according to host policy. The selected provider is an execution configuration choice, not a replacement for the agent's persistent profile identity.

## Default recipient

A workspace may define one default recipient. An unaddressed user message is routed only to that active default agent. The default recipient is not an implicit broadcast mechanism.

Explicit user-to-agent addressing must name an active agent participant. Agent-to-agent delegation must name an active agent participant explicitly.

Broadcast is not part of the base routing operation and requires a later explicit host policy.

## Workspace conversations

The workspace contains a shared **Lobby** conversation where the user and participating agents can visibly communicate.

The workspace also supports **Private Chats** between the user and a selected agent. Private chat history is distinct from lobby history and must not become visible to other agents merely because those agents are workspace participants.

The UI should clearly identify who authored each visible message, for example `User`, `Coordinator`, `Data Specialist`, and `Validation Specialist`, without requiring prompt-text addressing conventions.

System/approval events are first-class workspace-visible events alongside ordinary chat messages.

## Workspace messages

`WorkspaceMessage` preserves:

- workspace ID;
- message ID;
- sender and recipient IDs;
- message kind;
- correlation ID;
- causation ID;
- monotonic workspace sequence;
- message timestamp.

Correlation identifies the broader operation. Causation identifies the message or event that directly caused a later message.

## Routing boundary

`IWorkspaceRouter` performs routing decisions only. It does not invoke providers, execute agents, persist conversations, or perform host-side side effects.

The base router enforces participant existence, participant kind, active lifecycle state, and explicit recipient targeting. A host may supply an optional `IWorkspaceRolePolicy` to impose coordinator/specialist routing rules without changing the participant or runtime-agent types.

## Workspace execution

Workspace message execution is a later layer over routing. Once enabled, the workspace coordinator/default manager or explicitly addressed specialist is invoked through the normal HAgent runtime and provider pipeline. Agent-to-agent work may appear as visible lobby messages according to workspace execution policy.

Workspace execution must retain the existing execution/runtime/tool correlation model. It must not create a second execution engine inside the workspace subsystem.

## Approval integration

Approval requests belong to the workspace UI and conversation model when workspace approval handling is enabled.

An approval request identifies the requesting agent/execution, the requested operation, a bounded human-readable explanation, available decision options, and approval lifecycle state. Approval resolution must be correlated to the originating operation and persisted as user-owned workspace state when persistence is enabled.

The configured default approval type/policy is a workspace/host policy default, not a bypass of HAgent authorization. A denied or expired approval must not permit the underlying operation.

## Persistent workspace state

Persisted workspace state may include:

- workspace metadata and stable workspace identity;
- stable host `UserId` and `IsAdmin` identity metadata as appropriate to the host policy;
- participant membership, role assignments, and lifecycle state;
- lobby and private-chat conversations;
- selected channel/agent where the host chooses to persist that UX state;
- approval requests and resolution state;
- safe workspace/agent statistics and activity metadata;
- explicitly persisted workspace/shared memory according to memory-scope policy.

The persistence model must not store provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, or temporary execution objects. The same rule established by the generic runtime phase applies here.

Agent private memory remains private unless an explicit shared-memory policy grants workspace visibility.

## Workspace UI contract

HAgent should expose a host-facing workspace facade that provides operations conceptually equivalent to `Create`, `Show/Open`, `Hide`, `Close`, agent join/leave, lobby interaction, private-chat opening, and workspace-state observation. The host must not manipulate internal WinForms controls directly.

The WinForms workspace surface is an optional HAgent UI implementation, not the workspace domain itself. The intended visual design is a compact, professional, modern collaboration window rather than a large dashboard. The primary surface contains a Lobby conversation, participant/agent selection, private-chat access, approval presentation, and message composition.

The UI remains hidden until explicitly opened. Closing the UI preserves the workspace and all persisted state.

## Runtime relationship

A workspace contains participants; a participant may point to a live `AgentRuntimeInstance`. A runtime instance remains independently owned by the host/runtime layer and retains its own memory ownership, execution lifecycle, cancellation, timeout, correlation, and stale-result semantics.

Workspace membership must not mutate the persistent `AiAgent` profile.

## HWorld and external consumers

HAgent has no HWorld-specific workspace or routing dependency. HWorld or another host can reference HAgent and use the public workspace/runtime APIs according to its own application policy. HAgent does not own world state, simulation scheduling, or host-side actions.
