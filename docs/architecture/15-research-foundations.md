# Research Foundations

## Purpose

HAgent's architecture is informed by several generations of research on cognitive architectures, autonomous agents, planning, memory, continual learning, and language agents.

This document records the research lineage behind HAgent and, more importantly, distinguishes **inspiration** from **adoption**. HAgent is not intended to reproduce a historical cognitive architecture literally. It is a production software runtime that adapts useful architectural mechanisms to provider-neutral, asynchronous, persistent LLM agents operating inside real host applications.

The primary architectural references are:

- BDI agent architectures: explicit beliefs, desires/goals, intentions, plans, commitment and reconsideration.
- SOAR: working memory, operators, production knowledge, decision cycles, impasses/substates, and learning from solved subproblems.
- ACT-R: modular cognition, buffers/working memory, procedural vs declarative knowledge, utility-based selection, and production compilation.
- CoALA: modular memory, structured action spaces, and a generalized decision process for language agents.
- Generative Agents: observation, memory, reflection, retrieval, planning and continuous environment interaction.
- Global Workspace Theory / LIDA: distributed specialized processing, attention/salience, temporary global availability, and continual cognitive cycles.
- ReAct and related language-agent work: tightly interleaved reasoning, action and observation during deliberation.
- Reflexion: learning from feedback through reflective episodic memory without changing model weights.
- MemGPT: hierarchical memory and explicit context management for long-lived agents.
- Voyager: persistent executable skill libraries and experience-driven skill acquisition.
- Current persistent/lifelong-agent research: durable state, modular memory, temporal adaptation, recovery, continual learning, evaluation and governance.

HAgent's current architecture already contains several compatible ideas: explicit runtime-agent identity, immutable execution snapshots, first-class events, scoped memory, knowledge/skills separation, controlled learning candidates, host/environment boundaries, capability-aware execution planning, and an explicitly planned persistent cognitive runtime. See `docs/architecture/10-runtime.md`, `06-events.md`, `07-execution-planning.md`, `20-context.md`, `40-security.md`, `50-workspaces.md`, `70-external-host-integration.md`, and `80-knowledge-memory-learning.md`. fileciteturn4file0L2-L5 fileciteturn11file0L2-L5 fileciteturn14file0L2-L5 fileciteturn12file0L2-L5 fileciteturn26file0L2-L5 fileciteturn13file0L2-L5

## Architectural thesis

HAgent should be treated as a **hybrid production cognitive runtime** rather than as a copy of BDI, SOAR, ACT-R, or an LLM-agent pattern.

The intended synthesis is:

```text
Classical cognitive architecture
    -> explicit state, goals, intentions, plans, operators, memory and control

LLM agent research
    -> flexible language-mediated reasoning, reflection, retrieval and tool use

Production runtime engineering
    -> persistence, concurrency, cancellation, versioning, identity, policy,
       quotas, capability selection, observability, recovery and host integration

HAgent
    -> persistent cognitive runtime above an independent execution engine
```

The most important architectural consequence is that HAgent must separate:

```text
Cognitive question
    What should the agent do?

Execution question
    Where and how should the required inference execute?
```

The first belongs to the cognitive runtime/planner. The second belongs to the capability-aware execution planner. This separation is already explicit in HAgent's roadmap and execution-planning architecture. fileciteturn10file0L2-L5 fileciteturn14file0L2-L5

## Research-to-HAgent mapping

