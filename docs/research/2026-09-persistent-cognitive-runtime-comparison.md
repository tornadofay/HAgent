# HAgent Persistent Cognitive Runtime — Research Comparison and Recommended Evolution

## Executive conclusion

HAgent's planned Persistent Cognitive Runtime is not a completely new scientific idea. Its core components have deep roots in cognitive architectures and autonomous-agent research: BDI provides beliefs/goals/intentions/plans and commitment; SOAR provides working memory, operators, decision cycles, impasses and learning; ACT-R provides modular cognition, buffers, declarative/procedural separation and utility; Global Workspace/LIDA provides attention and temporary global availability; newer LLM-agent research provides modular memory, structured actions, reflection, retrieval and language-mediated planning.

The important opportunity is architectural synthesis. HAgent can combine these ideas into a **production-oriented persistent cognitive runtime** that is:

- host/application independent;
- event-driven rather than prompt-loop driven;
- persistent across executions and process restarts;
- capable of deterministic/reactive behavior without model calls;
- capable of bounded LLM deliberation when needed;
- explicit about goals, commitments, plans and current beliefs;
- governed by identity, authorization, policy and budgets;
- connected to a capability-aware execution planner that selects the actual inference target;
- able to learn through governed memory/knowledge/skill updates.

The strongest recommendation is therefore **not to replace the current 0.97 direction, but to refine it into a hybrid cognitive architecture with five explicit state/control layers: Belief State, Global Workspace/Attention, Goals + Intentions, Plans + Operators, and Impasse/Deliberation control.** HAgent should also add cognitive revision/commit semantics, memory consolidation/maintenance, and long-horizon evaluation.

This conclusion is consistent with HAgent's current architecture, which already separates runtime identity from execution, treats events as first-class infrastructure, keeps memory/knowledge/skills distinct, separates cognitive planning from execution-target planning, and defines persistent cognition as a layer above the existing execution engine. fileciteturn4file0L2-L5 fileciteturn11file0L2-L5 fileciteturn14file0L2-L5 fileciteturn24file0L2-L5

## 1. What exactly are we comparing?

The current HAgent 0.97 proposal defines a long-lived cognitive runtime above the execution engine. It already includes event intake, attention/salience, working state, goals, intentions, plans, activation/sleep, reactive policies, deliberative execution, persistence/recovery, context efficiency, observability, safety and business/simulation neutrality. It also defines `DecisionContext`, `DecisionPolicy`, and `Planner`. fileciteturn24file0L2-L5

The current HAgent system around that proposal already provides:

```text
Agent Profile
    reusable configuration

Runtime Agent Instance
    live long-lived identity, lifecycle, overrides, memory ownership

Execution
    bounded asynchronous inference with cancellation, timeout,
    snapshots, correlation and stale-result protection

Events
    bounded asynchronous provider-neutral event infrastructure

Memory / Knowledge / Skills
    distinct resource classes with scope and governance

Execution Planner
    capability/cost/health/quota/capacity-aware selection of a concrete target

Host Boundary
    host owns authoritative state, authorization and side effects
```

These existing pieces are important because the research architectures were generally designed as cognitive theories or research systems, not as production software libraries with modern asynchronous execution semantics. HAgent should therefore import their useful *mechanisms*, not their entire original implementation models. fileciteturn4file0L2-L5 fileciteturn14file0L2-L5 fileciteturn16file0L2-L5

## 2. BDI comparison

### Research idea

The BDI model is one of the principal traditions for symbolic agency and agent-oriented software engineering. A 2020 IJCAI survey reviews the major BDI architecture variants and trade-offs. The core distinction is between beliefs about the world, desires/goals, and intentions/commitments to pursue selected objectives through plans.

Reference: de Silva, Meneguzzi, Logan, **BDI Agent Architectures: A Survey**, IJCAI 2020. https://doi.org/10.24963/ijcai.2020/684

### HAgent correspondence

| BDI | HAgent today/planned | Assessment |
|---|---|---|
| Beliefs | Host events/observations + working state | Missing explicit semantic layer |
| Desires | Goals | Strong alignment |
| Intentions | Intentions | Strong alignment, but commitment semantics need strengthening |
| Plans | Plans | Strong alignment |
| Deliberation | Cognitive Planner / DecisionPolicy | Strong alignment |
| Reconsideration | Plan/goal re-deliberation triggers | Planned, needs formal rule |
| Environment | Host context/events | Strong alignment |

### What HAgent should adopt

