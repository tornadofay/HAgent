# Phase 0.954 — Prompt and Instruction Governance

## Status

**Planned architectural foundation before persistent cognition and advanced learning.**

## Goal

Define trusted instruction layers and provenance so HAgent can safely combine system policy, agent instructions, Skills, Knowledge, Memory, tools, runtime context, user input, and externally retrieved content.

## Requirements

1. [ ] Define normalized instruction/source records with source type, authority/trust level, provenance, scope, and lifecycle metadata.
2. [ ] Define deterministic instruction composition and precedence rules.
3. [ ] Distinguish trusted policy/instructions from untrusted retrieved content and ordinary user/model-generated text.
4. [ ] Prevent lower-authority content from silently overriding higher-authority policy.
5. [ ] Ensure prompts never substitute for authorization, permissions, approval, or other code-enforced controls.
6. [ ] Track the instruction sources contributing to an execution snapshot.
7. [ ] Support Skill, Knowledge, Memory, tool-description, runtime, and host-context instructions without creating provider-specific prompt formats in Core.
8. [ ] Define handling for instruction conflicts, unsafe/invalid sources, disabled resources, and unavailable source content.
9. [ ] Keep secrets and sensitive host data out of diagnostic instruction traces by default.
10. [ ] Add deterministic Example verification for precedence, untrusted-content handling, conflicts, disabled resources, and execution-snapshot provenance.

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

The exact precedence rules are implementation-defined, but authority and provenance must remain explicit.