| Research lineage | Key mechanism | HAgent equivalent | Adoption decision |
|---|---|---|---|
| BDI | Beliefs | Host observations + proposed `BeliefState` | **Adapt** |
| BDI | Desires | Goals | **Adopt** |
| BDI | Intentions | Intentions/commitments | **Adopt and strengthen** |
| BDI | Plans | Persistent `Plan` model | **Adopt and strengthen** |
| BDI | Reconsideration | Plan/intent review triggers | **Adopt** |
| SOAR | Working memory | Cognitive working state + attended context | **Adapt** |
| SOAR | Operators | Plan steps/actions with preconditions/effects | **Adopt** |
| SOAR | Decision cycle | Event -> attention -> policy -> plan/action loop | **Adapt** |
| SOAR | Impasse/substate | Explicit bounded deliberation/blocked-progress state | **Strongly adopt** |
| SOAR | Chunking | Skill/policy candidate generation from successful reasoning | **Adapt through governed learning** |
| ACT-R | Buffers/modules | Explicit working-state slots/resource channels | **Adapt** |
| ACT-R | Declarative memory | Semantic/episodic memory | **Already aligned** |
| ACT-R | Procedural memory | Skills/procedural memory | **Already aligned** |
| ACT-R | Utility | Policy/planner/execution utility ranking | **Adopt conceptually** |
| ACT-R | Production compilation | Learned deterministic skill/policy versions | **Adapt** |
| CoALA | Modular memory | HAgent Memory/Knowledge/Skills | **Already aligned; strengthen interfaces** |
| CoALA | Structured action space | Tools + internal cognitive actions | **Adopt internal actions** |
| CoALA | General decision loop | DecisionContext/DecisionPolicy/Planner | **Already planned; refine** |
| Generative Agents | Observation | Events/observations | **Already aligned** |
| Generative Agents | Memory stream | Episodic memory | **Already aligned** |
| Generative Agents | Reflection | Learning/reflection candidates | **Adopt with stronger governance** |
| Generative Agents | Retrieval | Goal/attention-driven retrieval | **Adopt** |
| Generative Agents | Planning | Persistent plans | **Adopt** |
| GWT/LIDA | Distributed processing | Event/handler/policy subsystem | **Adapt** |
| GWT/LIDA | Attention/salience | Attention layer | **Already planned; formalize** |
| GWT/LIDA | Global workspace | Bounded `GlobalWorkspaceFrame`/decision workspace | **Adopt conceptually** |
| GWT/LIDA | Cognitive cycle | Event-driven activation cycle | **Adopt; avoid fixed human timing** |
| ReAct | Reason/action interleaving | Deliberative execution mode | **Use inside deliberation, not as whole architecture** |
| Reflexion | Verbal feedback memory | Reflection/experience records | **Adopt with evaluation/provenance** |
| MemGPT | Hierarchical memory | Working/episodic/semantic/procedural tiers | **Adopt context-management principles** |
| Voyager | Skill library | Versioned Skill Library | **Already aligned; strengthen learning loop** |
| Lifelong learning research | Temporal adaptation | Durable state + memory/learning | **Major adoption area** |
| Persistent-agent research | Restart/recovery | Cognitive persistence + revisions/checkpoints | **Major adoption area** |

## BDI: what HAgent should learn

The BDI literature treats agents as systems whose behavior is organized around beliefs, desires and intentions, with plans and commitment helping connect deliberation to action. A 2020 IJCAI survey reviews the architecture and its trade-offs across implementations.

References:

- Lavindra de Silva, Felipe Meneguzzi, Brian Logan, **“BDI Agent Architectures: A Survey”**, IJCAI 2020. https://doi.org/10.24963/ijcai.2020/684
- Michael Bratman, **“Intention, Plans, and Practical Reason”**, 1987.

HAgent should adopt the semantic distinction rather than reproduce a classic BDI interpreter:

```text
BeliefState
    what the runtime currently takes to be true/relevant,
    with source, time, confidence and validity

Goal
    desired state/outcome

Intention
    commitment to pursue a goal through a selected course of action

Plan
    explicit representation of the selected course of action
```

The current HAgent roadmap already defines Goals, Intentions and Plans, but it does not yet make **belief revision** a first-class contract. That should be added. New observations must be able to invalidate assumptions, mark plan preconditions stale, and trigger reconsideration.

HAgent should also adopt BDI-style **reconsideration** rather than assuming that a plan remains valid until completion. Reconsideration should be policy-driven and triggered by events such as new evidence, plan failure, a higher-priority goal, changed permissions, or resource changes.

## SOAR: the most useful control ideas

SOAR models deliberate goal-directed behavior around the selection and application of operators to a state. It separates working memory from long-term procedural knowledge and uses impasses/substates when the current knowledge is insufficient to make progress. Its chunking mechanism learns new procedural rules from resolved subproblems. The official SOAR documentation makes these mechanisms explicit. https://soar.eecs.umich.edu/soar_manual/02_TheSoarArchitecture/

