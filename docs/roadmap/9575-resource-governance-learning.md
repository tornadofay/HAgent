# Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

Planned after Phase 0.957 Evaluation / Quality Measurement and before Phase 0.958 Agent Lifecycle / Health.

## Goal

Complete the **mature governance and learning layer** for HAgent's first-class Knowledge, Skills, Memory, and Learning resources.

The foundational resource model is established earlier through HAgent's internal storage/resource foundations. This phase does not introduce a second resource architecture and does not delay foundational resource contracts until this point. Instead, it makes those resources fully governed, reusable, scoped, observable, configurable, and administrable across profiles, runtime instances, executions, and learning workflows.

## Architectural position

HAgent treats these as four related but distinct first-class capabilities:

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed process that turns experience into typed candidates
            and, when permitted, promotes them into Memory, Knowledge, or Skills
```

The earlier resource foundation establishes canonical identity, scope, provenance, versioning, storage, and provider-neutral contracts. This phase establishes the mature governance model around those resources.

```text
Resource Foundations
    ↓
Identity / Ownership / Scope
    ↓
Policy / Authorization
    ↓
Resource Governance
    ↓
Context / Instruction / Execution
    ↓
Evaluation / Experience
    ↓
Learning Candidates
    ↓
Validation / Approval / Promotion
    ↓
New resource version or scoped state
```

## Prerequisites

1. [ ] Resource foundations from the earlier internal-storage/resource layer are canonical and provider-neutral. The historical 0.8 repository/backend wiring obligation is explicitly deferred to this phase for the resource persistence that governance requires; see `docs/roadmap/20-data-access.md`.
2. [ ] Identity / tenancy / user context is available for ownership and scope decisions.
3. [ ] Unified policy enforcement is available for resource access and learning promotion decisions.
4. [ ] Prompt/instruction governance and context engineering expose trust/provenance boundaries needed when resources enter execution context.
5. [ ] Evaluation provides quality signals usable by learning and promotion decisions.
6. [ ] Runtime instances and execution snapshots provide stable runtime ownership and immutable effective state boundaries.

## Resource governance

7. [ ] Define one generic resource governance model shared by Skills, Knowledge/Wiki, Memory families/types, and future resource types.
8. [ ] Make resource scope explicit and authorization-aware rather than inferred from agent identity alone.
9. [ ] Support Global, Tenant, Domain, User, Agent, Runtime, and Execution scopes where applicable to the resource type.
10. [ ] Preserve owner/resource identity separately from runtime-instance identity.
11. [ ] Preserve resource provenance, lifecycle/status, version, source, and relationship metadata through retrieval and promotion.
12. [ ] Ensure shared/reusable resources are references to authoritative resources rather than private copies embedded in agents.
13. [ ] Prevent prompt text or model output from granting access to a disabled or unauthorized resource.

## Capability policy and inheritance

14. [ ] Add profile capability defaults for Skills, Knowledge/Wiki, Memory families/types, individual resources, and future resource types.
15. [ ] Add tri-state runtime override: `Inherit`, `Enabled`, `Disabled`.
16. [ ] Resolve system/host policy → agent profile → runtime override into one effective resource capability state.
17. [ ] Capture the effective capability/resource state in every execution snapshot that can observe or invoke those resources.
18. [ ] Enforce capability policy before retrieval, exposure, or invocation.
19. [ ] Ensure runtime-only overrides never mutate the persistent profile.
20. [ ] Surface the source and effective value so operators can distinguish inherited, explicitly configured, and overridden state.

## Knowledge and Wiki

21. [ ] Complete the provider-neutral knowledge resource/source contract and managed Wiki model over the earlier resource foundation.
22. [ ] Support identity, title/content, summary, metadata, tags/categories, provenance, lifecycle/status, versioning, relationships, and bounded retrieval metadata.
23. [ ] Keep retrieval independent from physical indexing: keyword, semantic, hybrid, relational, or future implementations remain replaceable.
24. [ ] Support reusable shared knowledge plus authorized agent/runtime scoped resources.
25. [ ] Prevent model-generated content from silently becoming authoritative knowledge.
26. [ ] Preserve source and promotion provenance when a candidate becomes authoritative knowledge.
27. [ ] Make knowledge retrieval bounded by policy, resource limits, and context budgets.

## Skills

28. [ ] Complete stable/versioned `SkillDefinition` and skill-set/reference semantics over the earlier resource foundation.
29. [ ] Keep executable handlers separate from persisted definitions and never serialize handlers.
30. [ ] Support required knowledge, required tools, input/output contracts, preconditions, procedure steps, constraints, and lifecycle/version metadata.
31. [ ] Allow reusable skills to be referenced by multiple agents without duplicating the definition.
32. [ ] Preserve execution snapshot semantics so in-flight executions continue using their captured skill version/state.
33. [ ] Support `SkillCandidate` → validation/evaluation → new skill version or explicit rejection.
34. [ ] Ensure skill invocation remains subject to policy and authorization.

## Memory

35. [ ] Normalize memory families including working, episodic, semantic, procedural, and future extensible types.
36. [ ] Make memory scope explicit: execution, runtime, logical agent, user, tenant, or other approved scope.
37. [ ] Preserve independent runtime-instance private-memory isolation.
38. [ ] Keep storage implementation separate from memory ownership and retrieval policy.
39. [ ] Make memory-type enable/disable state governable at profile and runtime levels.
40. [ ] Support bounded retrieval and retention policies appropriate to each memory family/type.
41. [ ] Preserve provenance and confidence/evidence metadata where available.
42. [ ] Keep Memory usable without GPU hardware, vector databases, embeddings, or large resident indexes.

## Learning modes and candidates

43. [ ] Define provider-neutral `LearningMode`: `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, `FullyAutomatic`.
44. [ ] Keep Learning Mode distinct from resource/capability enablement.
45. [ ] Define one learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization.
46. [ ] Support typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts.
47. [ ] Preserve source execution ID, runtime ID, agent/profile identity, scope, provenance, and evidence/confidence on candidates when available.
48. [ ] Support deterministic code-derived learning signals without requiring an LLM.
49. [ ] Allow optional model-assisted extraction/classification while keeping the model non-authoritative.
50. [ ] Keep candidate creation separate from candidate promotion.
51. [ ] Preserve published Skill/Knowledge versions when learning produces an improvement; never silently mutate an active version.

