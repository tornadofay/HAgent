# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.954 Prompt and Instruction Governance — CURRENT

Phase 0.954 is the next ordered foundational milestone after the verified 0.953 Unified Policy Engine. The roadmap now treats Knowledge, Skills, Memory, and Learning as first-class architecture from the foundation upward: Phase 0.8 provides their canonical resource/persistence substrate, while Phase 0.9575 provides mature governance and learning promotion after the required instruction, context, and evaluation boundaries exist.

Work that was implemented ahead of the roadmap in 0.959 remains in source but is not treated as the current milestone or as evidence that the intervening phases are complete.

### Pre-phase Example UI prerequisite — CURRENT

Before beginning 0.954 implementation, the manual verification host must be reorganized according to the Example UI rules in `AGENTS.md`: architecture-level top-level feature tabs, with multiple examples represented by nested focused tabs instead of an ever-growing flat tab list.

- Entry: existing Example host contains a growing flat tab collection and the roadmap has been reset to the ordered 0.954 milestone.
- Scope: reorganize the existing Example UI into feature groups without changing the underlying Example test behavior; restore the existing `LEARNING INTERVENTION` example to the visible host; keep Example code split into focused partial files.
- Implementation: `src/HAgent.Example/MainForm.ExampleOrganization.cs` creates the feature-group/nested-tab presentation during `MainForm.OnLoad` and adds the already-implemented learning intervention example before grouping.
- Verification: user builds/runs `HAgent.Example`, confirms the top-level feature groups and nested examples are usable, confirms `LEARNING INTERVENTION` is present under `Cognition`, and confirms existing examples remain independently runnable.
- Completion: the UI is locally verified and no Example test capability is lost or duplicated.

### 0.954 Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Instruction source and authority contracts**
   - Scope: Define the provider-neutral normalized instruction/source model, source type, authority/trust level, provenance, scope, lifecycle metadata, deterministic precedence, conflict representation, and execution-snapshot provenance.
   - Entry: Verified Example UI prerequisite and verified 0.953 Unified Policy Engine; first-class resource foundations are established in the 0.8 architecture and existing resource contracts must be consumed rather than replaced.
   - Completion: Core contracts can represent trusted and untrusted instruction sources with deterministic authority/precedence semantics and evidence suitable for execution snapshots.
   - Verification: Add deterministic Example coverage for source creation/validation, precedence, authority separation, and provenance.

2. **Instruction composition and conflict handling**
   - Scope: Integrate the contracts into canonical prompt/instruction assembly, preserve higher-authority layers, handle conflicts/invalid or unavailable sources, and keep secrets/sensitive host data out of diagnostics by default.
   - Entry: Slice 1 verified.
   - Completion: canonical instruction composition produces a deterministic provider-neutral snapshot and rejects/contains invalid authority transitions.

3. **Resource and external-content boundaries**
   - Scope: Integrate the canonical first-class Skills, Knowledge, Memory, tool descriptions, runtime context, host context, user content, and externally retrieved content with explicit trust/provenance semantics while keeping authorization outside prompt text. Do not implement mature resource governance here; consume the Phase 0.8 resource substrate and preserve the later Phase 0.9575 governance boundary.
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

### Relationship to resource architecture and ahead-of-roadmap intervention work

Knowledge, Skills, Memory, and Learning are now first-class architecture. Their canonical resource/persistence substrate belongs in the early foundation represented by Phase 0.8. Mature scope/authorization governance, capability inheritance, runtime tri-state overrides, learning policy, evaluation-aware candidate validation, approval, promotion, and management UI are deferred to Phase 0.9575 where their prerequisite boundaries are available.

A prior run advanced into 0.959 before 0.954–0.958 were completed. That was an ordering mistake, not a reason to redefine the roadmap. The existing 0.959 intervention code is retained as ahead-of-roadmap work in the source tree, but it is not considered a completed project milestone until the ordered foundational phases and their required verification are reached.

The roadmap places 0.9591 Goal/Plan Persistence and Recovery before 0.959 Human-in-the-Loop / Intervention so durable goal/plan revision and recovery authority exists before later intervention can govern those persistent targets. Execution-level intervention remains valid independently and is preserved in source.

### Architectural boundaries

Prompt/instruction governance is an authority and provenance boundary, not an authorization mechanism. Prompt text cannot grant permissions, bypass policy, approve protected operations, or elevate untrusted content. Provider adapters receive provider-neutral effective instructions and remain responsible only for transport-specific representation.

The authoritative requirements for this phase are in `docs/roadmap/954-prompt-instruction-governance.md` and the broader instruction architecture. Resource architecture is authoritative in `docs/architecture/80-knowledge-memory-learning.md`; its early foundation and mature governance/promotion stages are represented separately in the ordered roadmap. The roadmap order is dependency-driven and may be changed when architectural understanding reveals a genuine dependency change; such a change must be recorded in the authoritative roadmap/current-state documents.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.
