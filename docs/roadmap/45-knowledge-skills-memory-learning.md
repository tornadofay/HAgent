# Phase 0.11 — Knowledge, Skills, Memory Governance + Learning

## Goal

Turn the existing memory/skill/wiki storage foundations into a coherent, provider-neutral knowledge and learning subsystem with explicit scope, capability inheritance, runtime overrides, and WinForms management surfaces.

## Architecture outcome

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = execution experience -> typed candidates -> policy -> promotion
```

Resources are shared/reusable by default; ownership and access are explicit through scope and authorization. Runtime instances inherit profile configuration but may override individual capability/resource states without mutating the profile.

## Learning modes

1. [ ] Add provider-neutral `LearningMode`: `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, `FullyAutomatic`.
2. [ ] Add learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, and promotion authorization.
3. [ ] Add typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts.
4. [ ] Preserve execution/runtime/agent provenance on candidates.
5. [ ] Support deterministic code-derived learning signals without requiring an LLM.
6. [ ] Allow optional model-assisted extraction/classification without making the model the authority.
7. [ ] Keep promotion separate from candidate creation and keep published Skills versioned.

## Knowledge and Wiki

8. [ ] Define the provider-neutral knowledge resource/source contract and managed Wiki model.
9. [ ] Define resource scope, lifecycle/status, provenance, versioning, metadata, tags/categories, and relationships.
10. [ ] Define bounded retrieval contracts independent of keyword/vector/index implementation.
11. [ ] Support reusable shared knowledge plus agent/runtime scoped resources where authorized.
12. [ ] Prevent model-generated content from silently becoming authoritative knowledge.

## Skills

13. [ ] Define stable/versioned SkillDefinition and SkillSet/reference contracts.
14. [ ] Keep executable handlers separate from persisted definitions.
15. [ ] Support required knowledge, required tools, input/output contracts, preconditions, procedure steps, constraints, and lifecycle.
16. [ ] Preserve snapshot semantics so in-flight executions are not changed by later skill edits.
17. [ ] Support SkillCandidate -> validation -> new skill version workflow.

## Memory

18. [ ] Normalize memory families including working, episodic, semantic, procedural, and future extensible types.
19. [ ] Make memory scope explicit: execution, runtime, logical agent, user, tenant, or host-approved future scope.
20. [ ] Preserve the existing invariant that independent runtime instances never share private mutable memory ownership.
21. [ ] Keep storage implementation independent of memory ownership and retrieval policy.
22. [ ] Add memory-type enable/disable policy to agent profiles and runtime overrides.

## Capability policy

23. [ ] Add profile capability defaults for Skills, Knowledge/Wiki, Memory, and individual resources/types.
24. [ ] Add tri-state runtime override: `Inherit`, `Enabled`, `Disabled`.
25. [ ] Compute one effective immutable capability snapshot per execution.
26. [ ] Enforce capability policy before retrieval, exposure, or invocation.
27. [ ] Use stable resource/type identifiers so future knowledge types can be surfaced without changing the agent domain model.

## Management UI

28. [ ] Add Learning Review management surface with pending candidate list, inspection, provenance/evidence, source execution/runtime, target scope, approve, and reject.
29. [ ] Add Wiki/Knowledge Manager with New/Edit/Delete, search/filter, relationships, and "used by/accessed by agents" views.
30. [ ] Add Skill Manager with New/Edit/Delete, version/status, relationships, and "used by agents" views.
31. [ ] Extend Agent Configuration so selecting an agent shows effective Skills, Knowledge/Wiki, Memory families, and all generic future resource types.
32. [ ] Add profile-level capability switches for Skills, Wiki/Knowledge, Memory, and memory types.
33. [ ] Add runtime-instance-level override controls using `Inherit`/`Enabled`/`Disabled`.
34. [ ] Keep known types specialized while rendering unknown/future resource types through the generic inventory view.
35. [ ] Follow existing HAgent.WinForms conventions: `HMessage`, shared `Header`, `HButton`, and preserve existing layouts unless this phase requires a targeted change.

## Storage

36. [ ] Add HAgent-owned storage migrations for candidates, knowledge resources, skill relationships/versioning, capability assignments/overrides, and memory-type policy where required.
37. [ ] Keep File, SQL Server, and MySQL behavior aligned through provider-specific migrations.
38. [ ] Keep learning/review metadata secret-safe and bounded.

## Runtime integration

39. [ ] Bind effective knowledge/skill/memory policy into the runtime execution snapshot.
40. [ ] Capture execution outcomes/observations as learning input without mutating runtime identity.
41. [ ] Preserve runtime-instance isolation, execution correlation, cancellation, timeout, stale-result protection, and concurrent execution behavior.
42. [ ] Ensure runtime-only overrides never write back to the persistent profile.

## Verification

43. [ ] Add deterministic Example verification for scope isolation, inherited/overridden capability state, memory types, knowledge retrieval, skill binding, and learning candidates.
44. [ ] Add Example verification for SuggestOnly review and approval/rejection.
45. [ ] Add tests that a candidate cannot bypass authorization or directly mutate a published Wiki/Skill.
46. [ ] Add tests for future/unknown resource types surviving inventory and UI projection.
47. [ ] Add tests that existing executions retain immutable capability/skill snapshots after later edits.

## Exit criterion

A host can define reusable Skills and managed Wiki/knowledge, keep independent scoped runtime memories, enable/disable individual capability families and resources at profile or runtime level, run automatic or review-based learning through `LearningMode`, and administer all of it through HAgent.WinForms without introducing host-specific domain types into Core.