**1. Make belief state first-class.** HAgent currently has working situation, assumptions and memory, but a cognitive runtime benefits from a clearly defined representation of what the agent currently takes to be true or plausible. A belief is not authoritative application state. It is an agent-side interpretation of evidence.

Recommended model:

```text
Belief
    proposition/state
    source/provenance
    observedAt
    validFrom / validUntil when known
    confidence/evidence
    status: accepted | uncertain | stale | contradicted
    supporting event IDs
```

**2. Formalize intention as commitment.** A goal is desired; an intention is a currently committed course of pursuit. This gives the runtime a place to ask, “Should I continue pursuing this despite a new event?” rather than treating goals as to-do items.

**3. Add explicit reconsideration policy.** Reconsider an intention when a significant event changes a precondition, a plan fails, a higher-priority goal appears, permissions/resources change, or new evidence contradicts the assumptions behind the plan.

### What HAgent should not adopt

Do not implement a literal BDI programming language/interpreter. The useful abstraction is the separation of belief, goal, intention and plan. The runtime can remain LLM/hybrid and provider-neutral.

## 3. SOAR comparison

### Research idea

SOAR models deliberate behavior around the selection and application of operators to a current state. It separates working memory, long-term procedural knowledge and preference/selection mechanisms. A defining feature is the **impasse**: when the available knowledge is insufficient to select or apply an operator, SOAR creates a substate to solve that subproblem. Its chunking mechanism can then learn procedural knowledge from the solved subproblem.

Reference: Laird, Newell, Rosenbloom, **SOAR: An architecture for general intelligence**, Artificial Intelligence 33(1), 1987. https://doi.org/10.1016/0004-3702(87)90050-6

Official architecture manual: https://soar.eecs.umich.edu/soar_manual/02_TheSoarArchitecture/

### HAgent correspondence

| SOAR | HAgent today/planned | Assessment |
|---|---|---|
| Working memory | Cognitive working state | Strong conceptually; make explicit |
| State | DecisionContext/cognitive state | Strong |
| Operator | Plan step/tool/action | Needs explicit generic contract |
| Goal | Goal | Strong |
| Decision cycle | Event -> attention -> policy -> plan/action | Strong planned alignment |
| Impasse | Blocked/ambiguous/missing-information triggers | Present implicitly; should become first-class |
| Substate | Bounded deliberation context | Strong candidate |
| Chunking | Skill/policy learning from solved cases | Existing learning path can absorb it |

### The biggest lesson: impasse-driven deliberation

HAgent should explicitly model:

```text
Normal state
    -> known reactive policy or current plan step
    -> execute

Impasse
    -> no applicable action
    -> ambiguous alternatives
    -> missing information
    -> failed precondition
    -> blocked plan
    -> conflicting goals
    -> uncertain belief
        |
        v
    Bounded deliberation
        |
        +-> retrieve information
        +-> compare alternatives
        +-> revise belief
        +-> revise goal/intent
        +-> create/revise plan
        |
        v
    resume / wait / escalate
```

HAgent's existing 0.97 roadmap already names novelty, ambiguity, blocked progress, goal failure and high-priority events as deliberation triggers. The recommendation is to unify these under an explicit `Impasse` contract with typed reasons, lifecycle and budget. fileciteturn24file0L2-L5

### Chunking and HAgent learning

SOAR's chunking is especially useful because it demonstrates a way to turn expensive reasoning into cheaper future procedural behavior. HAgent already plans controlled `SkillCandidate`/`KnowledgeCandidate`/`MemoryCandidate` promotion with provenance and policy. That should become the place where repeated successful deliberation can create a deterministic skill or policy version rather than forcing another expensive LLM deliberation forever. fileciteturn13file0L2-L5

Do not build a second symbolic rule-learning subsystem unless empirical work later shows that HAgent needs it. Use the existing skill-learning pipeline as the production analogue of chunking.

## 4. ACT-R comparison

### Research idea

ACT-R is a hybrid cognitive architecture expressed as a programmable computer theory of cognition. It uses specialized modules coordinated through a production system, with buffers mediating module state. The procedural system includes production matching, utility-based conflict resolution and production compilation.

References:

- Ritter et al., **ACT-R: A cognitive architecture for modeling cognition**, 2018/2019. https://doi.org/10.1002/wcs.1488
- ACT-R 7.30 Reference Manual: https://act-r.psy.cmu.edu/actr7.x/reference-manual.pdf

### HAgent lessons