## Learning lifecycle

52. [ ] Establish the canonical lifecycle:

```text
Experience / Observation
    → Candidate
    → Validation / Evaluation
    → Policy decision
    → Approval where required
    → Promotion
    → New authoritative version or scoped state
```

53. [ ] Support candidate rejection without modifying the target resource.
54. [ ] Support candidate expiry/retention according to policy.
55. [ ] Preserve candidate provenance after rejection or promotion according to retention rules.
56. [ ] Require explicit authorization for promotion into authoritative Knowledge or published Skills.
57. [ ] Ensure a model cannot bypass candidate state transitions by emitting apparently authoritative content.
58. [ ] Make promotion decisions auditable and attributable to policy, operator, or automated governance rules.
59. [ ] Allow evaluation outcomes to block, approve, or condition promotion.

## Context and instruction integration

60. [ ] Integrate governed Skills, Knowledge, Memory, and externally retrieved content into the canonical context/instruction pipeline.
61. [ ] Carry resource provenance, trust, scope, and effective capability state into the execution snapshot where exposed.
62. [ ] Ensure disabled, unauthorized, unavailable, stale, or failed resources remain diagnosable without being exposed as authoritative context.
63. [ ] Keep prompt/instruction composition separate from authorization and enforcement.
64. [ ] Ensure lower-trust resource content cannot override higher-authority instructions or policy.

## Management UI

65. [ ] Add Learning Review management with pending candidates, inspection, provenance/evidence, source execution/runtime, target scope, approve, reject, and retention state.
66. [ ] Add Knowledge/Wiki Manager with New/Edit/Delete, search/filter, relationships, version/status, provenance, and used-by/accessed-by views.
67. [ ] Add Skill Manager with New/Edit/Delete, version/status, relationships, required dependencies, and used-by views.
68. [ ] Extend Agent Configuration so selecting an agent shows effective Skills, Knowledge/Wiki, Memory families, and generic future resource types.
69. [ ] Add profile-level controls for resource/capability enablement.
70. [ ] Add runtime-instance override controls using `Inherit` / `Enabled` / `Disabled`.
71. [ ] Show inherited, explicit, overridden, and effective states clearly.
72. [ ] Show the effective AI selection/cost policy established by Phase 0.96 without duplicating provider/model discovery.
73. [ ] Expose Learning Mode and make its relationship to Learning Policy explicit.
74. [ ] Keep known resource types specialized while unknown/future resource types remain visible through generic resource inventory.
75. [ ] Follow HAgent.WinForms conventions: shared `Header`, `HButton`, `HMessage`, and existing configuration composition boundaries.

## Storage

76. [ ] Complete HAgent-owned persistence for candidates, knowledge-resource relationships, skill versions/relationships, resource capability assignments/overrides, and extensible memory-type policy where still outstanding. This is the consuming completion point for the historical 0.8 Item 8 repository/backend wiring obligation for mature resources.
77. [ ] Keep File, SQL Server, and MySQL behavior aligned through versioned migrations.
78. [ ] Keep learning/review metadata secret-safe and bounded.
79. [ ] Preserve resource identity/ownership/scope directly in persistence rather than creating subsystem-specific ownership models.

## Runtime integration

80. [ ] Bind effective resource capability state into runtime execution snapshots.
81. [ ] Capture execution outcomes and observations as learning input without mutating runtime identity.
82. [ ] Preserve runtime isolation, execution correlation, cancellation, timeout, stale-result protection, and concurrent execution semantics.
83. [ ] Ensure runtime overrides remain transient and cannot write back into persistent profile configuration.
84. [ ] Make resource access and learning promotion visible through the same observability and policy boundaries as other runtime actions.

## Verification

85. [ ] Add deterministic Example verification for resource scope isolation, inherited/overridden capability state, memory families/types, knowledge retrieval, skill binding, and future resource types.
86. [ ] Add Example verification for `SuggestOnly` review, approval, rejection, and candidate retention.
87. [ ] Add tests that candidates cannot bypass authorization or directly mutate published Knowledge/Skills.
88. [ ] Add tests for resource version/snapshot isolation after profile/resource edits.
89. [ ] Add tests for promotion conflicts, stale candidates, contradictory evidence, and policy denial.
90. [ ] Add tests for runtime isolation across two independent runtime instances.
91. [ ] Add UI verification that Agent Configuration displays effective resource/capability state, Learning Mode, and inherited/overridden configuration correctly.
92. [ ] Verify all supported framework targets required by HAgent before declaring the phase complete.

## Exit criterion

Knowledge, Skills, Memory, and Learning are first-class HAgent resources with explicit identity, scope, ownership, provenance, policy, capability inheritance, runtime overrides, immutable execution snapshots, and provider-neutral contracts. Hosts can administer reusable Knowledge and Skills, isolate Memory correctly, and run governed learning from experience through typed candidates, evaluation, approval/policy, and safe promotion without allowing model output to become authoritative by itself.
