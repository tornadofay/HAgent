# Instruction Governance

## Purpose

HAgent represents instruction-bearing content through provider-neutral source contracts before any provider-specific prompt formatting occurs. The contract preserves authority, trust, provenance, scope, lifecycle, and conflict evidence so later composition can remain deterministic and reviewable.

## Instruction source

`AiInstructionSource` is the canonical normalized source record for instruction-bearing content. It contains:

- `SourceType` — system policy, agent instruction, Skill, Knowledge, Memory, tool description, runtime/host context, user input, external content, or model-generated content;
- `Authority` — the explicit authority boundary relevant to precedence;
- `TrustLevel` — evidence/trust metadata kept separate from authority;
- `Scope` — canonical generic scope metadata using Global, Tenant, User, Workspace, Agent, Runtime, or Execution where applicable;
- `Lifecycle` — active, disabled, expired, or revoked;
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

## Deterministic precedence

`AiInstructionPrecedence.Compare` compares sources in this order:

1. explicit authority;
2. explicit priority;
3. trust level;
4. scope specificity;
5. deterministic source-type precedence;
6. stable source ID ordering as the final tie-breaker.

Lifecycle eligibility is evaluated separately at a supplied point in time. This keeps time-dependent activation/expiry outside the precedence ordering itself.

`SelectWinner(sources, at)` selects only sources active at the supplied time and then applies the same deterministic comparator. There is no declaration-order or prompt-position authority rule.

## Canonical composition

`AiInstructionComposer` is the single provider-neutral composition boundary for instruction-bearing sources. It validates each source, excludes disabled/expired/revoked or otherwise invalid sources, records bounded diagnostics for excluded input, groups competing active sources by `ConflictKey`, and selects one deterministic winner per conflict group using `AiInstructionPrecedence`.

Sources without a conflict key remain additive. The resulting `AiInstructionCompositionResult` contains:

- `Snapshot` — cloned authoritative sources plus conflict evidence;
- `ComposedText` — deterministic provider-neutral instruction text;
- `Diagnostics` — bounded exclusion diagnostics that identify source metadata/reason without copying source content.

`SystemPromptComposer` remains a small compatibility-shaped façade for the existing runtime API and delegates to `AiInstructionComposer`; it does not implement a second precedence or conflict mechanism.

This slice intentionally composes instruction-bearing content only. Mature Skill/Knowledge/Memory retrieval and external-content governance belong to later resource/context slices.

## Conflict representation

`AiInstructionConflict` records a conflict key, all competing source IDs, the selected winner when known, the disposition, a bounded explanation, and detection time. The contract can represent unresolved conflicts as well as deterministic higher-precedence resolution. Conflict objects are evidence; they do not themselves authorize or execute side effects.

## Execution snapshot provenance

`AiInstructionSnapshot` contains cloned source and conflict records. `AgentExecutionSnapshot.InstructionSnapshot` captures a cloned instruction snapshot so later caller-owned mutation cannot alter the captured provenance objects. Prompt assembly and execution integration consume this contract through the existing `SystemPromptComposer` delegation path without introducing a provider-specific prompt engine.

## Boundary rules

- Instruction text is never an authorization boundary.
- Provider adapters receive provider-neutral effective instructions and remain responsible only for transport representation.
- Lower-authority or lower-trust content must not erase higher-authority policy.
- Secrets and sensitive host payloads should not be copied into provenance/evidence fields; diagnostic composition excludes source content by default.
- Skills, Knowledge, Memory, tools, host context, and external content are represented as source types rather than provider-specific prompt formats.

## Verification status

Slice 1 source/authority contracts were locally verified by the user through `COGNITION INSTRUCTIONS` before composition was added. Slice 2 composition implementation is present in `HAgent.Core` with deterministic Example coverage, but its new composition checks have not yet been locally executed in this connected environment.
