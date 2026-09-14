# Decisions Required

These decisions are intentionally left open for the project owner. The rebuild must not silently choose a boundary merely because MAF exposes a similarly named API.

## 1. Public execution abstraction

### A — HAgent contract above MAF

`Host -> HAgent execution -> HAgent.Maf -> MAF -> IChatClient`

Pros: preserves HAgent framework-neutral execution semantics; separates runtime/execution identity; isolates MAF API changes; keeps future replacement possible.

Cons: extra mapping layer and some duplicated type concepts.

### B — Direct MAF contract

`Host -> HAgent + MAF types -> MAF`

Pros: less code, lower mapping cost, direct ecosystem access.

Cons: strong MAF coupling; MAF semantics become part of HAgent public API; future replacement becomes expensive; distinctive HAgent runtime semantics may have to fit MAF.

### C — Hybrid

HAgent owns distinctive runtime/cognitive contracts and exposes MAF types through explicitly advanced integration APIs.

Pros: balances isolation and ecosystem access.

Cons: two public styles increase conceptual complexity.

**Owner decision required:** choose A, B, or C before Phase 2 contract freeze.

## 2. Skills

MAF has Skills. HAgent also has governed, versioned Skills with identity, ownership, scope, policy, lifecycle, and learning relationships.

### A — HAgent Skills remain canonical

Pros: preserves HAgent governance and future framework independence.

Cons: requires mapping HAgent Skill resources to MAF Skill execution objects.

### B — MAF Skills become canonical

Pros: removes duplicated resource modeling and follows the ecosystem.

Cons: HAgent governance semantics must fit whatever MAF exposes.

### C — Hybrid adapter

HAgent resource metadata remains canonical; MAF Skill objects are generated for execution.

**Owner decision required:** compare the exact current MAF Skill API with HAgent semantics during Phase 1.

## 3. Sessions versus runtime instances

MAF sessions/conversations should not automatically be treated as HAgent runtime instances.

### A — Keep both

HAgent runtime owns durable cognitive state; MAF session owns conversation/execution context.

Pros: clear semantic separation.

Cons: state mapping.

### B — MAF session is the HAgent runtime

Pros: less state and code.

Cons: high coupling and likely semantic mismatch for persistent cognition.

**Owner review:** do not collapse these concepts until Phase 1 completes the semantic comparison.

## 4. Workflow ownership

### A — MAF owns generic workflows

Use MAF workflows for orchestration while HAgent plans remain cognitive objects when they represent goals and intentions rather than execution topology.

Pros: maximum de-duplication and reuse of MAF graph execution.

Cons: cognitive plans need an explicit mapping when they become executable workflows.

### B — HAgent keeps its own workflow engine

Pros: full control.

Cons: duplicates a major MAF capability and should not be chosen without a demonstrated semantic requirement.

**Default recommendation:** A unless Phase 1 identifies a hard mismatch.

## 5. Provider abstraction

### A — HAgent provider API wraps `IChatClient`

Pros: simple place for HAgent execution-target metadata and planning.

Cons: another provider abstraction remains.

### B — HAgent uses `IChatClient` directly

Pros: maximum reuse of MEAI.

Cons: provider registry, target metadata, quota, admission, and planner must live outside the transport abstraction.

### C — Hybrid

Use `IChatClient` as the transport contract while HAgent retains provider registry, execution target, capability, quota/admission, and planner contracts.

**Owner decision required:** make the choice after mapping the current provider implementation.

## 6. Persistence ownership

Every durable datum must have exactly one authoritative owner.

Likely split:

- MAF owns MAF-native workflow/session state where its model is sufficient.
- HAgent owns cognitive state, governed resources, provider registry, execution-target metadata, learning records, and HAgent configuration when these remain HAgent concepts.

The exact split must be decided after inspecting the selected MAF persistence APIs.

## Decision record format

For every decision record:

1. chosen option;
2. reason;
3. migration impact;
4. public API impact;
5. future MAF convergence path;
6. replacement plan if MAF later supplies a better native implementation.
