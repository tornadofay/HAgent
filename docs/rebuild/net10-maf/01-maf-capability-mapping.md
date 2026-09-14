# MAF Capability Mapping

## Purpose

This document defines how the rebuild decides whether an HAgent capability should be implemented by MAF, delegated to MAF, wrapped by HAgent, or retained as HAgent-specific architecture.

## Decision classes

| Class | Rule | Result |
|---|---|---|
| MAF-equivalent | MAF provides the required semantics and lifecycle | Remove duplicate HAgent implementation and use MAF |
| MAF-partial | MAF supplies some but not all required semantics | Add a replaceable HAgent extension above MAF |
| Different contract | MAF primitive exists but HAgent's required semantics differ materially | Present alternatives; owner chooses |
| HAgent-specific | No meaningful MAF equivalent | Keep as HAgent capability |
| Obsolete | Old HAgent mechanism no longer represents a required behavior | Remove during rebuild |

## Capability matrix

### Agent execution

MAF provides agent abstractions, chat-client-backed agents, sessions, run options, middleware, context providers, tools, structured outputs, streaming, and a layered execution pipeline.

HAgent should retain a canonical host-facing execution contract only where it adds HAgent-specific identity, policy, execution-target, runtime, and immutable snapshot semantics. The actual model/agent invocation should be implemented through MAF.

Decision point: whether the public HAgent execution contract should directly expose MAF agent types or remain framework-neutral through an adapter. See `06-decisions-required.md`.

### Middleware and policy pipeline

MAF provides agent, function, and chat-client middleware, runtime context, exception handling, guardrails, result overrides, and instrumentation.

HAgent should remove duplicated generic middleware/pipeline code. Retain HAgent policy evaluation where the policy governs HAgent resources, authorization, runtime lifecycle, budgets, learning promotion, or host authority.

### Tools

MAF provides function tools, agent-as-tool composition, function invocation middleware, and approval-capable tool flows.

HAgent should use MAF for invocation mechanics. Retain HAgent's separation between tool definitions and executable host-owned handlers, plus HAgent authorization/registration semantics when they are stronger or materially different.

### Workflows and orchestration

MAF provides sequential, concurrent, handoff, group-chat, and magentic orchestration. Its workflow model provides graph composition, state, checkpoint/resume, and human-in-the-loop facilities.

HAgent should not maintain a second generic workflow engine. HAgent may add cognitive admission, persistent cognitive state, or host scheduling above MAF workflows where those are distinct requirements.

### Conversations, context, and memory

MAF provides sessions, chat history, context providers, storage, compaction, and context injection.

HAgent should reuse MAF session/context mechanisms where their semantics match. HAgent's broader Memory/Knowledge/Wiki/Skill resource model remains an HAgent-level contract because it includes scope, ownership, provenance, lifecycle, policy, and governed learning promotion.

### Human-in-the-loop

MAF provides approval gates and request-info interactions in agent/workflow scenarios.

HAgent should delegate generic workflow-level HITL to MAF. HAgent keeps cognitive intervention, authorization, and persisted intervention state where these are part of HAgent's required semantics.

### Provider and model access

Microsoft.Extensions.AI provides `IChatClient` and related AI abstractions. MAF can build agents on top of an `IChatClient`.

HAgent should use these as lower-level transport/execution building blocks. HAgent's provider catalogue, logical model identity, execution target, capability evidence, operational state, cost state, quota/admission state, and execution planner remain HAgent-level concepts when not already supplied with equivalent semantics.

### Observability

MAF provides OpenTelemetry-oriented tracing and metrics and exposes pipeline-level instrumentation.

HAgent should consume MAF instrumentation instead of duplicating generic spans and metrics. Add HAgent-specific correlation, identity, runtime lifecycle, cognitive-state, planning, learning, and persistence events where they are outside MAF's domain.

### A2A and MCP

MAF includes interoperability support around A2A and MCP.

HAgent should use MAF integrations instead of implementing duplicate protocol plumbing unless HAgent needs a deliberately different host boundary or lifecycle contract.

### Persistent cognition

MAF provides workflow state, sessions, context storage, and checkpoint/resume building blocks.

HAgent retains the Cognitive Kernel and Cognitive Strategy architecture. Goals, beliefs, intentions, plans, operators, impasses, experience, deterministic decisions, learning, and restart recovery remain HAgent-level concepts.

### Learning governance

MAF provides agent skills and contextual capability mechanisms, but these do not replace HAgent's governed learning-candidate and promotion model.

HAgent retains learning governance and may use MAF skills/context as an implementation target only where semantics are equivalent. Promotion remains policy- and authorization-governed.

### HAgent-owned storage

MAF provides application-controlled persistence facilities for workflow/session-related state.

HAgent retains HAgent-owned storage for HAgent-specific resources and durable cognitive state. MAF persistence can be used internally where its model is authoritative and compatible. The rebuild must not create two stores for the same logical state.

### WinForms integration

MAF does not replace HAgent's UI Context / Control Adapter boundary. This remains HAgent-specific integration and stays in `HAgent.WinForms`.

### HWorld integration

MAF can provide agent and workflow machinery, but HWorld's ownership of world state, simulation time, sensors, action validation, and side effects remains outside both MAF and HAgent. HAgent remains the reusable cognitive subsystem between them.

## Reuse rule

The implementation question is never "can MAF do something similar?" The question is:

> Does MAF provide the same externally required behavior, safety boundaries, lifecycle guarantees, persistence semantics, concurrency semantics, and extension points that HAgent requires?

Only when the answer is yes should the old HAgent implementation be removed outright.

## Source references

- https://learn.microsoft.com/en-us/agent-framework/concepts/agents/
- https://learn.microsoft.com/en-us/agent-framework/agents/agent-pipeline
- https://learn.microsoft.com/en-us/agent-framework/journey/workflows
- https://learn.microsoft.com/en-us/agent-framework/workflows/orchestrations/
- https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
