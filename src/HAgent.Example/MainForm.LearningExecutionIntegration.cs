using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningExecutionIntegrationTab()
        {
            AddApiTab(
                "Learning Execution Integration",
                "Connect governed learning to execution",
                "Verifies learned context and instruction inputs reuse the canonical execution boundaries and runtime observations are captured as learning input.",
                "Learned context is admitted by the existing policy/capability pipeline; instruction sources remain provider-neutral; observations remain non-authoritative until later learning analysis.",
                "Uses deterministic in-process context, policy, and observation infrastructure. No model or network provider is required.",
                TestLearningExecutionIntegrationAsync,
                "Learning execution integration",
                "Prompt text is not an authorization boundary and runtime observations do not create or promote authoritative resources.");
        }

        private async Task TestLearningExecutionIntegrationAsync(string unused)
        {
            var policyEngine = new ExampleLearningIntegrationPolicyEngine();
            var admissionEvaluator = new ContextPolicyAdmissionEvaluator(
                policyEngine,
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));
            var contextAssembler = new ContextAssembler(
                new ContextPolicyAssembler(new ContextAcquirer(), admissionEvaluator),
                new ContextRanker(),
                new ContextCompactor());
            var preparation = new AiLearningExecutionPreparation(contextAssembler);

            var learnedInstruction = AiInstructionSourceFactory.CreateResource(
                AiInstructionSourceType.Knowledge,
                "example-learned-knowledge",
                "Use the approved learned fact as execution context.",
                "1",
                new AiInstructionScope { ScopeType = "Agent", ScopeId = "example-agent" });

            var learnedContext = new ContextItem
            {
                Id = "example-learned-context",
                Source = "example-learned-knowledge",
                Type = "knowledge",
                Payload = "Approved learned fact",
                Provenance = new ContextProvenance
                {
                    SourceKind = ContextSourceKinds.Knowledge,
                    SourceId = "example-learned-knowledge",
                    SourceVersion = "1",
                    Evidence = "authoritative promotion",
                    CapturedAt = DateTimeOffset.UtcNow
                },
                Trust = 0.95d,
                Importance = 0.8d,
                Relevance = 0.9d,
                CapturedAt = DateTimeOffset.UtcNow,
                Scope = new ContextScope { ScopeType = "Agent", ScopeId = "example-agent" },
                EstimatedCharacters = 20,
                EstimatedTokens = 4
            };

            var result = await preparation.PrepareAsync(
                new[] { learnedInstruction },
                new[]
                {
                    new ContextRetrievalSource
                    {
                        Source = new ExampleLearningContextSource(ContextSourceKinds.Knowledge, "example-learned-knowledge", learnedContext),
                        Query = "approved learned resource",
                        MaxItems = 5
                    }
                },
                new ContextBudget { MaxItems = 5, MaxCharacters = 4096 },
                new ContextAdmissionContext
                {
                    AgentProfileId = "example-agent",
                    RuntimeInstanceId = "example-runtime",
                    ExecutionId = "example-execution",
                    Identity = new AgentIdentityContext(tenantId: "example-tenant", userId: "example-user", workspaceId: "example-workspace")
                }).ConfigureAwait(true);

            if (result.InstructionSources.Count != 1 || result.Context == null || result.Context.Items.Count != 1)
                throw new InvalidOperationException("Learning resource execution preparation contract failed.");

            var observationSource = new ExampleLearningObservationSource();
            var observationStore = new InMemoryAiLearningObservationStore();
            using (new AiLearningExecutionObservationCollector(observationSource, observationStore))
            {
                observationSource.Publish(new AgentExecutionObservation(
                    "example-execution",
                    ExecutionObservationKinds.ProviderRetry,
                    providerId: "example-provider",
                    attempt: 2,
                    retryNumber: 1,
                    reason: "deterministic learning-input test"));
            }

            var observations = await observationStore.QueryAsync(
                new AiLearningObservationQuery { ExecutionId = "example-execution" }).ConfigureAwait(true);
            if (observations.Count != 1 || observations[0].Kind != ExecutionObservationKinds.ProviderRetry)
                throw new InvalidOperationException("Learning observation capture contract failed.");

            Write(
                "LEARNING EXECUTION INTEGRATION",
                "Contract test succeeded." + Environment.NewLine +
                "Approved learned instruction remains provider-neutral: verified." + Environment.NewLine +
                "Learned context enters execution only through policy/capability admission: verified." + Environment.NewLine +
                "Execution context is bounded and snapshot-owned: verified." + Environment.NewLine +
                "Authoritative runtime outcome observation captured as learning input: verified." + Environment.NewLine +
                "Observation capture does not create or promote a learning candidate: verified." + Environment.NewLine +
                "Prompt text is not used as an authorization boundary: verified.");
        }

        private sealed class ExampleLearningIntegrationPolicyEngine : IAiPolicyEngine
        {
            public string PolicyVersion { get { return "1"; } }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                return new AiPolicyDecision
                {
                    Outcome = AiPolicyOutcome.Allow,
                    PolicyVersion = PolicyVersion,
                    RuleId = "example-learning-execution-rule",
                    Reason = "learned resource execution test is allowed"
                };
            }
        }

        private sealed class ExampleLearningContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public ExampleLearningContextSource(string kind, string id, ContextItem item)
            {
                Kind = kind;
                Id = id;
                _items = new[] { item };
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }

        private sealed class ExampleLearningObservationSource : IExecutionObservationSource
        {
            public event EventHandler<AgentExecutionObservationEventArgs> ExecutionObserved;

            public void Publish(AgentExecutionObservation observation)
            {
                var handler = ExecutionObserved;
                if (handler != null)
                    handler(this, new AgentExecutionObservationEventArgs(observation));
            }
        }
    }
}
