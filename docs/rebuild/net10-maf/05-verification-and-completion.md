# Verification and Completion

## 1. Verification principle

The rebuild must prove two things separately:

1. required HAgent behavior was preserved or deliberately redesigned;
2. duplicated infrastructure was actually removed and replaced by MAF/MEAI rather than merely hidden behind wrappers.

Repository inspection is not verification. Builds, tests, and Example scenarios must be executed before a capability is declared complete.

## 2. Verification layers

### Layer A — Architecture

Verify:

- dependency graph;
- Core provider neutrality;
- no WinForms or database implementation leakage into Core;
- MAF adapter boundaries;
- one authoritative owner for each durable datum;
- no duplicate generic MAF mechanism without documented semantic difference;
- replaceable boundaries exist for HAgent extensions that close temporary MAF gaps.

### Layer B — Compilation

All target projects compile against `.NET 10`.

No `net481` or `net9.0` target remains in the product solution.

### Layer C — Contract tests

Focused tests must cover:

- execution request/snapshot semantics;
- runtime identity isolation;
- cancellation;
- timeout;
- stale result rejection;
- lifecycle transitions;
- provider failure;
- capability `Supported` / `Unsupported` / `Unknown` handling;
- admission and rate limits;
- structured output validation;
- policy enforcement;
- resource ownership and scope;
- memory isolation;
- learning promotion/rejection;
- persistence/restore;
- goal/plan recovery;
- shutdown behavior.

### Layer D — MAF integration tests

Verify that HAgent correctly composes with current MAF behavior for:

- agent execution;
- MAF middleware;
- tools;
- context providers;
- sessions;
- workflows;
- orchestration;
- HITL;
- checkpoint/resume where used;
- observability;
- supported interoperability protocols.

### Layer E — Example

Every meaningful public capability must have a reproducible `HAgent.Example` scenario, following the existing Example-first rules in `AGENTS.md`.

The Example must exercise the public HAgent surface, not an internal helper that bypasses the architecture.

### Layer F — Host integration

Verify:

- external consumer sample;
- WinForms management scenarios;
- UI Context / Control Adapters;
- HWorld integration boundary;
- host-side action validation and side-effect ownership.

## 3. Performance verification

The rebuilt HAgent must be measured for the scenarios that motivated the persistent runtime architecture.

At minimum:

- many independent runtime instances active concurrently;
- frequent decision cycles;
- provider failure and retry pressure;
- provider rate limits;
- context growth;
- memory/resource ownership churn;
- persistence and restart recovery;
- WinForms resource lifetime.

The objective is not to maximize benchmark numbers. It is to prove that the MAF foundation does not break the runtime model HAgent is intended to provide.

## 4. Dependency verification

Before release:

- enumerate direct and transitive MAF/MEAI dependencies;
- identify preview packages and isolate them where necessary;
- confirm licensing and redistribution requirements;
- record package versions used by the release;
- confirm all selected packages have the required `.NET 10` compatibility;
- remove dependencies that became unnecessary after deduplication.

## 5. No-regression rule

A capability cannot be declared preserved merely because a new implementation exists.

The replacement must be checked against the old required behavior using:

- old tests where still meaningful;
- new contract tests;
- Example scenarios;
- persistence fixtures;
- explicit boundary cases;
- manual host verification where applicable.

Obsolete tests may be removed during redesign, but the behavior they uniquely protected must either be shown unnecessary or receive new coverage.

## 6. MAF convergence test

For every HAgent extension created because MAF lacked a feature, record:

- MAF capability missing at the time;
- HAgent extension introduced;
- exact HAgent contract isolated;
- replacement criteria;
- how a future MAF implementation will be detected;
- migration path when MAF becomes sufficient.

This prevents temporary gaps from becoming permanent architecture.

## 7. Completion checklist

The rebuild is complete only when all are true:

- HAgent targets .NET 10 only;
- required HAgent capabilities are available;
- no required functionality was silently lost;
- MAF is the foundation for generic agent/workflow/tool pipeline functionality where equivalent;
- duplicate HAgent implementations of equivalent MAF capabilities are removed;
- every HAgent-only extension has a reason to exist;
- partial-gap extensions are replaceable;
- all unresolved owner decisions are closed;
- all meaningful capabilities have tests and Examples;
- persistence and recovery are verified;
- HWorld integration is verified;
- WinForms scenarios are verified;
- documentation reflects the final architecture;
- generated root plan/roadmap views are synchronized;
- package dependency graph is clean;
- no known migration shim or compatibility path remains solely to preserve the old architecture.

## 8. Release evidence

The release record should contain:

- exact .NET SDK version;
- exact MAF/MEAI package versions;
- build result;
- focused and full test results;
- Example scenarios executed;
- supported target/runtime matrix;
- persistence verification result;
- performance measurements relevant to the runtime goals;
- final architecture decision record;
- list of intentionally removed duplicated components.
