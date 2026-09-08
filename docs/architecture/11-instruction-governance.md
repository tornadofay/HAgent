# Instruction Governance

## Purpose

HAgent represents instruction-bearing content through provider-neutral source contracts before any provider-specific prompt formatting occurs. The contract preserves authority, trust, provenance, scope, lifecycle, availability, and conflict evidence so later composition can remain deterministic and reviewable.

## Instruction source

`AiInstructionSource` is the canonical normalized source record for instruction-bearing content. It contains:

- `SourceType` — system policy, agent instruction, Skill, Knowledge, Memory, tool description, runtime/host context, user input, external content, or model-generated content;
- `Authority` — the explicit authority boundary relevant to precedence;
- `TrustLevel` — evidence/trust metadata kept separate from authority;
- `Scope` — canonical generic scope metadata using Global, Tenant, User, Workspace, Agent, Runtime, or Execution where applicable;
- `Lifecycle` — active, disabled, expired, or revoked;
- `Availability` — available, disabled, or unavailable for the current source retrieval/context boundary;
- `Priority` — explicit deterministic priority within an otherwise equal authority boundary;
- `ConflictKey` — optional stable grouping key for competing instructions;
- version and lifecycle timestamps;
- bounded content;
- `AiInstructionProvenance` identifying the originating source, version, execution/runtime/principal when supplied, evidence, and capture time.

The source record is metadata and content only. It does not grant authorization, permission, approval, tool access, or host-side authority.

## Authority and trust

Authority and trust are intentionally independent dimensions. A high trust value cannot elevate a lower-authority source over a higher-authority policy or instruction. Trust expresses evidence quality or origin confidence; authority expresses the instruction boundary used for precedence.

The current authority ordering is:

```text
SystemPolicy
    > Agent
    > TrustedResource
    > Runtime
    > User
    > External
    > Untrusted
```

`AiInstructionAuthority` uses numeric ordering only as an implementation aid; consumers must use `AiInstructionPrecedence` rather than relying on enum values directly.

Resource sources such as Skills, Knowledge, Memory, and tool descriptions are created with explicit `TrustedResource` authority and `HAgentTrusted` trust metadata. Runtime and host context use runtime authority with host-trusted metadata. User input remains user authority, while externally retrieved content is explicitly external and untrusted/external-trust metadata.

## Deterministic precedence

`AiInstructionPrecedence.Compare` compares sources in this order:

1. explicit authority;
2. explicit priority;
3. trust level;
4. scope specificity;
5. deterministic source-type precedence;
6. stable source ID ordering as the final tie-breaker.

Lifecycle and availability eligibility are evaluated separately at a supplied point in time. This keeps time-dependent activation and source retrieval state outside the precedence ordering itself.

`SelectWinner(sources, at)` selects only sources active at the current point and then applies the same deterministic comparator. There is no declaration-order or prompt-position authority rule.

## Canonical composition

`AiInstructionComposer` is the single provider-neutral composition boundary for instruction-bearing sources. It validates each source, excludes disabled/expired/revoked or unavailable/invalid sources, records bounded diagnostics for excluded input, groups competing active sources by `ConflictKey`, and selects one deterministic winner per conflict group using `AiInstructionPrecedence`.

Sources without a conflict key remain additive. The resulting `AiInstructionCompositionResult` contains:

- `Snapshot` — cloned authoritative sources plus conflict evidence;
- `ComposedText` — deterministic provider-neutral instruction text;
- `Diagnostics` — bounded exclusion diagnostics that identify source metadata/reason without copying source content.

`SystemPromptComposer` remains a small compatibility-shaped façade for the existing runtime API and delegates to `AiInstructionComposer`; the execution path does not create a second precedence or conflict mechanism.

## Resource and external-content boundary

`AiInstructionSourceFactory` provides the canonical provider-neutral conversion from first-class resource/context categories into instruction sources without creating a second resource model. It supports Skills, Knowledge, Memory, tool descriptions, runtime context, host context, user input, and externally retrieved content.

The factory assigns conservative authority/trust semantics:

```text
Skill / Knowledge / Memory / Tool description -> TrustedResource / HAgentTrusted
Runtime / Host context                       -> Runtime / HostTrusted
User input                                   -> User / UserSupplied
External content                             -> External / External
```

This is instruction provenance metadata, not authorization. Resource capability policy, HAgent policy, and host authorization remain separate enforcement mechanisms and must execute before exposing or invoking protected resources.

A source may additionally be marked `Disabled` or `Unavailable`. The composer keeps such sources out of the effective instruction snapshot while retaining bounded metadata-only diagnostics so administrators can distinguish an unavailable source from a valid authoritative instruction.

Lower-authority external or user content can participate in conflict evidence but cannot replace a higher-authority trusted resource source under deterministic precedence. External-content text is never treated as policy merely because it contains imperative language.

Mature Skill/Knowledge/Memory retrieval, resource authorization, bounded retrieval, and learning promotion remain later roadmap responsibilities; this slice establishes only the instruction/context trust boundary around their canonical resource outputs.

## Execution integration

`AgentExecutionRequest.InstructionSources` is the provider-neutral host/runtime input boundary for resource, external-content, and other instruction sources that must participate in an execution. The runtime does not treat these sources as authorization; it passes them through the same canonical validation, availability, precedence, and conflict logic as built-in execution instructions.

`DefaultAgentRuntime` resolves the execution target first because the existing provider default system prompt is provider-specific configuration. Once the target is selected and policy permits execution, the runtime composes the provider default system instruction (when enabled), agent instruction, existing execution prompt layers, and request-supplied instruction sources exactly once through `AiInstructionComposer`. The result is captured as a cloned `AgentExecutionSnapshot.InstructionSnapshot` while the execution is still in `Created` state, then the execution transitions to `Running`.

The resulting `ComposedText` is retained for provider transport and the provider adapter receives that already-composed text. Provider adapters therefore do not rebuild instruction precedence or conflict behavior. `AgentExecution.CaptureInstructionSnapshot` refuses capture after the execution has entered `Running`, which prevents later source changes from replacing active execution instructions.

Caller-owned instruction sources are cloned before provenance is stamped with execution and principal context. The snapshot consequently retains the captured source content and provenance even if the caller mutates the original source objects while the provider is running.

The execution snapshot continues to carry the independently captured agent/provider, runtime/host context, identity, policy, and effective resource-capability state. Instruction capture must not rebuild that state through a second snapshot path.

## Conflict representation

`AiInstructionConflict` records a conflict key, all competing source IDs, the selected winner when known, the disposition, a bounded explanation, and detection time. The contract can represent unresolved conflicts as well as deterministic higher-precedence resolution. Conflict objects are evidence; they do not themselves authorize or execute side effects.

## Execution snapshot provenance

`AiInstructionSnapshot` contains cloned source and conflict records. `AgentExecutionSnapshot.InstructionSnapshot` captures a cloned instruction snapshot so later caller-owned mutation cannot alter the captured provenance objects. Execution integration is implemented in 0.954 Slice 4; local Example verification remains the current checkpoint before the phase can advance.

## Boundary rules

- Instruction text is never an authorization boundary.
- Provider adapters receive provider-neutral effective instructions and remain responsible only for transport representation.
- Lower-authority or lower-trust content must not erase higher-authority policy.
- Secrets and sensitive host payloads should not be copied into provenance/evidence fields; diagnostic composition excludes source content by default.
- Skills, Knowledge, Memory, tools, host context, and external content are represented as source types rather than provider-specific prompt formats.
- Disabled/unavailable source state remains diagnosable without making the source authoritative.
- Active execution instruction state is captured before `Running` and is not replaceable after execution start.

## Verification status

Slices 1–3 were verified by the user on 2026-09-08 through `COGNITION INSTRUCTIONS`. Slice 4 execution integration is implemented and has deterministic Example coverage, but the updated Example has not yet been locally executed in this connected environment. The current checkpoint is therefore **Slice 4 awaiting local verification**.
