# Runtime and agents

## Profile

`AiAgent` is persistent reusable configuration: identity, provider preferences, model, system prompt, generation settings, and tool references.

## Runtime instance

A runtime instance is the live execution identity created from a profile. It keeps profile identity separate from runtime identity and may carry host/context bindings, memory ownership, scope, and execution state.

Runtime instances must support:

- concurrent independent execution;
- cancellation and timeout;
- execution snapshots;
- stale-result protection;
- explicit retirement;
- optional persistence for recovery/collaboration.

A runtime instance is not automatically persisted as a configured agent.

## Scope

Scope describes where a runtime instance belongs. Planned scopes include Application, Workspace, Context/Form, Session, Task, and Ephemeral. Scope must not be encoded as different agent classes.

## Execution

The host supplies a request/context snapshot. Runtime resolves the profile/provider, creates an execution snapshot, invokes the provider, normalizes the result, and reports lifecycle/usage metadata.

The host may schedule executions independently of application or simulation timing.

## Sessions

A session is conversation state. It is related to an agent runtime but is not the same concept as an agent profile or runtime instance.

## HWorld

HWorld uses runtime instances at its external cognition boundary. HWorld owns simulation time and scheduling; HAgent owns generic execution. HAgent must not require HWorld.