The key HAgent adaptation should be **impasse-driven deliberation**:

```text
Normal case
    event -> policy -> known action/plan step -> execute

Impasse
    no safe action
    ambiguous alternatives
    missing information
    failed precondition
    blocked plan
        -> bounded deliberation
        -> obtain knowledge / compare alternatives / create plan revision
        -> resume normal execution
```

This matches HAgent's requirement that routine events can be handled without an LLM while novelty, ambiguity or blocked progress can activate deliberation. The current 0.97 roadmap already contains these triggers; the research recommends formalizing them as an explicit **Impasse** concept rather than leaving them as an assortment of trigger conditions. fileciteturn24file0L2-L5

SOAR's chunking also gives HAgent a powerful interpretation of its existing learning model. Instead of learning only text memories, successful deliberation can yield a candidate deterministic procedure or skill that makes future similar cases cheaper and faster. HAgent should use its existing governed `SkillCandidate`/versioning pipeline for this rather than creating a second rule-learning system. fileciteturn13file0L2-L5

Do **not** copy SOAR's production-rule language or require every HAgent application to become a symbolic production system.

## ACT-R: modular working state and utility

ACT-R is a hybrid cognitive architecture implemented as a programmable theory of cognition. Its architecture separates cognitive modules and uses buffers as interfaces to a centralized production system; its procedural system includes production matching, utility-based conflict resolution and production compilation for learning.

References:

- Ritter et al., **“ACT-R: A cognitive architecture for modeling cognition”**, 2018/2019. https://doi.org/10.1002/wcs.1488
- ACT-R 7.30 Reference Manual. https://act-r.psy.cmu.edu/actr7.x/reference-manual.pdf

The important lesson for HAgent is not human cognitive timing. It is **explicit modular state**:

```text
Working State
    current situation
    active focus
    current assumptions
    currently retrieved items
    current plan step

Long-term resources
    episodic memory
    semantic knowledge
    procedural skills
```

HAgent already distinguishes memory families, but persistent cognition needs a runtime-owned working state that is not treated as ordinary durable memory. This is a significant architectural change recommended by the research comparison.

ACT-R also motivates adding utility to HAgent's decision stack. HAgent should use hard constraints first and utility scoring second:

```text
Hard gates
    authorization
    required capabilities
    safety policy
    validity

Then rank surviving alternatives
    expected utility
    goal value
    confidence
    cost
    latency
    risk
    resource usage
```

This should be unified with, but remain distinct from, the provider Execution Planner. The cognitive planner chooses among actions/plans; the execution planner chooses among inference targets.

## CoALA: the best organizing framework for LLM cognition

CoALA was introduced in TMLR in 2024 to organize language agents around three ideas: modular memory, a structured action space interacting with internal memory and external environments, and a generalized decision-making process.

Reference:

- Theodore R. Sumers, Shunyu Yao, Karthik Narasimhan, Thomas L. Griffiths, **“Cognitive Architectures for Language Agents”**, TMLR 2024. https://arxiv.org/abs/2309.02427

CoALA largely confirms the direction HAgent already took. HAgent has separate Memory, Knowledge, Skills, Tools, and a planned `DecisionContext`/`DecisionPolicy`/`Planner` layer. fileciteturn13file0L2-L5 fileciteturn24file0L2-L5

The important addition is **internal cognitive actions**. HAgent's action space should not be limited to external tools. A deliberative policy should be able to request bounded internal operations such as:

```text
RecallMemory
RetrieveKnowledge
SelectSkill
CreateGoal
ReviseGoal
AdoptIntention
CreatePlan
RevisePlan
MarkBeliefStale
RequestDeliberation
Wait
Sleep
Wake
EmitObservation
```

These are still capabilities governed by policy. They are not unrestricted internal authority.

## Generative Agents: memory, reflection and planning

Generative Agents demonstrated an architecture that stores experiences, synthesizes higher-level reflections, retrieves memories dynamically, and uses them to plan behavior in an interactive environment. Their ablation study found observation, planning and reflection each contributed to believable behavior.

Reference:

- Park et al., **“Generative Agents: Interactive Simulacra of Human Behavior”**, UIST 2023. https://doi.org/10.1145/3586183.3606763

