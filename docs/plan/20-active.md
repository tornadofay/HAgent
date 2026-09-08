# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.954 Prompt and Instruction Governance — CURRENT

Phase 0.954 is the next ordered foundational milestone after the verified 0.953 Unified Policy Engine. The roadmap now treats Knowledge, Skills, Memory, and Learning as first-class architecture from the foundation upward: Phase 0.8 provides their canonical resource/persistence substrate, while Phase 0.9575 provides mature governance and learning promotion after the required instruction, context, and evaluation boundaries exist.

Work that was implemented ahead of the roadmap in 0.959 remains in source but is not treated as the current milestone or as evidence that the intervening phases are complete.

### Pre-phase Example UI prerequisite — VERIFIED

Before beginning 0.954 implementation, the manual verification host was reorganized according to the Example UI rules in `AGENTS.md`: architecture-level top-level feature tabs, with multiple examples represented by nested focused tabs instead of an ever-growing flat tab list.

- Entry: existing Example host contained a growing flat tab collection and the roadmap had been reset to the ordered 0.954 milestone.
- Scope: reorganize the existing Example UI into feature groups without changing the underlying Example test behavior; restore the existing `LEARNING INTERVENTION` example to the visible host; keep Example code split into focused partial files.
- Implementation: `src/HAgent.Example/MainForm.ExampleOrganization.cs` creates the feature-group/nested-tab presentation during `MainForm.OnLoad` and adds the already-implemented learning intervention example before grouping.
- Verification: user verified successful `LEARNING INTERVENTION`, `CONTEXT BUDGET`, and `RUNTIME INSTANCES` examples in the reorganized host on 2026-09-08.
- Completion: verified by the user; no Example test capability was reported lost or duplicated.

### 0.954 Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Instruction source and authority contracts — VERIFIED**
   - Scope: Define the provider-neutral normalized instruction/source model, source type, authority/trust level, provenance, scope, lifecycle metadata, deterministic precedence, conflict representation, and execution-snapshot provenance.
   - Entry: Verified Example UI prerequisite and verified 0.953 Unified Policy Engine; first-class resource foundations are established in the 0.8 architecture and existing resource contracts must be consumed rather than replaced.
   - Completion: Core contracts can represent trusted and untrusted instruction sources with deterministic authority/precedence semantics and evidence suitable for execution snapshots.
   - Verification: User executed `COGNITION INSTRUCTIONS` and confirmed source creation/validation, authority-vs-trust separation, precedence, explicit priority, conflict representation, and provenance-preserving snapshot cloning.

2. **Instruction composition and conflict handling — VERIFIED**
   - Scope: Integrate the contracts into canonical prompt/instruction assembly, preserve higher-authority layers, handle conflicts/invalid or unavailable sources, and keep secrets/sensitive host data out of diagnostics by default.
   - Entry: Slice 1 verified.
   - Completion: canonical instruction composition produces a deterministic provider-neutral snapshot and rejects/contains invalid authority transitions.
   - Verification: User executed the updated `COGNITION INSTRUCTIONS` and confirmed additive composition, deterministic conflicts, disabled/invalid containment, and secret-safe diagnostics.

3. **Resource and external-content boundaries — VERIFIED**
   - Scope: Integrate the canonical first-class Skills, Knowledge, Memory, tool descriptions, runtime context, host context, user content, and externally retrieved content with explicit trust/provenance semantics while keeping authorization outside prompt text. Do not implement mature resource governance here; consume the Phase 0.8 resource substrate and preserve the later Phase 0.9575 governance boundary.
   - Entry: Slice 2 verified.
   - Implementation: `AiInstructionSourceFactory` provides the provider-neutral mapping; `AiInstructionAvailability` distinguishes available, disabled, and unavailable sources; `AiInstructionComposer` excludes unavailable sources while preserving bounded metadata-only diagnostics; the Example exercises the trust boundary.
   - Completion: lower-authority/untrusted content cannot erase higher-authority policy or instruction layers, and disabled/unavailable sources remain diagnosable.
   - Verification: user executed **COGNITION INSTRUCTIONS → Run instruction contract test** on 2026-09-08 and confirmed trusted-resource authority/trust, external-content boundaries, unavailable-source handling, and lower-authority override resistance.

4. **Execution integration — VERIFIED**
   - Scope: Feed the effective instruction snapshot into the existing execution boundary without creating a second execution/prompt engine and preserve active-execution snapshot isolation.
   - Entry: Slices 1–3 verified.
   - Implementation: `AgentExecutionRequest.InstructionSources` carries host/resource instruction sources; `DefaultAgentRuntime` selects the execution target, composes the canonical instruction set once, captures a cloned `AgentExecutionSnapshot.InstructionSnapshot` before entering `Running`, and passes the same composed effective instruction text to provider transport. Caller-owned source mutations therefore cannot alter the captured execution instructions.
   - Completion: running executions retain immutable effective instruction state even when source configuration changes, and provider transport consumes the same effective composition rather than rebuilding it through a second prompt engine.
   - Verification: user executed **COGNITION INSTRUCTIONS → Run instruction contract test** on 2026-09-08 and confirmed effective snapshot capture, provider transport parity, caller-source mutation isolation, lower-authority exclusion, and execution/principal provenance.

5. **Example coverage and framework verification — CURRENT**
   - Scope: Complete deterministic public-API Example scenarios for precedence, conflicts, untrusted content, disabled resources, provenance, snapshot isolation, cancellation/failure boundaries, and supported framework targets.
   - Entry: execution integration verified.
   - Completion: the new Example coverage passes locally on the supported targets and the authoritative phase documents record verification.
   - Verification: run the complete 0.954 Example verification set on the supported targets, including the existing deterministic execution/cancellation/failure scenarios and the `COGNITION INSTRUCTIONS` scenario, then record the exact successful framework results.

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
