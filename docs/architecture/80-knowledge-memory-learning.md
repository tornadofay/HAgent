# Knowledge, Skills, Memory, and Learning

HAgent treats knowledge, capabilities, memory, and learning as related but distinct subsystems. They share retrieval and policy infrastructure but have different ownership, lifecycle, and promotion rules.

## Core distinction

```text
Skill
    reusable capability/procedure that can be executed

Knowledge
    reusable information that can be retrieved

Wiki
    persistent managed knowledge source within the knowledge system

Memory
    scoped information derived from experience or runtime state

Learning
    process that turns execution experience into typed candidates and,
    when policy permits, promotes candidates into memory, knowledge, or skills
```

A skill is not merely a prompt, Wiki knowledge is not an execution capability, and memory is not automatically authoritative knowledge.

## Knowledge system

The knowledge system is broader than Wiki storage. A Wiki is one managed knowledge source. Future knowledge sources may include documents, imported references, knowledge graphs, host-provided sources, or other provider-neutral implementations.

The Core model therefore uses source/resource contracts rather than making `WikiEntry` the universal knowledge type.

A managed Wiki supports identity, title/content, summary, metadata, tags/categories, provenance, lifecycle/status, versioning, relationships, and retrieval. Knowledge retrieval must remain independent from the physical index implementation. Keyword, semantic, hybrid, relational, or future retrieval implementations are interchangeable behind the knowledge contract.

## Scope and sharing

Knowledge and skills are not intrinsically owned by one runtime agent. They are reusable resources with explicit scope and authorization.

Supported scope concepts are:

```text
Global
Tenant
Domain
User
Agent
Runtime
Execution
```

Not every deployment needs every scope. The effective access set is determined by the host/application policy.

A typical agent may therefore see:

```text
Global knowledge + tenant/domain knowledge + agent-scoped knowledge
Global/domain skills + agent-specific skills
Agent/user/tenant memory according to policy
Execution working memory for the current execution
```

Private runtime memory remains isolated even when two runtime instances are created from the same persistent profile.

## Skill library

Skills are reusable definitions with stable identity and versioning. An agent has a **skill set**, not a private copy of every skill.

```text
Skill Library
    -> skill definitions/versions

Agent Profile
    -> selected/authorized skill references and policy

Runtime Instance
    -> inherited skill availability + optional runtime overrides

Execution
    -> immutable skill snapshot for the execution
```

Executable handlers remain separate from persisted skill definitions and are never serialized into skill storage.

Skill definitions should describe inputs, outputs, preconditions, procedure/steps, required knowledge, required tools, constraints, and version/lifecycle metadata.

## Memory architecture

Memory is explicitly scoped and layered.

```text
Execution / Working Memory
    current execution only

Episodic Memory
    events and experiences

Semantic Memory
    learned/generalized facts derived from experience

Procedural Memory
    learned strategies/procedures derived from experience

Persistent Agent/User/Tenant Memory
    long-lived scoped memories subject to policy
```

Working memory is isolated per execution. Long-term memory can be owned by a runtime instance, logical agent, user, tenant, or another explicit scope. Storage may be shared internally, but logical ownership and authorization must remain distinct.

Memory must remain usable without GPU hardware, a vector database, an embedding model, or a large resident index. Retrieval acceleration is optional infrastructure rather than a Core requirement.

## Learning

Learning is a provider-neutral process around observations and execution outcomes. It is not equivalent to changing model weights.

```text
Execution
    -> observations/events/results
    -> learning analysis
    -> typed learning candidates
    -> validation / policy
    -> promotion
```

Learning candidates are typed so Core code does not have to infer their destination from arbitrary text:

```text
MemoryCandidate
KnowledgeCandidate
SkillCandidate
```

Each candidate preserves provenance, confidence/evidence when available, source execution/runtime identity, and proposed scope. A candidate is not authoritative merely because an LLM proposed it.

Learning may use deterministic code, provider/model reasoning, or both. Code-derived signals such as repetition counts, success rates, explicit host labels, or structured tool outcomes must not require an LLM.

## Knowledge promotion

The default safe path is:

```text
Observation / Memory
    -> KnowledgeCandidate
    -> review/policy/validation
    -> Wiki or another managed knowledge source
```

Agent-generated content must not silently become authoritative Wiki knowledge merely because a model generated it. Provenance and lifecycle state must remain visible after promotion.

## Skill learning and improvement

A successful experience can produce a procedural or skill candidate, but it must not silently mutate a published skill definition.

```text
Execution experience
    -> SkillCandidate
    -> evaluation / validation
    -> new skill version or explicit rejection
```

Existing skill versions remain stable for already-started executions because executions use snapshots.

## Learning modes

HAgent exposes a configurable learning mode:

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

### Disabled

No learning candidates are produced or persisted through the learning subsystem.

### SuggestOnly

