# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage now explicitly includes the first-class resource foundation for Knowledge, Skills, Memory, and Learning candidates. Its storage/resource primitives are substantially implemented; mature resource governance and learning promotion remain intentionally later work.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is completed for its verified runtime/persistence/resource/learning-policy foundation. 0.954 Prompt and Instruction Governance is completed and verified on .NET Framework 4.8.1 and .NET 9. 0.955 Context Engineering is completed and verified on .NET Framework 4.8.1 and .NET 9. 0.956 Observability and Distributed Tracing is completed and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9, including authoritative execution-outcome observations consumed by tracing without provider-side state inference.

0.957 Evaluation and Quality Measurement is now **verified through Slice 6**. User verification on 2026-09-09 recorded **139/139 HAgent.Tests passed**, and the required evaluation Example scenarios succeeded on **.NET Framework 4.8.1 and .NET 9**. The completed evaluation path covers provider-neutral evaluation contracts, deterministic evaluators, human/application ratings, model-assisted judging, aggregation/comparison, and repeated regression-suite execution.

## First-class Knowledge / Skills / Memory / Learning architecture

HAgent no longer treats the old Phase 0.11 block as one late feature layer. The architecture is split across the roadmap according to dependency:

```text
0.8 Data Access / Internal Storage + Resource Foundations
    ↓
0.951–0.953 Identity / Events / Policy
    ↓
0.954–0.957 Instruction / Context / Observability / Evaluation
    ↓
0.9575 Mature Resource Governance + Learning
    ↓
0.958+ Lifecycle / Recovery / Intervention / Provider / Execution foundations
    ↓
0.97 Persistent Cognitive Runtime
```

The 0.8 resource foundation provides canonical provider-neutral resource identity, scope/ownership metadata, provenance, lifecycle/versioning, Skill definitions/references, Knowledge/Wiki contracts, Memory family/type foundations, typed learning-candidate foundations, and HAgent-owned persistence substrate. It is deliberately foundational rather than a complete management/governance feature.

Phase 0.9575 completes mature governance: resource authorization, capability inheritance, runtime tri-state overrides, effective execution resource snapshots, bounded retrieval/retention, resource management UI, Learning Mode, learning policy, evaluation-aware candidate validation, approval/promotion, version/conflict handling, and safe promotion into authoritative resources.

Knowledge, Skills, Memory, and Learning remain distinct. Skills are reusable executable capabilities; Knowledge is reusable retrievable information; Memory is scoped experience/state; Learning is the governed transformation of experience into typed candidates and, where permitted, promoted authoritative resource state.

## Foundational architecture hardening before 0.96

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Event subsystem
        ↓
0.953 Unified Policy Engine
        ↓
0.954 Prompt / Instruction Governance — verified
        ↓
0.955 Context Engineering — verified
        ↓
0.956 Observability / Tracing — verified through Slice 8
        ↓
0.957 Evaluation / Quality Measurement — verified through Slice 6
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning — current
        ↓
0.958 Agent Lifecycle / Health
        ↓
0.9591 Goal / Plan Persistence / Recovery
        ↓
0.959 Human-in-the-Loop / Intervention
        ↓
