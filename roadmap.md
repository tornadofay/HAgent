# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

HAgent is a general-purpose agent runtime that can serve simple chat, business applications, games, simulations, and other host environments.

## Status

- 0.1 Foundation — complete
- 0.2 Runtime foundation — complete
- 0.3 Memory + Context — foundation complete
- 0.4 Capabilities + Response Normalization — foundation complete
- 0.5 Tool foundation — verified; hardening remains
- 0.6 Safety + authorization foundation — partial
- 0.7 UI Context + Data Discovery — complete
- 0.8 Data Access + Authorization — active
- 0.9 Agent Runtime + Workspaces + Chat — planned after the security/data foundation
- 1.0 Collaboration + Workflows — planned
- Later: provider/extensibility/developer platform work and stable 1.0 release hardening

## Dependency order

```text
Provider/runtime foundation
        ↓
Memory/context
        ↓
Tools
        ↓
UI/data discovery
        ↓
Data access + authorization
        ↓
Runtime agent instances
        ↓
Workspaces + routing + visible collaboration
        ↓
Collaboration/workflows
        ↓
Extensibility + release hardening
```

## Documentation rule

Architecture describes what HAgent is. The implementation plan describes what is being built now. This roadmap describes what comes next. Engineering invariants live in `AGENTS.md`.

## Goal
Turn the verified data-discovery/query contracts into safe application and database access.

## Sequence

- [ ] Application-owned `IDataQuerySource` adapter.
- [ ] Authoritative schema/field allow-list.
- [ ] Separate discovery, query, export, and write permissions.
- [ ] Host authorization callbacks.
- [ ] Limits, cancellation, timeout, and resource budgets.
- [ ] Restricted SQL Server read adapter.
- [ ] Restricted MySQL read adapter.
- [ ] Database audit/correlation metadata.
- [ ] Live Example with runtime-only connection fields.

## Boundaries

No raw SQL tool. No implicit access to every table/field. Database credentials remain in the secret system. UI discovery and application-object metadata never grant database authorization.

## Exit criterion

A host can authorize a structured read against its application/database source, execute it through a restricted adapter, and observe bounded results and failures through the public Example verification surface.

## Goal
Separate reusable agent profiles from live runtime agent identities so one configured profile can produce many independent agents concurrently.

## Sequence

- [ ] Runtime agent instance model with stable instance ID and profile ID.
- [ ] Explicit scope: Application, Workspace, Context/Form, Session, Task, Ephemeral.
- [ ] Runtime-specific context and provider/model overrides.
- [ ] Runtime-instance memory ownership.
- [ ] Independent concurrent execution.
- [ ] External scheduling, cancellation, timeout, and stale-result protection.
- [ ] Explicit retirement and shutdown behavior.
- [ ] Optional persistence for recovery/collaboration.
- [ ] HWorld adapter verification.

## Boundaries

Agent roles are configuration/binding concepts, not separate agent classes. Dynamically created specialists are not permanent configuration records by default.

## Exit criterion

A host can create multiple independent runtime agents from configured profiles, execute them concurrently with isolated identity/context/memory, and safely retire or persist them according to host policy.

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

## Collaboration

- [ ] Agent delegation/handoffs as first-class routing operations.
- [ ] Explicit shared/private workspace context.
- [ ] Parallel agent work with bounded collaboration budgets.
- [ ] Human intervention and approval points.
- [ ] Agent lifecycle states for active, disabled, retired, and deleted definitions/instances.

## Workflows

- [ ] Task/job model.
- [ ] Planning, execution, and verification stages.
- [ ] Multi-step workflows and background execution.
- [ ] Pause/resume and durable checkpoints.
- [ ] Event-triggered execution.
- [ ] Per-step cancellation, timeout, retry, approval, and budget policies.

These capabilities remain generic orchestration infrastructure. Hosts define their domain actions and business rules.

## Provider ecosystem

- [ ] Additional provider adapters.
- [ ] Multimodal and embedding adapters where justified.
- [ ] Provider contract harness.

## Extensibility

- [ ] Provider/tool/UI-adapter/storage extension model.
- [ ] Extension validation and failure isolation.
- [ ] External secret stores/rotation.

## Developer platform

- [ ] Optional DI integrations.
- [ ] Optional interoperability integrations.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public capabilities.
- [ ] Simulation/test mode for external consumers such as HWorld.

## Release hardening

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packaging and release process.
- [ ] Security/provider/tool/memory integration coverage.
- [ ] Documentation and migration guidance.

`.NET 10` remains a future target after the development environment and compatibility policy are ready.
