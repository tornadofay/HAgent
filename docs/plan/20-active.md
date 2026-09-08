# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.954 Prompt and Instruction Governance — CURRENT

Phase 0.954 is the next ordered foundational milestone after the verified 0.953 Unified Policy Engine. The repository roadmap defines 0.954 through 0.958 as foundations that precede 0.959 Human-in-the-Loop / Intervention. Work that was implemented ahead of this order in 0.959 remains in source but is not treated as the current milestone or as evidence that the intervening phases are complete.

### Objective

Define trusted instruction layers and provenance so HAgent can safely combine system policy, agent instructions, Skills, Knowledge, Memory, tools, runtime context, user input, and externally retrieved content without allowing lower-authority or untrusted content to override higher-authority instructions or code-enforced controls.

### Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **CURRENT — Instruction source and authority contracts**
   - Scope: Define the provider-neutral normalized instruction/source model, source type, authority/trust level, provenance, scope, lifecycle metadata, deterministic precedence, conflict representation, and execution-snapshot provenance.
   - Entry: Verified 0.953 Unified Policy Engine; existing prompt construction and execution snapshot boundaries identified.
   - Completion: Core contracts can represent trusted and untrusted instruction sources with deterministic authority/precedence semantics and evidence suitable for execution snapshots.
   - Verification: Add deterministic Example coverage for source creation/validation, precedence, authority separation, and provenance.

2. **Instruction composition and conflict handling**
   - Scope: Integrate the contracts into canonical prompt/instruction assembly, preserve higher-authority layers, handle conflicts/invalid or unavailable sources, and keep secrets/sensitive host data out of diagnostics by default.
   - Entry: Slice 1 verified.
   - Completion: canonical instruction composition produces a deterministic provider-neutral snapshot and rejects/contains invalid authority transitions.

3. **Resource and external-content boundaries**
   - Scope: Integrate Skills, Knowledge, Memory, tool descriptions, runtime context, host context, user content, and externally retrieved content with explicit trust/provenance semantics while keeping authorization outside prompt text.
   - Entry: composition semantics verified.
   - Completion: lower-authority/untrusted content cannot erase higher-authority policy or instruction layers, and disabled/unavailable sources remain diagnosable.

4. **Execution integration**
   - Scope: Feed the effective instruction snapshot into the existing execution boundary without creating a second execution/prompt engine and preserve active-execution snapshot isolation.
   - Entry: instruction model and composition are stable.
   - Completion: running executions retain immutable effective instruction state even when source configuration changes.

5. **Example coverage and framework verification**
   - Scope: Add deterministic public-API Example scenarios for precedence, conflicts, untrusted content, disabled resources, provenance, snapshot isolation, cancellation/failure boundaries, and supported framework targets.
   - Entry: implementation is stable.
   - Completion: the new Example coverage passes locally on the supported targets and the authoritative phase documents record verification.

6. **Advance to 0.955 Context Engineering**
   - Scope: Only after 0.954 is verified, update the active plan to the next roadmap phase.

### Relationship to 0.959 work already present in source

A prior run advanced into 0.959 before 0.954–0.958 were completed. That was an ordering mistake, not a reason to redefine the roadmap. The existing 0.959 intervention code is retained as ahead-of-roadmap work in the source tree, but it is not considered a completed project milestone until the ordered foundational phases and their required verification are reached.

### Architectural boundaries

Prompt/instruction governance is an authority and provenance boundary, not an authorization mechanism. Prompt text cannot grant permissions, bypass policy, approve protected operations, or elevate untrusted content. Provider adapters receive provider-neutral effective instructions and remain responsible only for transport-specific representation.

The authoritative requirements for this phase are in `docs/roadmap/954-prompt-instruction-governance.md` and the broader instruction architecture. The roadmap order is normative unless an explicit architectural decision records a dependency-driven exception.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.
