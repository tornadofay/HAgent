# HWorld integration boundary

HWorld is a reference external consumer of HAgent, not a dependency of HAgent.

## Boundary

HWorld owns:

- world state and entities;
- physics and collision;
- simulation time;
- cameras and sensors;
- observations;
- world/action validation;
- world scheduling;
- rendering;
- generational and population rules.

HAgent owns generic agent capabilities:

- provider/model execution;
- agent profiles and runtime instances;
- context construction;
- memory integrations;
- tools and tool routing;
- execution lifecycle;
- cancellation/timeouts;
- multi-agent communication and coordination;
- usage and execution telemetry.

The integration therefore remains:

```text
HWorld observation/event
        -> HWorld/HAgent adapter
        -> HAgent runtime agent instance
        -> model/provider and optional tools/memory
        -> decision/action request
        -> HWorld validation
        -> world state
```

## Agent instances in HWorld

HWorld should normally keep its configured agent profiles reusable and create runtime instances for actual actors/experiments.

```text
configured HAgent profile
        |
        +-- actor instance A
        +-- actor instance B
        +-- actor instance C
```

Each runtime instance may have an independent provider/model, prompt/context, memory owner, tools, execution state, and observation history.

An HWorld actor does not need a permanent HAgent configuration record merely because the actor exists at runtime.

## Async requirement

HWorld simulation time must continue while HAgent is waiting for model/provider completion. HAgent therefore must expose non-blocking execution with cancellation, timeout, correlation, and stale-result protection.

## Observation requirement

HWorld decides what the actor is allowed to perceive. HAgent receives only the context supplied by HWorld. HAgent must not assume that a complete world snapshot is available or request hidden state through an implicit API.

## Tools

HWorld may expose movement, perception, inspection, inventory, interaction, or other capabilities as generic structured HAgent tools. HAgent must not contain HWorld-specific tool names or world logic.

HWorld validates and applies the resulting action/tool effect because HWorld remains authoritative over simulation state.

## Model diversity

The integration must permit different actors to use different providers/models/settings at the same time, and some actors may use a deterministic non-LLM controller instead of HAgent.

## Memory

HWorld may provide world-specific memory/knowledge/skill stores. HAgent may provide generic memory and context facilities, but must not impose HWorld semantics such as generational inheritance.

## Compatibility

The current HWorld core targets `netstandard2.0`, while its WinForms Example targets `net481`. HAgent must remain consumable from compatible host assemblies without requiring HAgent to become a dependency of HWorld.Core.

## What must never happen

- Add HWorld references to HAgent.Core.
- Add HWorld physics, rendering, simulation time, or world entities to HAgent.
- Require HAgent for HWorld to run.
- Let an LLM mutate HWorld state directly.
- Treat an HWorld observation as trusted authorization.
