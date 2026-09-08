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
        private void AddContextPolicyAssemblyTab()
        {
            AddApiTab(
                "Context Policy Assembly",
                "Run policy-aware context test",
                "Exercises the canonical policy and effective resource capability boundaries before context ranking and budget assembly.",
                "Denied or disabled sources must not be retrieved; denied candidates must not consume the global context budget; allowed items retain their provenance.",
                "No AI request is sent. Policy and capability decisions are evaluated locally using provider-neutral contracts.",
                RunContextPolicyAssemblyTestAsync,
                "Policy-aware context admission",
                "This slice composes the existing unified policy engine with effective resource capabilities. It does not create a second authorization model or put policy into prompt text.");
        }

        private Task RunContextPolicyAssemblyTestAsync(string unused)
        {
            var memorySource = new RecordingContextSource(
                "memory",
                "memory-example",
                CreatePolicyExampleItem("blocked", "memory", 80, 8),
                CreatePolicyExampleItem("allowed", "memory", 20, 2));
            var disabledSource = new RecordingContextSource(
                "knowledge",
                "knowledge-disabled",
                CreatePolicyExampleItem("disabled-item", "knowledge", 10, 1));

            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-example-blocked",
                Name = "Deny blocked context",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.include" },
                ResourceTypes = { "memory" },
                ResourceIds = { "blocked" },
                Reason = "Deterministic example candidate exclusion."
            });
            policy.Validate();

            var capabilityPolicy = new AiResourceCapabilityPolicy();
            capabilityPolicy.Set("knowledge", "knowledge-disabled", AiResourceCapabilityState.Disabled);
            var engine = new DefaultAiPolicyEngine(policy);
            var capabilities = AiResourceCapabilitySnapshot.Resolve(capabilityPolicy);
            var assembler = new ContextPolicyAssembler(
                new ContextAcquirer(),
                new ContextPolicyAdmissionEvaluator(engine, capabilities));

            var result = assembler.AcquireAsync(
                new[]
                {
                    new ContextRetrievalSource
                    {
                        Source = memorySource,
                        Query = "customer",
                        MaxItems = 2
                    },
                    new ContextRetrievalSource
                    {
                        Source = disabledSource,
                        Query = "knowledge",
                        MaxItems = 1
                    }
                },
                new ContextBudget
                {
                    MaxItems = 1,
                    MaxCharacters = 30,
                    MaxEstimatedTokens = 3
                },
                new ContextAdmissionContext
                {
                    AgentProfileId = "assistant"
                },
                CancellationToken.None).GetAwaiter().GetResult();

            if (result.Snapshot.Items.Count != 1 || result.Snapshot.Items[0].Id != "allowed")
                throw new InvalidOperationException("Policy-aware context assembly did not retain the allowed candidate within the global budget.");
            if (result.Snapshot.Items[0].Provenance.SourceKind != "memory" ||
                result.Snapshot.Items[0].Provenance.SourceId != "source-1")
                throw new InvalidOperationException("Allowed context provenance was not preserved.");
            if (memorySource.CallCount != 1 || disabledSource.CallCount != 0)
                throw new InvalidOperationException("Disabled context source was retrieved unexpectedly.");

            var blocked = FindDecision(result.Decisions, "blocked");
            if (blocked == null || blocked.Allowed || blocked.PolicyDecision == null || !blocked.PolicyDecision.IsDenied)
                throw new InvalidOperationException("Denied candidate decision was not captured deterministically.");

            var disabled = FindSourceDecision(result.Decisions, "knowledge-disabled");
            if (disabled == null || disabled.Allowed || disabled.ResourceCapabilityState != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Disabled source capability decision was not captured deterministically.");

            Write(
                "CONTEXT POLICY ASSEMBLY",
                "Policy-aware context admission and bounded assembly succeeded." + Environment.NewLine +
                "Allowed candidate retained: verified." + Environment.NewLine +
                "Denied candidate excluded before budget assembly: verified." + Environment.NewLine +
                "Disabled source excluded before retrieval: verified." + Environment.NewLine +
                "Provenance preservation: verified." + Environment.NewLine +
                "Safe policy/capability diagnostics: verified." + Environment.NewLine +
                "Provider request: none.");

            return Task.CompletedTask;
        }

        private static ContextAdmissionDecision FindDecision(
            IReadOnlyList<ContextAdmissionDecision> decisions,
            string itemId)
        {
            foreach (var decision in decisions)
            {
                if (string.Equals(decision.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                    return decision;
            }
            return null;
        }

        private static ContextAdmissionDecision FindSourceDecision(
            IReadOnlyList<ContextAdmissionDecision> decisions,
            string sourceId)
        {
            foreach (var decision in decisions)
            {
                if (string.Equals(decision.SourceId, sourceId, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(decision.ItemId))
                    return decision;
            }
            return null;
        }

        private static ContextItem CreatePolicyExampleItem(string id, string sourceKind, int characters, int tokens)
        {
            return new ContextItem
            {
                Id = id,
                Source = "ContextPolicyExample",
                Type = "record",
                Payload = id + "-payload",
                Provenance = new ContextProvenance
                {
                    SourceKind = sourceKind,
                    SourceId = "source-1",
                    SourceVersion = "1",
                    Evidence = "Deterministic policy-aware context example"
                },
                Scope = new ContextScope
                {
                    ScopeType = "User",
                    ScopeId = "user-42"
                },
                Trust = 1d,
                Importance = 1d,
                Relevance = 1d,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class RecordingContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public RecordingContextSource(string kind, string id, params ContextItem[] items)
            {
                Kind = kind;
                Id = id;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public int CallCount { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                CallCount++;
                return Task.FromResult(_items);
            }
        }
    }
}