The lesson for HAgent is the **experience -> reflection -> retrieval -> future behavior** loop. HAgent should preserve the useful separation between raw episodes and higher-level derived knowledge, which is already reflected in its episodic/semantic/procedural memory model and controlled learning candidates. fileciteturn13file0L2-L5

However, HAgent should improve on the research prototype in production-oriented ways:

- reflection must be budgeted and triggered, not continuous;
- reflections require provenance and evidence;
- contradictions must be detectable;
- durable knowledge must not become authoritative merely because an LLM generated it;
- retrieval must be driven by attention, goals and plan state;
- memory writes should be governed separately from memory reads.

## Global Workspace / LIDA: attention and broadcast

Global Workspace Theory and the LIDA architecture model cognition as many distributed specialized processes competing for attention, with a limited global workspace making selected information broadly available to other processes. LIDA further develops an explicit attention-driven cognitive cycle.

References:

- Baars & Franklin, **“An architectural model of conscious and unconscious brain functions: Global Workspace Theory and IDA”**, 2007. https://doi.org/10.1016/j.neunet.2007.09.013
- Baars & Franklin, **“Consciousness is Computational: The LIDA Model of Global Workspace Theory”**, 2009. https://doi.org/10.1142/S1793843009000050
- Franklin et al., **“Global Workspace Theory, its LIDA model and the underlying neuroscience”**, 2013. https://doi.org/10.1016/j.cortex.2012.12.018

The production interpretation for HAgent is valuable even without making claims about consciousness:

```text
many low-cost signals
        -> salience competition
        -> bounded global workspace / attended set
        -> decision policies
        -> action or deliberation
```

HAgent's current attention concept should therefore become an explicit bounded **Global Workspace Frame** or equivalent decision workspace. It should contain only the currently selected events/observations/resources and the reasons they were selected. It should have an expiry/revision boundary and should never be confused with the full memory store.

Do not implement neural activation-passing or human consciousness claims in `HAgent.Core`. Adapt the software-control abstraction: **distributed candidates, centralized bounded attention, explicit broadcast to decision modules**.

## ReAct: use interleaving inside deliberation

ReAct showed that language agents can perform better by interleaving reasoning and actions so actions gather information and reasoning updates plans based on observations.

Reference:

- Yao et al., **“ReAct: Synergizing Reasoning and Acting in Language Models”**, ICLR 2023. https://mlanthology.org/iclr/2023/yao2023iclr-react/

HAgent should adopt this as a **deliberation execution mode**, not as the entire persistent architecture:

```text
Deliberation
    -> infer candidate step
    -> call authorized tool / gather observation
    -> update DecisionContext
    -> continue or revise plan
```

HAgent should record structured decision/observation/action metadata rather than attempting to persist or expose private chain-of-thought. This preserves traceability without making hidden reasoning a storage contract.

## Reflexion: feedback as experiential learning

Reflexion improves language-agent behavior by converting feedback into linguistic reflections retained in episodic memory rather than updating model weights.

Reference:

- Shinn et al., **“Reflexion: Language Agents with Verbal Reinforcement Learning”**, NeurIPS 2023. https://mlanthology.org/neurips/2023/shinn2023neurips-reflexion/

HAgent should adopt the mechanism as a form of **reflection candidate generation**:

```text
execution outcome
   -> feedback / error / evaluation
   -> reflection candidate
   -> provenance + confidence + evidence
   -> policy/evaluation
   -> episodic/semantic/procedural promotion
```

This fits HAgent's existing learning candidate architecture and avoids conflating learning with model fine-tuning. fileciteturn13file0L2-L5

## MemGPT: memory as a managed hierarchy

MemGPT treats limited model context as a resource and introduces virtual context management inspired by operating-system virtual memory, moving information between tiers and allowing long-lived multi-session behavior.

Reference:

- Packer et al., **“MemGPT: Towards LLMs as Operating Systems”**, 2023. https://arxiv.org/abs/2310.08560

HAgent should adopt the principle, not the specific API:

```text
Fast working state
    -> current global workspace / context

Hot episodic or semantic cache
    -> recently relevant memories

Long-term stores
    -> durable memory / knowledge / skills

Context compiler
    -> admits only the information needed by this decision
```

