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

## System-prompt composition

System prompts are **additive layers**, not replacement values.

The current composition order is:

```text
Higher priority
    Provider policy
        ↓
    Agent profile
        ↓
    Runtime / execution additions
Lower priority
```

A layer may add instructions or restrictions for the layer below it, but it must not erase, replace, or contradict a higher-priority layer. Lower layers may add narrower constraints; they do not obtain authority to weaken an earlier layer.

The provider layer is included when `AiAgent.UseProviderSystemPrompt` is enabled. Disabling that layer is an explicit configuration choice; it does not turn the agent prompt into a replacement mechanism for another layer.

`SystemPromptLayer.Priority` provides deterministic composition order. Future runtime/context/workspace layers should use reserved priority ranges rather than inventing separate prompt-merging logic.

Prompt composition is not an authorization boundary. Permissions, authorization callbacks, approvals, budgets, and host-side validation remain authoritative outside model instructions.

## Execution

The host supplies a request/context snapshot. Runtime resolves the profile/provider, creates an execution snapshot, composes the applicable system-prompt layers, invokes the provider, normalizes the result, and reports lifecycle/usage metadata.

The host may schedule executions independently of application or simulation timing.

## Sessions

A session is conversation state. It is related to an agent runtime but is not the same concept as an agent profile or runtime instance.

## HWorld

HWorld uses runtime instances at its external cognition boundary. HWorld owns simulation time and scheduling; HAgent owns generic execution. HAgent must not require HWorld.