**Modularity:** HAgent should make cognitive state modular rather than storing one giant “agent memory.” The current Memory/Knowledge/Skills distinctions are already moving in this direction. fileciteturn13file0L2-L5

**Working memory:** persistent cognition needs a runtime-owned working-state layer distinct from durable memory. That working state should hold the current situation, attended items, temporary assumptions, current plan step and recent observations.

**Utility:** the cognitive planner should be able to rank candidate actions/plans according to expected value, confidence, risk, latency, cost and other application-independent preferences. Hard safety/authorization/capability constraints must be evaluated before utility ranking.

Recommended decision ordering:

```text
1. Reject impossible/unauthorized/unsafe choices
2. Verify required capabilities/preconditions
3. Rank surviving choices by utility/preferences
4. Select or request deliberation
```

This mirrors the HAgent philosophy that execution target selection is constraint-heavy before ranking. The same principle should apply to cognitive choices. fileciteturn14file0L2-L5

**Production compilation:** when a recurring reasoning pattern becomes well understood, HAgent can generate a new Skill/Policy version through its governed learning pipeline rather than hard-coding a human-written rule language.

### What not to copy

Do not import ACT-R's human cognition timing models into HAgent. HAgent is not a human-behavior simulator. The useful contribution is modular state and utility-driven selection, not psychometric fidelity.

## 5. CoALA comparison

### Research idea

CoALA was created specifically to organize LLM language agents. It describes modular memory, a structured action space that can interact with internal memory and external environments, and a generalized decision-making process.

Reference: Sumers, Yao, Narasimhan, Griffiths, **Cognitive Architectures for Language Agents**, TMLR 2024. https://arxiv.org/abs/2309.02427

### HAgent relationship

CoALA is the closest conceptual bridge between classical cognitive architectures and modern LLM agents. HAgent already resembles CoALA strongly:

```text
CoALA
    modular memory
    structured actions
    decision loop

HAgent
    Memory / Knowledge / Skills
    Tools + planned cognitive actions
    DecisionContext / DecisionPolicy / Planner
```

HAgent's 0.97 proposal already establishes these three concepts explicitly. fileciteturn24file0L2-L5

### Important change recommended

Expand the action space to include **internal cognitive actions**, not only external tools.

Examples:

```text
RecallMemory
RetrieveKnowledge
SelectSkill
CreateGoal
ReviseGoal
AdoptIntention
SuspendIntention
CreatePlan
RevisePlan
MarkBeliefStale
RequestDeliberation
Wait
Sleep
Wake
EmitObservation
```

These should be represented as provider-neutral control operations and governed exactly like other capabilities. A model requesting `RecallMemory` does not automatically receive access to all memory; the policy determines which resources are admissible.

This makes the cognitive runtime closer to a real operating architecture rather than a loop that only knows how to call external tools.

## 6. Generative Agents comparison

### Research idea

Generative Agents combine an experience memory stream, reflection and retrieval to create longer-lived behavior. Experiences are stored, higher-level reflections are synthesized over time, relevant memories are retrieved, and the result informs planning.

Reference: Park et al., **Generative Agents: Interactive Simulacra of Human Behavior**, UIST 2023. https://doi.org/10.1145/3586183.3606763

### HAgent lessons

The important loop is:

```text
observe
   -> record experience
   -> retrieve relevant experience
   -> reflect when justified
   -> plan
   -> act
   -> observe again
```

HAgent should adopt this loop but make it much more explicit and governable.

**Reflection should be an operation, not an automatic side effect of every execution.**

Reflection triggers can include:

```text
repeated failure
major goal completion
unexpected outcome
new evidence
high-value experience
contradiction
explicit maintenance window
```

**Reflection output must have provenance.** A reflection is a derived interpretation, not an authoritative fact.

**Retrieval must be context/attention/goal driven.** HAgent should not dump the complete memory stream into every LLM call. Its existing context-budget work already points in this direction. fileciteturn24file0L2-L5

## 7. Global Workspace / LIDA comparison

### Research idea

Global Workspace Theory describes cognition as a system with many specialized processes and a limited workspace where selected information becomes globally available. LIDA develops this into a computational cognitive architecture with explicit attention and recurrent cognitive cycles.

References:

- Baars & Franklin, **An architectural model of conscious and unconscious brain functions: Global Workspace Theory and IDA**, 2007. https://doi.org/10.1016/j.neunet.2007.09.013
- Baars & Franklin, **Consciousness is Computational: The LIDA Model of Global Workspace Theory**, 2009. https://doi.org/10.1142/S1793843009000050
- Franklin et al., **Global Workspace Theory, its LIDA model and the underlying neuroscience**, 2013. https://doi.org/10.1016/j.cortex.2012.12.018

