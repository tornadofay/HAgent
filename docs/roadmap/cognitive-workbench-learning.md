# 0.97 Subdocument — Cognitive Workbench History and Learning

This document is part of **Phase 0.97 — Persistent Cognitive Runtime**, supporting its workbench and diagnostics.

Show the complete cognitive timeline: events, belief changes, attention changes, goal and intention changes, plan revisions, impasses, deliberation, executions, outcomes, memory/experience creation, learning decisions, sleep/wake and recovery, including user interventions.

Show learning as:

```text
Experience
   ↓
Memory / bounded evidence
   ↓
Reflection / learning input
   ↓
Typed candidate
   ↓
0.9575 validation / governance / promotion
   ↓
Authoritative resource version
   ↓
0.9576 reliability / applicability / forgetting
```

Candidates must remain visibly distinct from authoritative versions. The workbench must not imply that repeated LLM reasoning is automatically a Skill, Knowledge item, Memory record, or Policy change.

Show the active cognitive strategy and version, such as Adaptive Hybrid Cognition (AHC), when the runtime exposes such metadata. Future strategies must use the same generic workbench while allowing strategy-specific diagnostics.

Historical state is initially read-only. Future experimentation may branch from checkpoints, but a branch must not silently replace the live runtime's authoritative state.
