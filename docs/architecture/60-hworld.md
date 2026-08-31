# HWorld integration

HWorld is a reference external consumer of HAgent, not a dependency of HAgent.

HWorld owns:

- world state and entities;
- physics and collision;
- simulation time;
- sensors and observations;
- world scheduling;
- action validation and world-side effects;
- rendering and generational rules.

HAgent owns generic agent infrastructure:

- provider/model execution;
- agent profiles and runtime instances;
- context construction;
- tools and tool routing;
- memory integrations;
- cancellation, timeout, lifecycle and telemetry;
- future workspace/coordination features.

The integration is:

```text
HWorld observation
      -> HWorld/HAgent adapter
      -> runtime agent instance
      -> provider/model and optional tools/memory
      -> structured decision/action request
      -> HWorld validation
      -> world state
```

The HWorld adapter belongs in HWorld. No HWorld types belong in HAgent.Core.

HWorld can use HAgent before the business-application workspace/chat features are complete. The minimum integration requirement is a runtime agent instance that accepts caller-supplied context, executes asynchronously, supports cancellation/timeout/correlation, and returns provider-neutral structured results.

The HWorld project currently keeps its core independent of HAgent and defines this external boundary explicitly.
