# Architectural Decisions

This file contains only durable decisions needed to preserve project direction across sessions. It is not a conversation log.

## D-001 — Complete known architecture before implementation

**Status:** Active

Substantial features must be implemented against the complete intended architecture that can reasonably be derived from the repository, roadmap, architecture documents, and requirements. Do not deliberately create simplified temporary implementations when the required design is already understood.

Testing is primarily for verification, defect discovery, and genuinely unforeseen interactions. It must not be used as a substitute for architectural analysis that should have happened before implementation.

## D-002 — Repository documents are persistent project memory

**Status:** Active

Small purpose-specific Markdown documents preserve durable project state so development can resume without relying on conversation history. They contain compressed conclusions, current state, active work, and durable decisions—not transcripts or unrestricted reasoning.

## D-003 — No duplicate sources of truth

**Status:** Active

Each durable fact should have one authoritative source. Other documents should reference that source rather than copying the same architectural decision. Generated root documents remain generated views.

## D-004 — Active work is state, not history

**Status:** Active

Current unfinished work is represented by the compact active-work document. When work advances, update the current state instead of appending a chronological diary. Completed work is removed from active state once it is reflected in the appropriate authoritative project document.

## D-005 — Unforeseen discoveries may refine architecture

**Status:** Active

Testing and implementation may reveal requirements or interactions that could not reasonably be known beforehand. Such discoveries should produce an explicit architectural decision or update to the authoritative source rather than an undocumented workaround.

## D-006 — Model-assisted evaluation uses an injected judge boundary

**Status:** Active

Model-assisted evaluation must not make `HAgent.Core` a provider router or a model-specific grading client. `IAiEvaluationJudge` is the provider-neutral boundary for model judging, and `AiModelAssistedEvaluationEvaluator` is the `IAiEvaluator` adapter that maps a bounded judge rating into `AiEvaluation` evidence.

The evaluator owns detached per-call request snapshots, validation, cancellation checks, evidence ownership, and evaluator/judge provenance. Provider transport, credentials, model selection, retries, and host-specific evidence resolution remain inside the injected judge implementation or its owning subsystem.

Model-assisted output is explicitly non-authoritative. It can produce `Passed`, `Failed`, `Inconclusive`, or `NeedsReview`, but it never grants authorization, changes configuration, promotes learning, or mutates authoritative cognitive state by itself.

## D-007 — Evaluation aggregation is measurement-only

**Status:** Active

Evaluation aggregation and alternative-target comparison remain provider-neutral measurement operations over bounded evaluation evidence. They may group samples, compute outcome/quality/operational metrics, compute left-minus-right deltas, and identify a strictly preferred variant according to an explicit metric direction.

A `PreferredVariantId` is comparative evidence, not a routing instruction, authorization decision, policy decision, configuration change, learning promotion, or authoritative cognitive mutation. Aggregation does not invoke providers, resolve credentials, persist authoritative state, or execute regression suites. Later regression-suite orchestration must consume these contracts rather than introduce a parallel metric/result model.

## D-008 — Regression suites orchestrate host-owned execution and evaluation only

**Status:** Active

Evaluation regression suites are provider-neutral orchestration contracts. A suite owns bounded case definitions, alternative target descriptors, and a concurrency limit; an injected `IAiEvaluationRegressionExecutor` owns how a specific case is executed for a specific target and returns the existing `AiEvaluationSample` evidence contract.

The regression runner may repeat every case across every configured target, bound concurrent executions, isolate executor failures as case results, propagate cancellation, discard late results after cancellation, and deterministically hand successful samples to the existing aggregation/comparison layer. It does not route production execution, authorize actions, select providers or credentials, persist authoritative state, or introduce a second metric/result model.

Failure and cancellation are kept distinct from evaluation outcomes: an execution that cannot produce a valid evaluation is recorded as a regression case failure/cancellation and does not become a fabricated `AiEvaluation`. Regression results remain measurement evidence only.

## D-009 — Resource governance composes canonical ownership, capability, and policy

**Status:** Active

Mature resource admission must compose the existing canonical `AgentResourceOwnership`, effective `AiResourceCapabilitySnapshot`, and unified `IAiPolicyEngine` rather than creating subsystem-specific ownership or authorization models. Non-global resource requests require the authoritative resource owner ID and must match the identity-derived owner; capability state is evaluated from an immutable profile/runtime snapshot; policy remains the final authorization boundary.

The generic `AiResourceGovernanceEvaluator` is reusable across Skills, Knowledge/Wiki, Memory families/types, and future resource types. Disabled capability, ownership mismatch, policy denial, approval requirement, deferral, and non-applicable policy all result in non-admitted resource access. Effective capability source (`Default`, `Profile`, `RuntimeOverride`) is provenance for configuration diagnostics, not authority.

## D-010 — Skills are versioned descriptive resources with governed execution snapshots

**Status:** Active

