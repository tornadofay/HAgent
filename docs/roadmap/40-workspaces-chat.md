# Phase 0.9 — Workspaces, Routing, and Chat

## Goal
Allow a host to put a user and multiple runtime agents into one shared conversation without sending every message to every agent.

## Sequence

- [ ] Workspace model independent of UI.
- [ ] Participant lifecycle and default recipient.
- [ ] Direct user-to-agent addressing.
- [ ] Agent-to-agent addressed messaging.
- [ ] Visible delegation and specialist responses.
- [ ] Host-configurable addressing syntax.
- [ ] Conversation ordering, correlation, and causation metadata.
- [ ] Visibility policy for internal messages.
- [ ] Loop protection and collaboration budgets.
- [ ] Optional shared/persistent workspace state.
- [ ] WinForms chat surface with global agent selection.

## Coordination pattern

The common desktop pattern is a coordinator plus specialists. The coordinator is simply the workspace's configured default recipient; HAgent does not hard-code business roles.

Specialists may represent an entire domain/table/subsystem rather than one record. They receive context according to host policy and can report known facts, inference, unknowns, and authorization limits honestly.

## Boundaries

An unaddressed user message goes only to the workspace default recipient. An addressed message goes only to its target unless an explicit policy invokes additional participants. Agent-to-agent work is visible as workspace messages when enabled.
