# HAgent .NET 10 + MAF Rebuild — Executive Report

**Date:** 2026-09-14  
**Status:** Architectural proposal / rebuild baseline  
**Target:** .NET 10 only  
**Foundation:** Microsoft Agent Framework (MAF) + Microsoft.Extensions.AI where appropriate

## 1. Executive decision

HAgent should be rebuilt as a **.NET 10-only provider-neutral cognitive runtime that uses Microsoft Agent Framework as a foundational implementation platform rather than competing with it at every layer**.

The rebuild should start from **Phase 1**, not from the current 0.9591 implementation state. The existing repository is an inventory of functionality, architecture decisions, tests, examples, storage behavior, UI, and integration expectations that must be accounted for. The current code is not automatically preserved when MAF offers a better implementation.

The target relationship is:

```text
Host application / HWorld
          |
          v
+----------------------------------------+
|                 HAgent                 |
|                                        |
| Cognitive Runtime                      |
| Agent profile/runtime identity         |
| Learning governance                    |
| Knowledge / Memory / Skills ownership  |
| Execution planning + admission         |
| Lifecycle / recovery semantics         |
| Host authority boundary                |
| HAgent persistence                     |
| Evaluation / project-specific policy   |
| WinForms integration                   |
+--------------------+-------------------+
                     |
                     v
+----------------------------------------+
|       Microsoft Agent Framework        |
| Agents / Workflows / Middleware        |
| Tools / orchestration / HITL           |
| Context/session mechanisms             |
| MAF-native interoperability            |
+--------------------+-------------------+
                     |
                     v
+----------------------------------------+
|      Microsoft.Extensions.AI           |
| IChatClient / embeddings / AI contracts|
+----------------------------------------+
                     |
                     v
             Provider / model layer
```

MAF 1.0 is production-ready for .NET and Python and explicitly covers agents, multi-agent orchestration, workflows, middleware, tools, memory/context, human-in-the-loop, checkpoint/resume, and interoperability. Current documentation also exposes an agent pipeline around middleware, context providers, and `IChatClient`. citeturn425874search5turn425874search1turn425874search4

As of August 31, 2026, the `Microsoft.Agents.AI` 1.20.0 package is available and is compatible with `net10.0`; current MAF source also contains .NET 10 targets. citeturn479880search4turn479880search6turn479880search0

## 2. Why rebuild instead of port

A straight .NET Framework 4.8.1 → .NET 10 port would preserve too much internal duplication and would force HAgent to continue owning infrastructure that MAF now provides.

The correct transformation is:

```text
Current HAgent
    |
    | inventory every capability
    v
MAF capability analysis
    |
    +-- already equivalent --> reuse MAF; remove duplicate HAgent implementation
    |
    +-- partially equivalent --> HAgent supplies replaceable missing layer
    |
    +-- different semantics --> present architectural alternatives; owner decides
    |
    +-- HAgent-specific --> keep and build above MAF
    v
New HAgent .NET 10 architecture
```

The rebuild must not create permanent parallel implementations merely because the old implementation already exists.

## 3. Hard requirements

### 3.1 Platform

- .NET Framework 4.8.1 support is removed.
- .NET 9 support is removed.
- The target is `.NET 10` only.
- Windows-specific functionality remains in explicitly Windows-targeted projects; Core remains platform-neutral where practical.

### 3.2 Foundation

- MAF is a first-class foundational dependency.
- `Microsoft.Extensions.AI` is used where its lower-level abstractions are the correct foundation, especially `IChatClient` and embeddings.
- HAgent does not recreate an MAF capability merely to retain an old implementation.

`IChatClient` is the standard chat-client abstraction in `Microsoft.Extensions.AI`; current .NET 10 package documentation shows the abstraction available for .NET 10 and supports concurrent use according to the interface contract. citeturn425874search0turn425874search6

### 3.3 No loss of required HAgent functionality

Every current meaningful capability must map to one of these outcomes:

1. retained as HAgent-owned behavior;
2. implemented using MAF;
3. implemented as an HAgent layer above MAF;
4. replaced by a MAF facility after semantic equivalence is demonstrated;
5. deliberately redesigned because the existing mechanism is obsolete, with the capability itself preserved when it remains a project requirement.

### 3.4 Replaceable HAgent extensions

If HAgent fills a current MAF gap, the extension must be behind a replaceable boundary. The implementation must not make a temporary HAgent workaround part of the permanent internal architecture.

The intended lifecycle is:

```text
HAgent contract
     |
     +-- current MAF implementation
     |
     +-- HAgent extension for missing semantics
     |
     +-- future MAF implementation
            |
            +--> replace extension without redesigning HAgent contract
```

### 3.5 Explicit decision points

When MAF supplies a lower-level facility but HAgent has a materially different desired contract, the report will not silently choose one. The alternatives are presented with pros/cons and implementation/maintenance impact; the project owner chooses.

## 4. What HAgent remains responsible for

The strongest HAgent-specific responsibilities are the ones that turn agent infrastructure into a durable application subsystem:

- persistent per-agent cognitive state;
- stable runtime-agent ownership and isolation;
- goals, intentions, plans, revision, and restart/recovery semantics;
- governed learning candidates and promotion;
- resource ownership, scope, provenance, versioning, policy, and lifecycle;
- explicit separation of memory, Knowledge, Wiki, and Skills;
- capability-aware execution planning and target admission;
- provider/account/project/endpoint/model operational state;
- host-controlled authorization and side-effect boundaries;
- lifecycle semantics that survive cancellation, timeout, shutdown, and late provider completion;
- HAgent-owned storage and configuration portability;
- generic WinForms UI Context / Control Adapter integration;
- HWorld integration without making HWorld a dependency.

These are consistent with the current HAgent repository's documented direction and project invariants. fileciteturn1file0L2-L2

## 5. What should move to MAF

The rebuild should aggressively remove duplicate implementations where MAF already supplies suitable semantics. High-probability candidates include:

- agent invocation pipeline mechanics;
- agent middleware;
- workflow graph infrastructure;
- standard tool/function calling plumbing;
- common agent orchestration patterns;
- common context/session mechanisms where the semantics match;
- human-in-the-loop workflow mechanics where HAgent does not need stronger semantics;
- MAF-native A2A/MCP integration;
- standard agent pipeline hooks;
- lower-level provider interaction through `IChatClient`.

MAF documents sequential, concurrent, handoff, group-chat, and magentic orchestration, plus HITL approval/request-info and checkpoint/resume. Those should be treated as existing foundation rather than invitations to maintain a second HAgent workflow engine. citeturn425874search12turn425874search2

## 6. What must not be blindly delegated to MAF

MAF should not automatically become authoritative for requirements that define HAgent's identity. Examples:

- HAgent's cognitive state model;
- host/application authority;
- HAgent resource ownership/scope;
- governed learning promotion;
- HAgent execution-target planner;
- persistent cognitive lifecycle;
- HAgent storage partitioning and persistence contract;
- the HAgent/host/HWorld boundary;
- existing HAgent-specific security and authority invariants.

The point is not to keep HAgent code for its own sake. The point is to keep only requirements that MAF does not already satisfy at the required semantic level.

## 7. Important current repository discrepancy

The repository currently documents more than one checkpoint view. `README.md` describes 0.9591 as current, while `docs/plan/00-current-state.md` still describes 0.958 Slice 3 as active and pending verification. fileciteturn1file0L2-L2 fileciteturn7file0L2-L2

This rebuild report does not silently resolve that discrepancy. Phase 1 must establish a single authoritative rebuild baseline before implementation work begins.

## 8. Expected result

The completed product should be smaller in duplicated infrastructure and stronger in architectural separation:

```text
                 HAgent
                     |
        +------------+-------------+
        |                          |
 HAgent-specific              MAF foundation
 cognition/runtime             agent infrastructure
        |                          |
        +------------+-------------+
                     |
          Microsoft.Extensions.AI
                     |
                  Providers
```

The success condition is not "HAgent contains less code." It is:

> **HAgent contains no duplicate implementation where MAF already satisfies the requirement, while all HAgent-specific project goals remain available through a coherent .NET 10 architecture.**

## 9. Non-goals

This report does not authorize immediate code changes outside the rebuild documentation. It does not declare the architecture implemented, tested, or production-ready. Phase 1 is an architecture/dependency/code-inventory phase and must complete before migration implementation begins.

## 10. Primary references

- Microsoft Agent Framework documentation: https://learn.microsoft.com/en-us/agent-framework/
- MAF agents: https://learn.microsoft.com/en-us/agent-framework/concepts/agents/
- MAF workflows: https://learn.microsoft.com/en-us/agent-framework/journey/workflows
- MAF middleware/pipeline: https://learn.microsoft.com/en-us/agent-framework/agents/agent-pipeline
- Microsoft.Extensions.AI: https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
- `Microsoft.Agents.AI` package: https://www.nuget.org/packages/Microsoft.Agents.AI/