### HAgent lessons

HAgent should formalize the idea of a small, bounded set of currently important information:

```text
Event stream
    ↓
salience / attention
    ↓
Global Workspace Frame
    ↓
Decision policies + planner + context compiler
```

The `GlobalWorkspaceFrame` should contain something like:

```text
attended events
active beliefs
active goals
current intention
current plan step
relevant resources
selection reasons
expiry/revision metadata
```

The frame should be intentionally small and replaceable. It is not long-term memory, not the entire host state, and not the conversation transcript.

This can dramatically improve efficiency because only a bounded subset of the environment has to become relevant to deliberation.

### What not to copy

Do not implement biological neural activation, consciousness claims or neuroscience-specific machinery. HAgent should take the control abstraction—distributed candidate processing plus selective global availability—not the biological theory itself.

## 8. ReAct comparison

ReAct demonstrated that language agents benefit when reasoning and action are interleaved: the agent reasons, acts to gather information, receives observations, and updates its plan.

Reference: Yao et al., **ReAct: Synergizing Reasoning and Acting in Language Models**, ICLR 2023. https://mlanthology.org/iclr/2023/yao2023iclr-react/

### HAgent adaptation

Use ReAct-like interleaving **inside a deliberation episode**:

```text
Deliberation
   -> candidate step
   -> authorized action/observation
   -> new evidence
   -> update belief/context
   -> continue/replan
```

Do not make the whole Persistent Cognitive Runtime into an endless “thought/action/tool” loop. The persistent runtime should manage state and activation; an individual deliberation can use an interleaved reasoning/action policy.

Operational traces should record structured decisions, action requests, observations and outcomes. HAgent should not make raw private chain-of-thought a persistence or observability requirement.

## 9. Reflexion comparison

Reflexion shows that useful adaptation can occur by recording verbal feedback/reflections in episodic memory without updating model weights.

Reference: Shinn et al., **Reflexion: Language Agents with Verbal Reinforcement Learning**, NeurIPS 2023. https://mlanthology.org/neurips/2023/shinn2023neurips-reflexion/

### HAgent adaptation

Add a typed `ReflectionCandidate`/experience-analysis concept or extend the learning-candidate model so the system can distinguish:

```text
raw experience
feedback
reflection
generalized knowledge
procedural improvement
```

Do not collapse all of these into one Memory record. The current HAgent learning architecture already distinguishes memory, knowledge and skill promotion, which is the right direction. fileciteturn13file0L2-L5

## 10. MemGPT comparison

MemGPT treats model context as a managed resource, using multiple memory tiers and explicit movement of information between them.

Reference: Packer et al., **MemGPT: Towards LLMs as Operating Systems**, 2023. https://arxiv.org/abs/2310.08560

### HAgent adaptation

The core lesson is that persistent cognition needs a **context compiler**, not just a memory database.

```text
Durable memory / knowledge / skills
        ↓
retrieval + relevance
        ↓
hot working cache
        ↓
Global Workspace
        ↓
DecisionContext
        ↓
LLM execution only when needed
```

The compiler should make context budget, retrieval cost, freshness and source provenance explicit. HAgent's current 0.97 roadmap already states that persistent working context should prefer state deltas, event summaries, plan changes and relevant retrieval over repeated full-state prompting. fileciteturn24file0L2-L5

## 11. Voyager comparison

Voyager demonstrated an agent that accumulates an executable skill library while interacting with its environment. New skills are generated, verified and reused.

Reference: Wang et al., **Voyager: An Open-Ended Embodied Agent with Large Language Models**, TMLR 2024. https://arxiv.org/abs/2305.16291

### HAgent adaptation

HAgent's versioned Skill Library is a strong match. The skill system should gain richer procedural metadata:

```text
skill identity/version
preconditions
inputs/outputs
required tools/resources
procedure
expected effects
verification criteria
known failure modes
source experience
confidence/evidence
performance statistics
```

A skill should be usable without requiring a full fresh deliberation when its preconditions are satisfied. This is the production equivalent of moving repeated reasoning into procedural memory.

HAgent's existing separation between persisted Skill definitions and runtime handlers is an important safety boundary and should be retained. fileciteturn13file0L2-L5

## 12. Current lifelong/persistent-agent research

The recent literature is converging on several requirements that matter directly to HAgent.

### Lifelong adaptation