Learning produces candidates for human/application review but does not promote them automatically. This is the recommended development/default governance mode.

### AutomaticWithPolicy

Candidates may be promoted when an explicit learning policy approves the candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, and other configured rules.

### FullyAutomatic

The host explicitly permits automatic promotion without an approval step. This is an advanced policy and is never implied by enabling learning alone.

Learning mode is separate from whether memory, knowledge, or skills are enabled for an agent.

## Capability policy and inheritance

Agents require explicit controls for which capability families and resources are available. Profile configuration establishes defaults; runtime instances may override those defaults without mutating the persistent profile.

The effective policy follows an inheritance model:

```text
System/host policy
    -> Agent Profile
        -> Runtime Instance Override
            -> Execution Snapshot
```

For a resource or capability, a runtime override can be:

```text
Inherit
Enabled
Disabled
```

The model is intentionally tri-state so a runtime can selectively override a profile without permanently copying the whole configuration.

The policy must support at least:

```text
Skills
Wiki / knowledge sources
Memory
Individual memory types
Individual skill resources
Individual knowledge resources
```

A disabled capability is enforced by HAgent before retrieval/invocation. Prompt instructions are never used as the enforcement mechanism.

## Future-proof knowledge inventory

The agent configuration UI must not hard-code a fixed list of knowledge tabs as the complete agent knowledge model.

The Core exposes a generic capability/resource inventory describing:

```text
resource ID
resource type ID
name/display metadata
scope
enabled/disabled/effective state
source/provenance metadata
relationships/dependencies where applicable
```

Known types such as Skills, Wiki, Episodic Memory, Semantic Memory, and Procedural Memory can receive specialized views. Unknown or future resource types remain visible through the generic inventory so adding a new knowledge type does not require redesigning the agent overview contract.

## Agent knowledge overview

When an administrator selects an agent profile, the management surface should show its effective knowledge/capability view, including:

```text
Skills
    assigned, inherited, available, disabled, and usage relationships

Knowledge / Wiki
    accessible sources/entries, scope, status, provenance, and relationships

Memory
    enabled memory families, effective scopes, counts/metadata where available

Other knowledge resources
    generic inventory entries for future resource types
```

The overview is a projection of effective configuration and stored relationships; it does not require copying resources into the agent profile.

## Runtime-instance overrides

A live runtime instance inherits the profile's effective capabilities and can apply runtime-only overrides.

Examples:

```text
Runtime A
    Skills: inherit
    Wiki: disabled
    Episodic Memory: enabled

Runtime B
    Skills: disabled
    Wiki: inherit
    Semantic Memory: disabled
```

These overrides are runtime state. They must not mutate the persistent `AiAgent` profile and must be captured in the execution snapshot used by an execution.

## Review and approval

`SuggestOnly` and policy-governed learning require a reviewable suggestion record. The management UI must support:

```text
pending suggestions
candidate type
proposed content/definition
source execution/runtime/agent
confidence/evidence
provenance
proposed target scope
created/updated time
approve
reject
inspect source
```

Approval creates or updates the authoritative target through the appropriate repository contract. Rejection preserves the audit/provenance record according to retention policy and does not modify the target.

## Storage

All persisted Wiki, skill, memory, and learning records remain HAgent-owned data and use the configured HAgent storage backend. Storage implementations remain outside Core.

The storage model must support relationships between agents and reusable skills/knowledge, scoped memory ownership, candidate/review state, provenance, versioning, and bounded retrieval. Schema changes are versioned migrations.

## UI boundary

Knowledge/skill/memory administration belongs in `HAgent.WinForms`. Core exposes provider-neutral repositories, contracts, effective-policy evaluation, and inventory metadata.

The WinForms management surfaces include:

```text
Knowledge / Wiki Manager
    New / Edit / Delete
    search/filter
    relationships / "used by"

Skill Manager
    New / Edit / Delete
    version/status
    relationships / "used by"

Learning Review
    pending candidates
    approve / reject
    provenance and evidence

Agent Configuration > Knowledge
    effective skills
    wiki/knowledge access
    memory families
    generic future resource inventory
    profile-level enable/disable
    instance-level override visibility/editing
```

All WinForms dialogs follow the existing HAgent UI conventions, including `HMessage`, `Header`, and `HButton`.

## Security and authority

Knowledge retrieval, memory recall, skill invocation, learning, and promotion are separate authorization decisions where meaningful. A model request does not grant access.

The effective capability policy is enforced before a resource is exposed to the model or invoked. Host authorization and approval remain authoritative for sensitive operations.

## Architectural rule

Knowledge, Skills, Memory, and Learning are related by lifecycle but are not collapsed into one object model:

```text
Knowledge = reusable information
Skills    = reusable executable capability
Memory    = scoped experience/state
Learning  = controlled transformation of experience into candidates/promotions
```