0.9592 Provider Ecosystem / Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability Evolution
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
```

The numbered roadmap is dependency-driven, not permanently locked. When architectural understanding reveals a real dependency change, the roadmap and current-state documents must be updated together. Ahead-of-roadmap implementation remains code evidence rather than milestone completion.

## 0.957 Evaluation completion boundary

0.957 is complete and verified through Slice 6. Slices 1–4 established the evaluator boundaries and evidence types; Slice 5 established bounded aggregation/comparison; Slice 6 established repeated case-target regression execution through a host-owned executor with bounded concurrency, failure isolation, cancellation/late-result protection, deterministic results, and direct handoff to the existing aggregation/comparison contracts.

Evaluation remains measurement-only. Regression results and aggregate preferences do not route production execution, authorize actions, mutate configuration, promote learning, or become cognitive authority. Failed/canceled regression executions remain explicit lifecycle results and do not become fabricated evaluation evidence.

## 0.9575 Resource governance completion boundary

0.9575 Slice 1 is **verified**. The mature resource admission foundation is established over the existing 0.8 resource primitives, canonical identity ownership, and unified policy engine. The `AiResourceGovernanceEvaluator` composes owner proof, effective capability state, and policy authorization without introducing a second authorization or resource model. `AiResourceCapabilitySnapshot` preserves the source of effective configuration as `Default`, `Profile`, or `RuntimeOverride` for operator diagnostics.

User verification on 2026-09-09 succeeded on both `.NET Framework 4.8.1` and `.NET 9` for `HAgent.Example → Cognition → Resource Governance → Resource Governance`. The full `.NET 9` `HAgent.Tests` suite passed **146/146** with **0 failed** and **0 skipped**.

0.9575 Slice 2 is **verified**. The provider-neutral Knowledge/Wiki contract establishes managed Knowledge/Wiki resources with explicit scope/ownership, lifecycle/versioning, provenance/source metadata, bounded tags/categories/metadata, typed relationships, chunk evidence, and provider/index-neutral retrieval contracts. `AiGovernedKnowledgeRetriever` composes the same verified resource governance boundary before forwarding admitted resource IDs to retrieval implementations.

User verification on 2026-09-09 succeeded on both `.NET Framework 4.8.1` and `.NET 9` for `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki`. The full `.NET 9` `HAgent.Tests` suite passed **153/153** with **0 failed** and **0 skipped**. Verification demonstrated published version/provenance preservation, non-authoritative model-generated drafts, owner isolation before retrieval, and bounded provider/index-independent retrieval.

The separate context-budget integration requirement remains open; Knowledge Manager CRUD, persistent resource repositories, semantic/vector indexing, learning promotion, and retention governance remain later 0.9575 work.

## 0.9575 Slice 3 Skills completion boundary

Slice 3 is **implemented; verification pending**. The provider-neutral Skill contract is now built over the existing generic governance and execution-snapshot foundations.

The implementation provides versioned `AiSkillDefinition` resources with explicit scope/ownership/lifecycle/provenance, bounded input/output contracts, preconditions, ordered procedure steps, required Knowledge/Tool dependencies, constraints, metadata, and relationships. `AiSkillReference` / `AiSkillSet` provide reusable explicit-scope references with optional version pins, and `AiAgent.Skills` is the canonical profile reference set.

`IAiSkillDefinitionSource` plus `AiGovernedSkillResolver` reuse `AiResourceGovernanceEvaluator` with `skill.invoke` before definition-source access. Admitted definitions must match identity/scope/owner, optional version pins, and published lifecycle state. `AiSkillExecutionSnapshot` and `AgentExecutionSnapshot.Skills` deep-clone admitted bindings so in-flight executions retain their captured Skill version/state independently of later profile/source mutation.

Executable handlers, delegates, callbacks, and provider SDK objects are intentionally absent from persisted Skill definitions and references. Skill-specific persistence, Skill Manager UI, SkillCandidate promotion, and context-budget integration remain later work.

Verification assets are in place: focused `SkillResourceTests`, the public Example `HAgent.Example → Cognition → Skills → Skill Definitions`, architecture `docs/architecture/82-skills.md`, durable decision D-010, and the dedicated Slice 3 workflow `.github/workflows/verify-phase-0-9575-slice-3.yml`.

GitHub Actions has queued Slice 3 verification from `master`, but no completed build/test result has been observed yet. The public WinForms Example also has not been executed in this environment, so Slice 3 is not marked verified or closed.

## Storage implications

The configuration/storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md` remains a cross-cutting foundation before 0.96. It must directly support authoritative resource relationships, scoped ownership, capability state, and portable configuration rather than creating a second resource configuration model.

The unified policy set is a canonical HAgent-owned configuration record exposed through `IAiStore`. File storage persists it with the main settings document; SQL Server and MySQL persist it in `HAgentPolicies`, and their HAgent database bootstrap paths create that table. The default runtime loads the current persisted policy asynchronously at execution creation when no explicitly injected policy engine is supplied, then captures the effective policy in the execution snapshot.

Agent profile resource capability defaults are part of the canonical `AiAgent` configuration and persist through the normal agent storage path. Runtime capability overrides remain transient and resolve above profile defaults into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.

Learning-promotion decisions are represented as normal `AiPolicyDecision` outcomes on `learning.promote`, with typed candidate metadata carried as bounded policy attributes. Mature candidate/target repositories and broader human intervention remain ordered roadmap work.

Human intervention is implemented ahead of its ordered milestone only as a coherent canonical workflow boundary. Execution-level intervention and concurrency/stale-state hardening have deterministic local Example verification; durable persistence, management UI, broader target support, and remaining lifecycle controls are not treated as complete.

Provider API keys are persisted with provider configuration and encrypted at rest. There is no separate provider secret-reference or vault architecture. Shared SQL Server/MySQL configuration can therefore be used by multiple authorized HAgent processes/machines. Configuration export/import remains planned as a versioned portable representation with optional encrypted credential inclusion.

### Deferred exclusions

The roadmap does not introduce a separate distributed-coordination subsystem and does not introduce a sophisticated external secret-management architecture. Distributed behavior is handled through the existing storage/runtime contracts where required, while provider credentials use the project's intentionally simple fixed encryption/decryption mechanism.
