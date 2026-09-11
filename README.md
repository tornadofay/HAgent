# HAgent

**Provider-neutral AI agent, cognition, execution, memory, learning, and host-integration library for .NET applications.**

HAgent provides reusable infrastructure for applications that need LLM-backed agents without taking ownership of the host application's domain state, scheduling, authorization, persistence, or side effects.

> **Current implementation milestone: 0.9575 — Knowledge, Skills, Memory Governance + Learning, Slice 8 in progress.**
>
> Major foundations through 0.957 are implemented and verified. The remaining roadmap builds toward capability-aware execution and a persistent cognitive runtime.
>
> **Targets:** .NET Framework 4.8.1 and .NET 9.

## What HAgent provides

HAgent separates generic agent infrastructure from the application that hosts the agent.

```text
Host application
        |
        |  request + bounded context / observations
        v
+------------------------------------------------+
|                    HAgent                      |
|                                                |
|  Agent profiles / runtime instances            |
|  Execution / providers / execution targets     |
|  Skills / Knowledge / Memory / Learning        |
|  Tools / structured output / policy             |
|  Events / context / observability / evaluation |
|  Lifecycle / intervention / persistence        |
|  Persistent cognition                          |
+------------------------------------------------+
        |
        v
Host-owned domain state, authorization,
scheduling, persistence, and side effects
```

The host remains authoritative for its own business or simulation state and for the actual side effects applied to that state.

## Architecture

HAgent has two complementary levels:

```text
HAgent
│
├── Execution and agent infrastructure
│   ├── provider/model execution
│   ├── agent profiles and runtime instances
│   ├── context and resource integration
│   ├── tools and structured output
│   ├── policy and authorization boundaries
│   ├── lifecycle / cancellation / timeout safety
│   └── telemetry and evaluation
│
└── Persistent Cognitive Runtime
    ├── persistent per-agent state
    ├── observations and beliefs
    ├── bounded decision workspace
    ├── goals and intentions
    ├── deterministic decisions
    ├── bounded reasoning
    ├── plans and recovery
    ├── experience and learning integration
    └── lifecycle and restart recovery
```

The ordinary execution API remains useful on its own. Persistent cognition builds on the same provider-neutral execution contracts rather than replacing them.

Each persistent runtime agent owns its authoritative cognitive state. Background LLM, tool, or retrieval work returns typed results to that owner; it does not directly mutate authoritative state. State mutation is serialized per runtime agent, while independent runtime agents may operate concurrently.

## Basic usage

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Plain string messaging is the convenience form. The canonical execution boundary is `AgentExecutionRequest`, which can carry multiple messages, bounded host context, host correlation identity, execution options, and structured-output requirements.

## Knowledge, Skills, Memory, and Learning

These are first-class HAgent resources with shared identity, scope, ownership, provenance, lifecycle, versioning, policy, and persistence rules.

- **Skills** are reusable, versioned capabilities and procedures with governed dependencies and constraints.
- **Knowledge** is reusable information that can be retrieved and governed. **Wiki** is one managed persistent knowledge source within the broader knowledge system.
- **Memory** stores scoped experience and state, including working, episodic, semantic, procedural, and extensible memory families.
- **Learning** turns experience into typed candidates. Candidates must pass the applicable policy, validation, authorization, and promotion boundaries before they can become authoritative resources.

Runtime instances can inherit profile resource settings and apply runtime-only `Inherit` / `Enabled` / `Disabled` overrides without changing the persistent profile.

## Providers and execution targets

HAgent separates provider integration from agent behavior.

A provider adapter can expose execution, model discovery, capability information, provider-native identifiers, operational information, and other supported metadata. Missing information remains explicitly unknown rather than being invented.

HAgent distinguishes:

```text
Provider
    = provider/service integration

Logical Model
    = provider-independent model identity when known

Execution Target
    = concrete provider + account/project + endpoint + model/deployment

Capability
    = what the target supports

Operational State
    = quota / rate / concurrency / health / availability

Cost State
    = Free / FreeWithinQuota / Paid / Unknown

Execution Planner
    = selects and admits the compatible concrete target
```

Capability, permission, health, quota, capacity, and cost are separate concerns. A target with unknown information is not silently treated as supported or free.

The execution planner is the single authority for concrete provider/model execution-target selection. Cognition requests the reasoning capability it needs; it does not hard-code provider or model names.

## Host integration

HAgent is designed to work with desktop applications, business software, services, games, simulations, automation, developer tools, and other host environments.

The host owns:

- domain objects and authoritative state;
- observations and external events;
- application or simulation scheduling;
- authorization and approval rules;
- host persistence;
- real-world or application side effects.

HAgent provides:

- agent profiles and long-lived runtime instances;
- asynchronous execution;
- cancellation, timeout, and stale-result protection;
- scoped memory, Knowledge, Skills, and governed Learning;
- structured tools and tool execution boundaries;
- context ingestion and bounded context handling;
- provider/model discovery and execution-target assessment;
- lifecycle and intervention contracts;
- observability and evaluation;
- persistent per-agent cognition.

HAgent does not require the host to adopt a particular domain model, scheduler, persistence system, authorization framework, or UI technology.

## HWorld integration

HWorld is an external consumer of HAgent, not a dependency of HAgent.

HWorld remains authoritative over world state, physics, simulation time, sensors, observations, action validation, rendering, and world-side effects. HAgent supplies generic agent execution and cognition through the external decision boundary.

A typical integration is:

```text
HWorld observation/event
        ↓
HWorld/HAgent integration boundary
        ↓
HAgent runtime agent instance
        ↓
provider/model + optional tools/memory
        ↓
provider-neutral decision or action request
        ↓
HWorld validation
        ↓
world state
```