Zheng et al.'s 2026 TPAMI review identifies perception, memory and action as core modules for lifelong LLM-agent adaptation and notes that many current agents remain largely static. https://doi.org/10.1109/TPAMI.2025.3650546

**HAgent implication:** persistence is not just storing conversation history. It needs a controlled process for experience accumulation, memory evolution, skill improvement and temporal adaptation.

### Memory mechanisms

A 2025 ACM TOIS survey identifies memory as a central mechanism supporting long-term agent-environment interaction and reviews emerging memory designs. https://doi.org/10.1145/3748302

**HAgent implication:** its existing layered memory model is directionally correct, but retrieval, consolidation, forgetting, conflict handling and provenance need to be explicit runtime services rather than incidental storage behavior. fileciteturn13file0L2-L5

### Modular memory and continual learning

A 2026 position paper argues that modular memory can combine rapid in-context adaptation with more stable longer-term capability updates and frames continuous operation, experience accumulation and personalization as key limitations of current foundation-model systems. https://arxiv.org/abs/2603.01761

**HAgent implication:** keep memory modular and separate from model weights. The current Skill/Knowledge/Memory/Learning separation is a useful foundation.

### Stateful reflective memory

Memento-II proposes a stateful reflective decision process in which episodic memory is read and written as part of continual adaptation and gives a formal interpretation of memory-augmented policy improvement. https://arxiv.org/abs/2512.22716

**HAgent implication:** memory writes should be connected to outcomes/feedback and should influence later decision state; memory cannot be treated only as an archive.

### System-level agent research

A 2026 systematic survey of agentic AI organizes current systems across architecture, cognitive foundations, interaction/adaptation, explainability, security/safety and evaluation, highlighting modularity, interoperability and system-level evaluation as important directions. https://doi.org/10.1016/j.neucom.2026.134049

**HAgent implication:** evaluation, security and explainability should be designed as first-class runtime capabilities, not after-the-fact logging.

## 13. Consolidated comparison

| Dimension | BDI | SOAR | ACT-R | CoALA | Generative Agents | GWT/LIDA | HAgent target |
|---|---|---|---|---|---|---|---|
| Persistent identity | Strong | Strong | Model-specific | Varies | Strong in simulation | Strong | **Strong** |
| Belief/state model | Strong | Strong working state | Module/buffer state | Memory/context | Experience/memory | Workspace/state | **Needs explicit BeliefState** |
| Goals | Strong | Strong | Goal/task models | Decision goals | Implicit/explicit goals | Motivation/attention | **Strong** |
| Intentions/commitment | Strong | Operator commitment | Less central | Less central | Less explicit | Less explicit | **Needs strengthening** |
| Explicit plans | Strong | Operators/substates | Productions | Action/decision loops | Yes | Action selection | **Strong, needs operators** |
| Impasses | Limited | **Core concept** | Production conflict | General decision loop | Not explicit | Attention competition | **Needs first-class Impasse** |
| Working memory | Yes | **Core** | **Core** | Modular memory | Memory stream | Workspace | **Needs runtime-owned layer** |
| Attention/salience | Varies | Selection/preferences | Activation/module constraints | Decision | Retrieval scoring | **Core** | **Strong, formalize workspace frame** |
| Internal actions | Plans/intents | Operators | Module requests | **Core concept** | Planning operations | Global processing | **Needs explicit cognitive actions** |
| Reflection | Varies | Learning/chunking | Learning mechanisms | General framework | **Core** | Learning cycles | **Should be governed** |
| Procedural learning | Yes | **Chunking** | **Production compilation** | Possible | Limited | Learning | **Skills + candidates** |
| Memory hierarchy | Varies | Strong separation | Declarative/procedural | **Core** | Episodic/reflection | Working/long-term | **Strong, needs context compiler** |
| Environment grounding | Yes | Yes | Yes | Yes | **Yes** | Yes | **Host context/events** |
| LLM-native | No | No | No | **Yes** | Yes | No | **Yes** |
| Reactive path | Yes | Yes | Yes | General | Yes | Yes | **Yes** |
| Bounded deliberation | Yes | **Substates** | Yes | General | Yes | Yes | **Should be explicit** |
| Recovery/restart | Usually external | Research system | Model dependent | Varies | Research prototype | Architecture dependent | **Major HAgent concern** |
| Provider routing | No | No | No | No | No | No | **HAgent differentiator** |
| Cost/quota/capacity planning | No | No | No | No | No | No | **HAgent differentiator** |
| Host application boundary | No | Environment interface | Environment interface | External environment | Simulation | Environment | **HAgent differentiator** |