This strongly reinforces HAgent's existing context-efficiency requirements and argues for making context compilation a first-class runtime service rather than embedding ad hoc memory retrieval into each execution path. fileciteturn24file0L2-L5

## Voyager: skills as persistent procedural knowledge

Voyager demonstrated open-ended embodied learning through an automatically growing skill library, with environment feedback and iterative self-verification.

Reference:

- Wang et al., **“Voyager: An Open-Ended Embodied Agent with Large Language Models”**, TMLR 2024. https://arxiv.org/abs/2305.16291

HAgent already has a versioned Skill Library and a learning pipeline that can generate `SkillCandidate` objects. The research suggests strengthening the skill contract with:

```text
Preconditions
Inputs/outputs
Procedure
Required tools/resources
Expected effects
Verification criteria
Known failure modes
Version
Evidence/provenance
Performance statistics
```

A skill should be executable without re-running a large deliberation whenever its preconditions are satisfied. That is the production equivalent of learned procedural memory.

## Current persistent/lifelong-agent research

Recent surveys emphasize that persistent agents are not simply agents with a larger chat history. Lifelong operation requires evolving memory, temporal adaptation, grounded interaction, evaluation, and mechanisms for learning from experience.

Key references:

- Zheng et al., **“Lifelong Learning of Large Language Model Based Agents: A Roadmap”**, IEEE TPAMI 2026. https://doi.org/10.1109/TPAMI.2025.3650546
- Zhang et al., **“A Survey on the Memory Mechanism of Large Language Model-based Agents”**, ACM TOIS 2025. https://doi.org/10.1145/3748302
- Dorovatas et al., **“Modular Memory is the Key to Continual Learning Agents”**, 2026. https://arxiv.org/abs/2603.01761
- Wang, **“Memento-II: Learning by Stateful Reflective Memory”**, 2025. https://arxiv.org/abs/2512.22716
- **“Agentic AI systems: A systematic survey of multi-agent architectures, cognitive foundations, interaction, explainability, security, and performance evaluation”**, Neurocomputing, 2026. https://doi.org/10.1016/j.neucom.2026.134049

These works strengthen several HAgent requirements already present in the roadmap: persistent state, modular memory, temporal adaptation, event-driven operation, reflection, learning governance, recovery and evaluation. fileciteturn24file0L2-L5

## What HAgent should explicitly add from the research

The research comparison identifies seven architectural additions that should become explicit concepts in HAgent:

### 1. Belief State

Add a provider-neutral, provenance-aware `BeliefState`/belief assertion layer between raw observations and higher-level decisions.

A belief should carry, where available:

```text
belief identity
proposition/state representation
source/provenance
observed-at / valid-from / valid-until
confidence/evidence
status: accepted / uncertain / stale / contradicted
caused-by event(s)
```

Belief state is not authoritative host state. It is the agent's current model of relevant information.

### 2. Impasse

Add a first-class `Impasse` concept for cases where reactive policy or current plan execution cannot safely progress.

Typical reasons:

```text
NoApplicableAction
AmbiguousAlternatives
MissingInformation
FailedPrecondition
PlanBlocked
ConflictingGoals
UncertainBelief
PolicyConflict
```

An impasse creates a bounded deliberation request or a wait/escalation state. It should be observable and budgeted.

### 3. Operators / Actions

Plans should contain executable action specifications with enough structure to support deterministic progression:

```text
Action/Operator
    identity
    preconditions
    parameters
    expected effects
    failure conditions
    idempotency/retry semantics
    required capabilities
    risk/policy classification
```

The host still owns authoritative side effects; HAgent owns only the generic representation of intended actions and their execution boundary.

### 4. Global Workspace Frame

Formalize a bounded attended set that is distinct from memory and conversation context:

```text
GlobalWorkspaceFrame
    attended events
    active beliefs
    active goals
    current intention
    current plan step
    relevant memory/knowledge
    reasons for selection
    expiry/revision
```

Only this bounded frame should be promoted into a deliberative `DecisionContext` unless additional retrieval is explicitly justified.

### 5. Cognitive Revision / Commit Model

The current roadmap already uses revision checks to protect from stale asynchronous executions. Extend this idea so cognitive state transitions have a durable revision/compare-and-apply boundary:

```text
CognitiveRevision N
    -> deliberation A starts
    -> external event changes state
    -> CognitiveRevision N+1
    -> deliberation A returns
    -> conditional apply sees N != N+1
    -> result becomes stale/rejected
```

This is stronger than merely checking execution identity. It protects goals, beliefs, intentions, plans and working state as a coherent transactional cognitive state.

### 6. Memory Consolidation / Maintenance

Add background or event-triggered maintenance operations for:

```text
reflection
consolidation
forgetting/expiration
contradiction detection
skill evaluation
memory compression
retrieval-index maintenance
```

These should not run for every event. They should be scheduled as bounded maintenance work.

### 7. Persistent-Agent Evaluation

Introduce dedicated long-horizon metrics and deterministic tests, including:

```text
Event-to-deliberation ratio
Reactive resolution rate
Plan completion rate
Plan revision rate
Impasse rate
Stale-result rejection count
Recovery correctness
Memory retrieval usefulness
Memory contradiction rate
Learning-promotion precision
Skill reuse rate
Deliberation cost/tokens
Cost per successful goal
Latency to goal completion
Autonomy budget consumption
```

A persistent cognitive runtime should be evaluated as a system over time, not only by single-turn response quality.

## What HAgent should explicitly NOT copy

### Do not implement a literal BDI interpreter

BDI concepts are useful; forcing all behavior through beliefs/desires/intentions with historical interpreter semantics is unnecessary.

### Do not implement SOAR production syntax

SOAR's impasse and chunking concepts are valuable. Its complete symbolic rule language is not required for a general-purpose LLM runtime.

### Do not reproduce ACT-R human timing

ACT-R's modularity and utility ideas are relevant. Human reaction-time equations and psychological model fidelity are not HAgent goals.

### Do not implement consciousness

Global Workspace/LIDA provides useful control metaphors. HAgent should not claim consciousness or implement neuroscience-specific mechanisms.

### Do not make every event an LLM call

This would directly contradict the strongest lessons from efficient autonomous-agent design and HAgent's own event-driven architecture.

### Do not treat generated memory as truth

Reflection and semantic memory are hypotheses/learned resources until validated by policy and evidence.

### Do not make persistent cognition a second execution engine

The cognitive runtime should produce decisions and canonical `AgentExecutionRequest`s and then use the existing execution engine. This invariant is already explicit in HAgent's 0.97 architecture. fileciteturn24file0L2-L5

## Resulting conceptual architecture

The research-backed HAgent architecture should evolve toward:

```text
Host Environment
    |
    +--> Event / Observation
    |
    v
Persistent Cognitive Runtime
    |
    +--> Event Intake / Dedup / Backpressure
    |
    +--> Belief Update / Revision
    |
    +--> Attention / Global Workspace
    |
    +--> Goals
    |
    +--> Intentions / Commitments
    |
    +--> Current Plan / Operators
    |
    +--> Reactive Policy
    |
    +--> Impasse Detection
    |        |
    |        +--> bounded Deliberation
    |
    +--> Decision Policy
    |
    +--> Context Compiler
    |        |
    |        +--> relevant Memory
    |        +--> Knowledge
    |        +--> Skills
    |
    +--> Cognitive Planner
    |        |
    |        +--> Decision / Plan Revision
    |
    +--> canonical AgentExecutionRequest
             |
             v
       Execution Engine
             |
       Execution Planner
             |
       Provider / Model / Target
             |
       Tool / Observation Result
             |
             v
       Cognitive Revision Apply
             |
       Memory / Reflection / Learning
             |
       Next Event / State Revision
```

## Architectural position

The research does not invalidate HAgent's direction. It makes the intended architecture more precise.

HAgent should be described as:

> **A provider-neutral, application-native persistent cognitive runtime that combines explicit cognitive state and control mechanisms from classical cognitive architectures with LLM-based deliberation and a production-grade asynchronous execution infrastructure.**

This is an architectural synthesis, not a claim that HAgent invented BDI, SOAR, ACT-R, Global Workspace, memory-augmented agents, or persistent-agent concepts. Scientific novelty would require a dedicated literature, prior-art and empirical study beyond this architecture document.