Multiple HWorld actors can use independent HAgent runtime instances concurrently. HAgent does not require HWorld to depend on HAgent in order to run.

## Storage

HAgent persists its own data separately from the host application's business database.

Supported storage backends are:

- **File** — HAgent-owned files beneath the host executable in `HAgentData`;
- **SQL Server** — a dedicated HAgent-owned database, normally named `<application-name>-ai`;
- **MySQL** — a dedicated HAgent-owned database, normally named `<application-name>-ai`.

HAgent-owned storage covers provider configuration, models and execution targets, agents, tools, memory, conversations, Skills, Knowledge/Wiki, learning candidates, policies, runtime metadata, and execution audit data.

Provider API keys are stored with provider configuration and encrypted at rest. HAgent storage never grants implicit access to the host application's business database.

## WinForms integration

`HAgent.WinForms` provides the HAgent management surface and the generic **UI Context / Control Adapters** integration boundary.

It can work with Forms and UserControls, inspect bindings and native data sources, discover bounded relationships, and inspect bounded application objects under host policy.

`DataTable` is optional. Native/bound sources, bounded projections, paging, and lazy adapters are supported so HAgent does not require one specific data representation.

## Management surface

The management architecture covers:

```text
General
    system defaults and policy settings

Providers / Models
    provider connections, discovery, targets, capabilities,
    limits, availability, cost, and execution compatibility

Agents
    agent selection, Skills, Knowledge, Memory, Learning,
    effective configuration, and runtime settings

Cognition
    persistent runtime state, decisions, goals, plans,
    memory, learning, lifecycle, and diagnostics

Learning Review
    inspect governed learning candidates and approvals

Knowledge / Wiki
    manage knowledge resources and relationships

Skills
    manage versioned skill definitions and relationships

Storage
    File / SQL Server / MySQL configuration and verification
```

Management UI is built over the same authoritative HAgent state and policy boundaries used by the runtime. It does not create a parallel persistence or authorization system.

## Security and authority

The model is a requester, not an authority.

Authorization, permissions, approvals, capability checks, budgets, cancellation, lifecycle enforcement, and host-side validation are enforcement boundaries. Prompt text is not an authorization boundary.

Tool definitions are separate from executable handlers. Handlers remain runtime-owned and are not serialized into configuration.

Learning output is not automatically authoritative. Published Memory, Knowledge, or Skills require their governed promotion boundaries.

## Example application and tests

`HAgent.Example` is the manual developer and verification application. `HAgent.Tests` provides automated contract and boundary verification.

Meaningful capabilities have corresponding test coverage and reproducible Example scenarios using public HAgent APIs. Examples are organized by architecture/capability rather than by implementation class name.

## Current implementation position

The roadmap currently stands at:

```text
0.951  Identity / Tenancy / User Context                    complete
0.952  First-Class Event Subsystem                          complete
0.953  Unified Policy Engine                                complete
0.954  Prompt / Instruction Governance                       complete
0.955  Context Engineering                                  complete
0.956  Observability / Distributed Tracing                  complete
0.957  Evaluation / Quality Measurement                      complete
0.9575 Knowledge / Skills / Memory Governance + Learning   current
0.9576 Learned Resource Reliability + Adaptation            planned
0.958  Agent Lifecycle / Health                              planned
0.9591 Goal / Plan Persistence / Recovery                    planned
0.959  Human-in-the-Loop / Intervention                      planned
0.9592 Provider Ecosystem / Adapter Lifecycle                planned
0.96.x Configuration / Storage / Portability                 planned
0.96   Capability-Aware Execution                            planned
0.97   Persistent Cognitive Runtime                          planned
0.10   Workspaces / Routing / Chat                           planned later
1.0    Collaboration / Workflows                             planned later
```

The current implementation milestone is **0.9575 Slice 8 — Canonical Learning Lifecycle Gate**. Future phases may contain architectural evidence and documented decisions without changing the current implementation milestone.

See [`roadmap.md`](roadmap.md) for the ordered roadmap and [`docs/plan/`](docs/plan/) for current implementation state.

## Project structure

- `HAgent.Core` — provider-neutral agent, execution, runtime, cognition, context, memory, Knowledge, Skills, Learning, tools, events, policy, tracing, and evaluation contracts.
- `HAgent.Providers.OpenAICompatible` — OpenAI-compatible provider transport and capabilities.
- `HAgent.Storage.File` — file-based HAgent persistence.
- `HAgent.Storage.SqlServer` — HAgent-owned SQL Server persistence and schema bootstrap.
- `HAgent.Storage.MySql` — HAgent-owned MySQL persistence and schema bootstrap.
- `HAgent.WinForms` — management UI and UI Context / Control Adapters.
- `HAgent.Example` — manual verification host and capability laboratory.
- `HAgent.Tests` — automated tests.
- `samples/HAgent.ExternalConsumer` — external-host integration sample.

## Documentation

- [`docs/architecture/`](docs/architecture/) — stable architecture and boundaries.
- [`docs/plan/`](docs/plan/) — master direction, current state, and active implementation.
- [`docs/roadmap/`](docs/roadmap/) — ordered implementation path.
- [`docs/storage.md`](docs/storage.md) — storage-specific details.
- [`AGENTS.md`](AGENTS.md) — engineering rules and project invariants.

Root [`plan.md`](plan.md) and [`roadmap.md`](roadmap.md) are generated views from the modular source documents.

## Supported targets

- .NET Framework 4.8.1
- .NET 9

## License

MIT — see [LICENSE](LICENSE).