## 14. What HAgent should change

### Priority 1 — Add an explicit Belief State

This is the largest conceptual gap in the current 0.97 plan.

Current state is described as working situation and temporary assumptions. That is not enough to support rigorous belief revision.

Add:

```text
IBeliefStore / BeliefState
BeliefAssertion
BeliefStatus
BeliefEvidence
BeliefRevision
```

At minimum support accepted, uncertain, stale and contradicted status. Tie every important belief to evidence/events and valid-time metadata where practical.

### Priority 2 — Make Impasse first-class

Instead of:

```text
"many things can trigger deliberation"
```

define:

```text
Impasse
   kind
   reason
   state/revision
   affected goal/intent/plan
   candidates considered
   required missing information
   budget
   resolution
```

This gives HAgent a clean control point connecting SOAR-style bounded problem solving to LLM deliberation.

### Priority 3 — Introduce generic Operators/Actions

A Plan should not just be a list of strings or generic steps. It should contain structured operators/actions with:

```text
preconditions
parameters
expected effects
failure conditions
required capabilities
risk classification
idempotency/retry behavior
```

This is necessary to execute a plan across time without asking an LLM to reinterpret every step.

### Priority 4 — Add Global Workspace Frame

Attention is already planned. Make its output an explicit immutable bounded object that is the immediate cognitive workspace.

This becomes the bridge:

```text
Event stream
  -> attention
  -> GlobalWorkspaceFrame
  -> DecisionContext
```

### Priority 5 — Add cognitive compare-and-apply / transactional revision

HAgent already protects execution state with IDs/revisions and prevents stale provider results from overwriting terminal executions. The same idea must protect the entire cognitive state. fileciteturn4file0L2-L5

Recommended pattern:

```text
Cognitive Revision 41
       |
       +--> Deliberation A starts
       |
       +--> New event arrives
       |
       +--> Cognitive Revision 42
       |
       +--> Deliberation A completes
               |
               +--> base revision 41 != current 42
                       |
                       +--> result cannot blindly mutate state
```

This should be a general `TryApply(expectedRevision, transition)` or equivalent transaction boundary.

### Priority 6 — Add memory consolidation and maintenance

The current architecture defines memory and learning well, but persistent cognition needs maintenance loops:

```text
Consolidate
Reflect
Detect contradictions
Expire/forget
Compress
Evaluate skills
Refresh retrieval indexes
```

These operations should be event-triggered or scheduled and budgeted, not executed after every event.

### Priority 7 — Add long-horizon evaluation

The current architecture needs metrics specifically for persistent cognition. Add a dedicated evaluation model with at least:

```text
reactive resolution rate
LLM deliberation rate
deliberation cost per successful goal
impasse frequency
plan completion rate
plan revision rate
belief contradiction rate
memory retrieval usefulness
memory write precision
skill reuse rate
skill promotion precision
recovery correctness
stale-result rejection correctness
mean time to goal completion
budget violations
human intervention frequency
```

The most important metric may be:

> **How much useful autonomous progress does the agent achieve per unit of compute, latency, cost and risk?**

That directly tests the idea that selective persistent cognition is better than repeatedly prompting an LLM from scratch.

## 15. Architecture that should result

I recommend changing the conceptual 0.97 architecture to:

```text
                         HOST ENVIRONMENT
                                |
                         Event / Observation
                                |
                                v
                  +----------------------------+
                  | Persistent Cognitive       |
                  | Runtime                    |
                  +----------------------------+
                     |        |         |
                     |        |         +--> Activation / Sleep
                     |        |
                     |        +------------> Belief Revision
                     |
                     +------------------------> Attention
                                                 |
                                                 v
                                      Global Workspace Frame
                                                 |
                    +----------------------------+-------------------+
                    |                            |                   |
                    v                            v                   v
                 Goals                      Intentions          Current Plan
                                                                  |
                                                                  v
                                                              Operators
                                                                  |
                                                +-----------------+----------------+
                                                |                                  |
                                                v                                  v
                                         Reactive Policy                       Progress
                                                |                                  |
                                                |                         precondition/failure
                                                |                                  |
                                                +----------------+-----------------+
                                                                 |
                                                               Impasse
                                                                 |
                                                                 v
                                                           Deliberation
                                                                 |
                                               +-----------------+----------------+
                                               |                                  |
                                               v                                  v
                                          Context Compiler                    Planner
                                               |                                  |
                                               +----------------+-----------------+
                                                                |
                                                                v
                                                     AgentExecutionRequest
                                                                |
                                                                v
                                                        Execution Engine
                                                                |
                                                        Execution Planner
                                                                |
                                             Provider / Model / Deployment
                                                                |
                                                                v
                                                     Tool / Observation result
                                                                |
                                                                v
                                                  Cognitive revision apply
                                                                |
                                             +------------------+----------------+
                                             |                                   |
                                             v                                   v
                                      Memory/Learning                       Goal/Plan update
```

