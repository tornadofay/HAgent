# Workspaces and routing

## Workspace

`AgentWorkspace` is a provider-neutral collaboration context. It has its own workspace identity and contains explicit participants; it is independent of WinForms, providers, and persistent agent profiles.

## Participants

A `WorkspaceParticipant` represents either a user or a runtime agent. Participant identity is separate from persistent agent profile identity and runtime instance identity.

Agent participants may reference a `ProfileId` and `RuntimeInstanceId`. Participant lifecycle is explicit through `Active`, `Suspended`, and `Retired` states.

## Default recipient

A workspace may define one default recipient. An unaddressed user message is routed only to that active default agent. The default recipient is not an implicit broadcast mechanism.

Explicit user-to-agent addressing must name an active agent participant. Agent-to-agent delegation must name an active agent participant explicitly.

Broadcast is not part of the base routing operation and requires a later explicit host policy.

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

The base router enforces participant existence, participant kind, active lifecycle state, and explicit recipient targeting. A host may compose it with execution and persistence components later.

## Runtime relationship

A workspace contains participants; a participant may point to a live `AgentRuntimeInstance`. A runtime instance remains independently owned by the host/runtime layer and retains its own memory ownership, execution lifecycle, cancellation, timeout, correlation, and stale-result semantics.

Workspace membership must not mutate the persistent `AiAgent` profile.

## HWorld and external consumers

HAgent has no HWorld-specific workspace or routing dependency. HWorld or another host can reference HAgent and use the public workspace/runtime APIs according to its own application policy. HAgent does not own world state, simulation scheduling, or host-side actions.
