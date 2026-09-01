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

The base router enforces participant existence, participant kind, active lifecycle state, and explicit recipient targeting. A host may supply an optional `IWorkspaceRolePolicy` to impose coordinator/specialist routing rules without changing the participant or runtime-agent types.

## Coordinator and specialist roles

Coordinator and specialist are roles over ordinary agent participants, not separate agent classes.

`WorkspaceAgentRoleAssignment` can describe:

- `Participant` — normal workspace agent participation;
- `Coordinator` — an agent allowed to coordinate or delegate according to policy;
- `Specialist` — an agent assigned a bounded responsibility or specialty description.

The assignment may declare whether an agent can receive user messages and which target roles it may delegate to. `WorkspaceRolePolicy` evaluates these rules. The policy is optional; without one, the router preserves the permissive routing behavior established by the base workspace contract.

A specialist's `Responsibility` is descriptive metadata only. It does not force HAgent to model a specialist as a table, record, subsystem, or domain-specific class. Hosts may use that metadata to identify whole-domain or capability responsibilities.

## Runtime relationship

A workspace contains participants; a participant may point to a live `AgentRuntimeInstance`. A runtime instance remains independently owned by the host/runtime layer and retains its own memory ownership, execution lifecycle, cancellation, timeout, correlation, and stale-result semantics.

Workspace membership must not mutate the persistent `AiAgent` profile.

## HWorld and external consumers

HAgent has no HWorld-specific workspace or routing dependency. HWorld or another host can reference HAgent and use the public workspace/runtime APIs according to its own application policy. HAgent does not own world state, simulation scheduling, or host-side actions.
