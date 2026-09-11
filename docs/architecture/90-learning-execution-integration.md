# Learning Execution Integration

## Purpose

Slice 11 connects governed learned resources to real execution inputs without creating a second context, instruction, authorization, or observability architecture.

The boundary is:

```text
Authoritative learned resource
        ↓
Host/resource retrieval
        ↓
Canonical context admission + assembly
        ↓
ContextSnapshot

Authoritative learned resource
        ↓
Canonical instruction source
        ↓
AgentExecutionRequest.InstructionSources
        ↓
Existing runtime instruction composition

Authoritative execution runtime
        ↓
IExecutionObservationSource
        ↓
AiLearningExecutionObservationCollector
        ↓
IAiLearningObservationStore
        ↓
Later learning analysis / typed candidate creation
```

## Execution input integration

`AiLearningExecutionPreparation` is a provider-neutral preparation boundary. It accepts learned instruction sources and learned context retrieval sources supplied by the host/resource layer.

Instruction sources are validated and cloned, then returned for the existing `AgentExecutionRequest.InstructionSources` boundary. The preparation layer does not compose prompts, change instruction precedence, or treat prompt text as authority. The existing `AiInstructionComposer` and runtime remain responsible for final instruction composition.

Learned context retrieval sources are passed through the existing `ContextAssembler`, which already composes policy admission, capability gating, bounded retrieval, ranking/deduplication, and final compaction. The resulting `ContextSnapshot` is execution-owned and isolated from caller mutation.

This slice intentionally does not create hidden Knowledge or Skill repositories. Resource implementations remain responsible for retrieving authoritative resources and exposing them as canonical context/instruction source contracts.

## Authorization boundary

Learned resources are not authorized because a model requests them or because their content contains an instruction. Context admission uses the existing `IAiPolicyEngine` and effective resource-capability snapshot. Host authorization remains a separate boundary where required.

Instruction sources are provenance/trust representations, not authorization. Resource authorization must occur before exposing a protected resource to the execution preparation boundary.

## Runtime learning input

`IExecutionObservationSource` already publishes authoritative runtime facts such as retry, wait, recovery, fallback, and stale-result observations.

`AiLearningExecutionObservationCollector` adapts those facts into bounded `AiLearningExecutionObservation` records stored through `IAiLearningObservationStore`.

The observation record intentionally excludes prompts, responses, tool payloads, credentials, raw exceptions, and arbitrary host objects. It preserves only the bounded runtime fact exposed by the authoritative observation contract.

Observation capture does not create a learning candidate and does not promote or mutate any authoritative resource. Later learning analysis decides whether observations become typed candidates through the existing Learning Policy and Lifecycle boundaries.

## Persistence boundary

`InMemoryAiLearningObservationStore` is the deterministic reference implementation for this slice. It is thread-safe, bounded by a configurable capacity, supports filtered queries, and returns detached observation copies.

A future persistent observation store must preserve the same provider-neutral contract and ownership/redaction boundaries without introducing a second event or audit model.

## Concurrency and lifecycle

The observation collector subscribes/unsubscribes explicitly from the runtime observation source and can be disposed independently of the runtime. It does not own runtime lifecycle, cancellation, retry, fallback, or terminal-state decisions.

The execution preparation boundary accepts caller-owned sources but clones instruction sources and relies on the canonical context assembly clone/snapshot path before execution.

## Explicit exclusions

This slice does not add automatic model-driven candidate generation, authoritative promotion, management UI, a new policy engine, a new prompt composer, or a persistent Knowledge/Skill repository. Those remain separate architecture boundaries or later work.
