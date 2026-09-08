# Phase 0.954 — Prompt and Instruction Governance

## Status

**Verified and complete on 2026-09-08.**

## Goal

Define trusted instruction layers and provenance so HAgent can safely combine system policy, agent instructions, Skills, Knowledge, Memory, tools, runtime context, user input, and externally retrieved content.

## Requirements

1. [x] Define normalized instruction/source records with source type, authority/trust level, provenance, scope, and lifecycle metadata.
2. [x] Define deterministic instruction composition and precedence rules.
3. [x] Distinguish trusted policy/instructions from untrusted retrieved content and ordinary user/model-generated text.
4. [x] Prevent lower-authority content from silently overriding higher-authority policy.
5. [x] Ensure prompts never substitute for authorization, permissions, approval, or other code-enforced controls.
6. [x] Track the instruction sources contributing to an execution snapshot.
7. [x] Support Skill, Knowledge, Memory, tool-description, runtime, and host-context instructions without creating provider-specific prompt formats in Core.
8. [x] Define handling for instruction conflicts, unsafe/invalid sources, disabled resources, and unavailable source content.
9. [x] Keep secrets and sensitive host data out of diagnostic instruction traces by default.
10. [x] Add deterministic Example verification for precedence, untrusted-content handling, conflicts, disabled resources, and execution-snapshot provenance.

## Verification

The user verified `COGNITION INSTRUCTIONS` on both .NET Framework 4.8.1 and .NET 9, covering source creation/validation, authority-vs-trust separation, precedence, explicit priority, conflict representation, provenance-preserving snapshot cloning, canonical additive composition, deterministic conflicts, disabled/invalid containment, sensitive-content exclusion, trusted-resource authority/trust, external-content boundaries, unavailable-source handling, lower-authority override resistance, effective snapshot capture, provider transport parity, caller-source mutation isolation, and execution/principal provenance.

The user also verified the deterministic runtime boundary examples on the supported targets:

- .NET 9: `RUNTIME TERMINAL STATE`, `RUNTIME CONCURRENCY`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.
- .NET Framework 4.8.1: `RUNTIME INSTANCES`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.

These Examples use local/in-memory provider infrastructure where deterministic behavior is required; configuration-driven `RUNTIME EXECUTION` remains a separate live-provider host example and is not used as a deterministic milestone gate.

## Architectural outcome

```text
System / Policy
      ↓
Agent instructions
      ↓
Skills / trusted resources
      ↓
Knowledge / Memory / tool descriptions
      ↓
Runtime + host context
      ↓
User / external content
      ↓
Provider request
```

The exact precedence rules are implementation-defined, but authority and provenance remain explicit. Execution captures the effective instruction snapshot before provider transport, and provider adapters receive the same composed effective instruction text rather than rebuilding instruction governance.