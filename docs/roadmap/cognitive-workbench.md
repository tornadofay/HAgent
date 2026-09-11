# 0.97 Subdocument — Cognitive Runtime Workbench

This document is part of **Phase 0.97 — Persistent Cognitive Runtime**, primarily supporting Slice 8 management, diagnostics, observability, and production verification.

HAgent.WinForms must add a top-level Cognitions view for active runtime instances. It provides complete inspection of current runtime cognition: beliefs, goals, intentions, plans, attention, working state, memory, knowledge, skills, events, executions, learning, lifecycle, and history.

The workbench is a diagnostic/management surface over the authoritative runtime-agent state owner. It must not create a second cognitive-state store, second policy evaluator, second execution planner, or independent persistence model.

All edits flow through public HAgent runtime state-transition/intervention APIs. The UI never writes directly to persistence and must preserve owner/revision/stale-result rules.