## 16. What should remain unchanged

Several parts of the existing architecture should be preserved as design invariants.

### Request-oriented execution remains first-class

Persistent cognition must be optional. Ordinary applications should still be able to call `ExecuteAsync` without enabling a cognitive runtime. This is already explicit in the 0.97 design. fileciteturn24file0L2-L5

### Host remains authoritative

HAgent should not become the owner of application/business/simulation truth or side effects. The host provides context, authorization and action validation. This is already explicit in HAgent's external-host and HWorld boundaries. fileciteturn16file0L2-L5 fileciteturn18file0L2-L5

### Provider-neutral execution planning remains separate

The cognitive planner decides **what to do**. The execution planner decides **where/how to perform required inference**. Provider-specific quotas, capabilities and transport details must remain behind the execution layer. fileciteturn14file0L2-L5

### Memory/knowledge/skill separation remains

Do not collapse reusable knowledge, executable skills, experiential memory and learning into one universal “memory” object. The existing separation is one of the strongest parts of HAgent's architecture. fileciteturn13file0L2-L5

### Identity, policy and authorization remain separate

Research architectures often assume a controlled environment. Production HAgent cannot. Its existing model of identity context, policy and host authorization must remain authoritative and must not be replaced by model-generated intent. fileciteturn20file0L2-L5 fileciteturn26file0L2-L5

## 17. Proposed roadmap changes

The current roadmap already puts identity, events, policy, context, observability, evaluation, lifecycle/intervention, recovery, provider lifecycle, 0.96 capability-aware execution and then 0.97 persistent cognition in a sensible order. fileciteturn10file0L2-L5

The research suggests these refinements:

### Before or during 0.97

Add explicit architectural foundations for:

1. **Belief State + Belief Revision**
2. **Global Workspace Frame**
3. **Impasse Model**
4. **Cognitive Operators/Actions**
5. **Cognitive Revision Compare-and-Apply**
6. **Utility/Decision scoring for cognitive alternatives**
7. **Memory Consolidation/Maintenance**
8. **Long-Horizon Cognitive Evaluation**

These should be contracts first, with small deterministic implementations initially.

### During the first 0.97 implementation slice

Do not start with a highly autonomous LLM planner. Start with deterministic infrastructure:

```text
Events
 -> Attention
 -> Belief update
 -> Global Workspace
 -> Goal/Intention selection
 -> Known plan/operator progression
 -> Impasse detection
 -> bounded deliberation request
 -> existing Execution Engine
 -> compare-and-apply
```

This gives a testable skeleton before adding sophisticated model-driven behavior.

### Later in 0.97 / post-0.97

Add:

```text
LLM planner
LLM attention policy
Reflection
Memory consolidation
Skill learning
Utility estimation
Advanced plan search
Multi-agent cognitive coordination
```

Use the same provider-neutral contracts so advanced strategies are replaceable.

## 18. What would make HAgent scientifically interesting?

The strongest potential research contribution is not “HAgent has agents, memory and goals.” Those ideas are established.

The most promising measurable thesis is:

> **Selective persistent cognition can outperform request-per-task agent loops by combining event-driven attention, deterministic/reactive execution, impasse-triggered deliberation, durable structured state, governed memory, and resource-aware inference selection.**

This can be evaluated experimentally.

Compare at least four systems on the same long-running tasks:

```text
A. Stateless LLM calls
B. Memory-enabled reactive agent
C. Persistent LLM agent with always-on deliberation
D. HAgent selective cognitive runtime
```

Measure:

```text
success rate
compute/tokens
latency
provider cost
number of LLM calls
number of tool calls
plan revisions
recovery after restart
performance after repeated experience
memory errors/contradictions
human interventions
```

A convincing result would demonstrate not merely that HAgent “looks like” a cognitive architecture, but that the architecture produces measurable engineering benefits.

## 19. Scientific positioning

The appropriate description is:

> **HAgent is a production-oriented hybrid cognitive architecture/runtime for LLM agents.**

It draws architectural mechanisms from classical cognitive architectures and modern language-agent research, but its design target is a reusable software runtime rather than a model of human cognition.

A defensible novelty statement should be conservative:

> HAgent is not presented as the inventor of persistent cognition, BDI, cognitive architectures, memory-augmented agents, or continual learning. Its potential contribution lies in a specific synthesis: persistent cognitive control + application-host boundaries + asynchronous execution semantics + capability-aware inference planning + resource/cost governance + durable, versioned cognitive state.

A stronger claim would require a much broader novelty/prior-art review and empirical publication.

## 20. Recommended implementation philosophy

The research suggests six rules for implementation:

1. **State over prompts.** Persistent cognitive state should be explicit objects with lifecycle, revisions and provenance, not hidden in giant system prompts.
2. **Selective cognition.** Most routine events should not require an LLM.
3. **Deliberation only on demand.** Use impasses, uncertainty, novelty, goal conflicts and other bounded triggers.
4. **Learning through controlled promotion.** Experience can improve memory/knowledge/skills, but no generated artifact becomes authoritative without policy/evidence.
5. **Separate “what” from “where/how.”** Cognitive planning and execution-target planning remain distinct.
6. **Measure the architecture.** The value proposition must be demonstrated with long-horizon efficiency, quality, recovery and adaptation metrics.

## 21. References / reading order

### Start here

1. **CoALA** — best bridge from cognitive architectures to modern LLM agents.
   https://arxiv.org/abs/2309.02427
2. **BDI Agent Architectures: A Survey** — best overview of beliefs/goals/intentions/plans.
   https://doi.org/10.24963/ijcai.2020/684
3. **SOAR** — best source for impasses, working state, operators and procedural learning.
   https://doi.org/10.1016/0004-3702(87)90050-6
4. **ACT-R** — best source for modular cognitive state and utility/production learning.
   https://doi.org/10.1002/wcs.1488
5. **Generative Agents** — best source for observation, reflection, retrieval and long-lived behavior with LLMs.
   https://doi.org/10.1145/3586183.3606763

### Then study

6. **Global Workspace / LIDA**
   https://doi.org/10.1016/j.neunet.2007.09.013
   https://doi.org/10.1142/S1793843009000050
7. **ReAct**
   https://mlanthology.org/iclr/2023/yao2023iclr-react/
8. **Reflexion**
   https://mlanthology.org/neurips/2023/shinn2023neurips-reflexion/
9. **MemGPT**
   https://arxiv.org/abs/2310.08560
10. **Voyager**
    https://arxiv.org/abs/2305.16291
11. **Lifelong Learning of Large Language Model Based Agents: A Roadmap**
    https://doi.org/10.1109/TPAMI.2025.3650546
12. **Memory Mechanism Survey for LLM Agents**
    https://doi.org/10.1145/3748302
13. **Memento-II**
    https://arxiv.org/abs/2512.22716
14. **Modular Memory is the Key to Continual Learning Agents**
    https://arxiv.org/abs/2603.01761
15. **2026 Systematic Survey of Agentic AI Systems**
    https://doi.org/10.1016/j.neucom.2026.134049

## Final recommendation

Do not redesign HAgent around one historical architecture. **Synthesize them deliberately.**

The recommended HAgent cognitive core is:

```text
BDI
    Beliefs + Goals + Intentions + Reconsideration

SOAR
    Operators + Impasses + bounded subproblem solving + learned shortcuts

ACT-R
    Explicit working state + modular resources + utility

CoALA
    Modular memory + internal/external action space + generalized decision loop

Generative Agents
    Experience + reflection + retrieval + planning

GWT/LIDA
    Attention + bounded global workspace + cyclic activation

ReAct
    Reason/action interleaving inside deliberation

Reflexion
    Feedback-driven reflective experience

MemGPT
    Hierarchical memory/context management

Voyager
    Persistent reusable executable skills

Current persistent-agent research
    Durable state + continual adaptation + governance + recovery + evaluation

HAgent
    All of the above, plus production runtime semantics:
    asynchronous execution, identity, policy, authorization,
    capability-aware execution planning, quotas/cost/capacity,
    host boundary, persistent state, versioning and recovery.
```

That is the strongest version of the architecture currently supported by the literature and by HAgent's existing design. It turns the 0.97 concept from “an LLM that stays alive” into a disciplined cognitive runtime with explicit state, attention, intention, planning, deliberation, memory and recovery.
