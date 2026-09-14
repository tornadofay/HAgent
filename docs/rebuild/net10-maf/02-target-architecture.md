# Target Architecture — .NET 10 + MAF

## 1. Target solution shape

The preferred target is a clean .NET 10 solution. Project names may change during Phase 1 if the audit shows a better split, but responsibilities should converge toward this model.

```text
HAgent.slnx
|
+-- HAgent.Core
|     HAgent-specific contracts and cognitive runtime
|
+-- HAgent.Maf
|     HAgent-to-MAF integration and adapters
|
+-- HAgent.Providers.*
|     Provider-specific configuration/discovery/admission integration
|
+-- HAgent.Storage.File
+-- HAgent.Storage.SqlServer
+-- HAgent.Storage.MySql
|
+-- HAgent.WinForms
|
+-- HAgent.Example
+-- HAgent.Tests
|
+-- samples/HAgent.ExternalConsumer
```

A separate `HAgent.Maf` assembly is preferred initially because it creates a clean migration seam. If Phase 1 proves that some MAF abstractions can safely become direct Core dependencies without damaging provider/framework neutrality, that choice becomes an explicit decision rather than an accidental coupling.

## 2. Core dependency direction

```text
                  HAgent.Core
                       |
          +------------+------------+
          |                         |
   HAgent-specific             contracts
   cognition/runtime                |
          |                         |
          +------------+------------+
                       |
                HAgent.Maf
                       |
           Microsoft Agent Framework
                       |
          Microsoft.Extensions.AI
                       |
                  providers
```

No host project, WinForms project, SQL implementation, or provider transport should flow back into `HAgent.Core`.

## 3. HAgent Core

`HAgent.Core` should own only the architecture that is still distinct after the MAF audit.

### 3.1 Identity

Retain the existing canonical identity model for deployment, tenant, principal, user, session, workspace, agent profile, runtime instance, and execution where each is actually required.

### 3.2 Runtime instance

Retain the HAgent runtime-instance abstraction because it is the owner of HAgent-specific cognitive state, runtime overrides, lifecycle, and private resource ownership.

MAF sessions are not automatically equivalent to an HAgent runtime instance. Phase 1 must map these concepts explicitly rather than collapsing them simply because both represent "state."

### 3.3 Cognitive Kernel

Retain the stable Cognitive Kernel plus replaceable Cognitive Strategies. The kernel must not depend on an individual model, MAF agent class, or provider.

MAF should be used for actual agent/model execution when a strategy requires language-model reasoning.

### 3.4 Execution Planner

Retain the HAgent execution planner for concrete execution-target choice where it considers factors outside a generic MAF workflow, including capability, authorization, operational state, quota, concurrency, cost state, health, and runtime policy.

The planner can output an MAF-compatible execution mechanism rather than invoking providers directly.

### 3.5 Memory / Knowledge / Skills / Learning

Retain HAgent resource governance. Use MAF context providers, agent skills, tool/function infrastructure, and storage where they satisfy the relevant subcontracts.

HAgent remains authoritative for:

- ownership and scope;
- profile/runtime override resolution;
- resource lifecycle;
- provenance;
- learning candidates;
- promotion and rejection;
- resource versioning;
- policy-governed publication.

### 3.6 Policy

Retain the HAgent policy engine for HAgent authority boundaries. Remove generic pipeline checks that are directly replaced by MAF middleware.

Policy must remain an enforcement mechanism and not merely prompt text.

## 4. HAgent.Maf adapter

This project is the primary anti-duplication seam.

It should contain:

- conversion between HAgent execution snapshots and MAF run/session options;
- construction of MAF agents/workflows from HAgent configuration;
- integration of HAgent policy and lifecycle gates into MAF middleware/hooks where appropriate;
- mapping of MAF responses and events to HAgent provider-neutral result contracts;
- HAgent-specific state propagation through MAF runtime context;
- durable-state coordination where HAgent and MAF persistence both participate;
- adapters for MAF tools, skills, context providers, and workflow components.

It should not copy MAF implementation logic. The adapter should translate contracts and compose components.

## 5. Provider architecture

The lower-level model contract should prefer Microsoft.Extensions.AI where suitable.

```text
HAgent Provider Registry
       |
       v
Execution Planner
       |
       +--> selected execution target
                 |
                 v
             IChatClient
                 |
                 v
          MAF ChatClientAgent
          or workflow component
                 |
                 v
              provider
```

The current HAgent provider-neutral response model must be reviewed against `ChatResponse`, contents, tool calls/results, usage, structured output, and provider metadata. Anything that exists only to compensate for a lower-level abstraction already standardized by MAF/MEAI should be deleted.

## 6. Persistence architecture

HAgent-owned persistence remains necessary for HAgent-specific state.

The rebuild should distinguish:

```text
MAF/application session/workflow state
              |
              +-- use MAF persistence where appropriate

HAgent authoritative resource/cognitive state
              |
              +-- HAgent storage contracts
```

No duplicate durable representation should exist merely because both layers can serialize similar objects.

The exact persistence ownership split must be settled during Phase 2 after actual MAF persistence APIs are inspected for current version semantics.

## 7. Lifecycle model

HAgent lifecycle must remain authoritative for HAgent runtime instances.

MAF agent/workflow execution runs inside an HAgent lifecycle boundary:

```text
HAgent lifecycle admission
        |
        v
Create immutable execution snapshot
        |
        v
Construct / invoke MAF execution
        |
        +-- MAF pipeline
        +-- model/tool/workflow work
        |
        v
HAgent terminal-state arbitration
        |
        v
Persist / publish result
```

A late MAF/provider completion must not overwrite an HAgent terminal state such as cancellation, timeout, retirement, or shutdown.

## 8. Scheduling and concurrency

Host scheduling remains outside HAgent unless explicitly represented through a generic host-controlled contract. HAgent provides admission/lifecycle safety; MAF provides agent/workflow execution mechanics.

Independent runtime instances must remain isolated. Shared MAF services are reusable only where their contracts are thread-safe and do not accidentally share HAgent-owned mutable state.

## 9. WinForms

`HAgent.WinForms` remains HAgent-owned.

The rebuild should preserve the existing UI management and UI Context / Control Adapter goals, but remove any internal UI code that duplicates MAF management surfaces unless the surface is specifically HAgent management rather than general agent orchestration.

The UI must expose HAgent state and effective configuration, not raw MAF internals as the project's primary model.

## 10. HWorld

HWorld remains external to HAgent.

```text
HWorld
  world state / sensors / time / validation
              |
              v
          HAgent Runtime
              |
              v
        MAF execution layer
              |
              v
            Model
```

HAgent must not require HWorld, and HWorld must not become an HAgent implementation dependency.

## 11. API policy

The rebuild should prefer a small HAgent-owned public surface around distinctive concepts and delegate implementation to MAF internally.

Where the owner chooses direct MAF contracts as public API, the decision must be recorded because it is a deliberate reduction in HAgent abstraction ownership.

## 12. Future MAF replacement rule

Any HAgent adapter added because MAF currently lacks an HAgent requirement must be a replaceable implementation behind a capability boundary.

This rule applies particularly to:

- cognitive state persistence around MAF sessions;
- richer runtime lifecycle semantics;
- governed learning promotion;
- HAgent execution-target planning;
- HAgent-specific resource governance;
- stronger recovery/idempotency behavior.

A future MAF feature can replace the implementation without changing the HAgent contract when semantics match.
