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

## Conflict representation

`AiInstructionConflict` records a conflict key, all competing source IDs, the selected winner when known, the disposition, a bounded explanation, and detection time. The contract can represent unresolved conflicts as well as deterministic higher-precedence resolution. Conflict objects are evidence; they do not themselves authorize or execute side effects.

## Execution snapshot provenance

`AiInstructionSnapshot` contains cloned source and conflict records. `AgentExecutionSnapshot.InstructionSnapshot` captures a cloned instruction snapshot so later caller-owned mutation cannot alter the captured provenance objects. Prompt assembly and execution integration consume this contract in later 0.954 slices.

## Boundary rules

- Instruction text is never an authorization boundary.
- Provider adapters receive provider-neutral effective instructions and remain responsible only for transport representation.
- Lower-authority or lower-trust content must not erase higher-authority policy.
- Secrets and sensitive host payloads should not be copied into provenance/evidence fields; diagnostic redaction remains an observability concern.
- Skills, Knowledge, Memory, tools, host context, and external content are represented as source types rather than provider-specific prompt formats.

## Planned follow-on

Slice 2 will use these contracts for canonical instruction composition and explicit conflict/invalid-source handling. Slice 1 deliberately does not introduce a second prompt engine or provider-specific message format.