`AiSkillDefinition` is the canonical provider-neutral representation of one reusable Skill version. Its stable identity is `SkillId + Version`, with explicit `AgentResourceScope`, owner, lifecycle state, provenance, bounded input/output contracts, preconditions, ordered procedure steps, required Knowledge/Tool dependencies, constraints, metadata, and relationships.

`AiSkillReference` identifies a reusable definition by Skill ID plus explicit scope/owner and an optional version pin. `AiSkillSet` is a bounded collection of these references and may be reused by multiple agent profiles without duplicating Skill definitions. Agent profiles own the references through `AiAgent.Skills`; the references do not themselves grant authorization.

Skill access must reuse `AiResourceGovernanceEvaluator`. The canonical `skill.invoke` resource boundary is evaluated before the definition source is accessed, using the reference's explicit scope/owner, identity, profile/runtime/execution context, and effective capability state. There is no parallel Skill authorization engine.

`IAiSkillDefinitionSource` is a provider/storage-neutral read boundary. `AiGovernedSkillResolver` composes that source with the canonical governance evaluator and admits only definitions whose ID, scope, owner, optional pinned version, and published lifecycle state match the requested reference.

Executable handlers are deliberately outside persisted Skill definitions and Skill sets. No provider SDK object, delegate, callback, or executable handler is serializable as Skill configuration. `AiSkillExecutionSnapshot` deep-clones admitted bindings, and `AgentExecutionSnapshot.Skills` owns that snapshot for the execution lifetime so later profile/source mutations cannot change the bound Skill version/state.

Slice 3 does not introduce a Skill-specific persistence backend, execution handler registry, learning-promotion workflow, management UI, or context-budget integration; those remain later roadmap boundaries.

## D-011 — Memory families use one extensible MemoryEntry contract

**Status:** Active

Memory remains one provider-neutral `MemoryEntry` persistence contract. The entry identifies its broad semantic family with `AiMemoryFamily` (`Working`, `Episodic`, `Semantic`, `Procedural`, or `Custom`) and its stable concrete type with a bounded `TypeId` namespace. Built-in families use reserved family prefixes; custom types use application-specific namespaces. Future memory types therefore do not require a new persisted class or provider-specific model.

`AiMemoryProvenance` carries source kind, source identity, optional source URI/creator information, originating execution/runtime IDs, evidence, and bounded confidence. Provenance is descriptive evidence only; model-generated memory is not automatically authoritative.

`MemoryEntry.ExpiresAt` is optional metadata and `IsExpired(...)` is a deterministic point-in-time helper. Retention enforcement remains a later governance concern. Historical records may have `OccurredAt` before `CreatedAt`.

`MemoryEntry.Validate()` owns structural bounds and family/type consistency but does not authorize access. `MemoryEntry.Clone()` deep-copies mutable metadata and provenance. Existing memory stores continue to use `MemoryEntry` as their single representation; no parallel memory repository is introduced by this slice.

## D-012 — Memory governance composes generic capability state with one provider-neutral policy boundary

**Status:** Active

Memory family/type access must reuse the existing `AiResourceCapabilityPolicy` and immutable `AiResourceCapabilitySnapshot`. Canonical Memory capability resource types are `memory`, `memory.family`, and `memory.type`; no Memory-specific authorization system is introduced.

`MemoryQuery` owns explicit family/type and expiration filters plus the caller's requested result bound. `AiMemoryGovernancePolicy` owns deterministic global/family/type retrieval limits and retention caps. Exact TypeId rules take precedence over family rules, and policy limits are bounded to 1000.

`AiMemoryGovernanceEvaluator` performs capability checks, while `AiGovernedMemoryStore` composes capability and policy enforcement around the existing `IMemoryStore` boundary. The decorator clones writes, applies the maximum retention expiration without extending a shorter explicit expiration, filters expired/unauthorized records during recall, and returns detached records. Physical storage remains the existing File/SQL Server/MySQL/InMemory implementations.

Memory governance is therefore separated into capability authorization, retrieval/retention policy, and storage. Runtime execution snapshot binding and management UI remain later integration slices.

## D-013 — Learning policy and typed candidates compose the existing lifecycle

**Status:** Active

`AiLearningCandidate` remains the single canonical learning lifecycle. `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` are typed payload contracts that compose an existing `AiLearningCandidate`; they do not define parallel status transitions or promotion state.

`AiLearningPolicy` is a deterministic, provider-neutral candidate admission/evaluation contract. Its rules may constrain candidate type/scope, minimum confidence, evidence, provenance, contradiction state, evaluation state, retention class, and the required promotion-authorization classification. An unmatched rule denies the candidate by default.

The learning policy is not a replacement for authorization. External promotion remains behind the existing `IAiPolicyEngine` / `AiLearningPromotionPolicy` boundary. Model-generated content may propose a candidate but cannot make Knowledge or Skills authoritative by itself.

Knowledge and Skill candidate payloads must remain non-authoritative (`Draft`). Candidate payloads are cloned at construction so caller mutation does not alter the captured proposal. Source execution/runtime/profile identity remains on the canonical candidate lifecycle object.
