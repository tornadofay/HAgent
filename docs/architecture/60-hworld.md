# HWorld integration boundary

HWorld is a reference external consumer of HAgent, not a dependency of HAgent.

## Ownership boundary

HWorld owns:

- world state and entities;
- physics and collision;
- simulation time;
- cameras, sensors, and observations;
- world scheduling;
- action validation and world-side effects;
- rendering;
- generational and population rules.

HAgent owns generic agent infrastructure:

- provider/model execution;
- reusable agent profiles and live runtime instances;
- caller-supplied context construction;
- memory integrations;
- structured tools and tool routing;
- asynchronous execution lifecycle;
- cancellation, timeout, correlation, and stale-result protection;
- usage/execution telemetry;
- future generic workspace and coordination features.

Neither side should absorb the other's domain responsibilities.

## Integration flow

```text
HWorld observation/event
        -> HWorld/HAgent adapter
        -> HAgent runtime agent instance
        -> provider/model and optional tools/memory
        -> provider-neutral decision/action request
        -> HWorld validation
        -> world state
```

The adapter belongs in HWorld. No HWorld types, world rules, physics, rendering, or simulation-time concepts belong in `HAgent.Core`.

## Runtime agents in HWorld

HWorld should keep reusable HAgent profiles separate from live actor instances:

```text
configured agent profile
        |
        +-- actor instance A
        +-- actor instance B
        +-- actor instance C
```

Each runtime instance may have an independent provider/model, prompt/context, memory owner, tools, execution state, and observation history. An actor does not require a permanent HAgent configuration record merely because it exists at runtime.

Different actors may use different providers/models at the same time, and some actors may use a deterministic non-LLM controller instead of HAgent.

## Async and stale-result requirements

HWorld simulation time must continue while HAgent waits for provider/model work. HAgent therefore needs non-blocking execution with cancellation, timeout, correlation, and stale-result protection.

The host may attach an observation/context version or other correlation value so a late decision cannot be applied to a newer world state accidentally.

## Observation boundary

HWorld decides what an actor is allowed to perceive. HAgent receives only the supplied observation/context. HAgent must not assume a complete world snapshot or request hidden world state through an implicit API.

Observation objects/snapshots are caller-owned inputs and should be treated as immutable for the duration of an execution.

## Tools and actions

HWorld may expose movement, perception, inspection, inventory, interaction, or other capabilities as generic structured HAgent tools. HAgent must not contain HWorld-specific tool names or world logic.

HWorld validates and applies any resulting action/tool effect because HWorld remains authoritative over simulation state.

An HAgent tool result is an intent/request or observation, not permission to mutate the world directly.

## Memory

HWorld may provide world-specific memory, knowledge, or skill stores. HAgent may provide generic memory/context facilities, but must not impose HWorld semantics such as generations or population rules.

Runtime-agent memory ownership must remain independent so multiple actors created from the same profile do not automatically share private memories.

## When HWorld can start using HAgent

HWorld does not need the business-application workspace/chat features before integration.

The minimum useful gate is a runtime agent instance that can:

- accept caller-supplied context/observation;
- execute asynchronously without blocking simulation time;
- support cancellation and timeout;
- return provider-neutral responses or structured tool/action requests;
- expose execution correlation needed for stale-result protection;
- maintain independent runtime identity and memory ownership.

The roadmap therefore treats the 0.9 runtime-instance milestone as the first HWorld integration point.

## Compatibility

The HWorld core must remain runnable without HAgent. The integration should be implemented at HWorld's external cognition/decision boundary. HAgent must remain a separately usable library.

## Non-negotiable boundary

Never:

- add HWorld references to `HAgent.Core`;
- add HWorld physics/rendering/simulation-time/world entities to HAgent;
- require HAgent for HWorld to run;
- allow an LLM to mutate authoritative HWorld state directly;
- treat an HWorld observation as trusted authorization.
