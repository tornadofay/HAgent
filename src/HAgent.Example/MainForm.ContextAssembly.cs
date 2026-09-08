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
        private void AddContextAssemblyTab()
        {
            AddApiTab(
                "Context Assembly",
                "Run end-to-end context assembly",
                "Exercises the canonical context pipeline: policy admission, bounded retrieval, deterministic ranking/deduplication, and final budgeted compaction.",
                "Denied candidates should disappear before ranking, the higher-relevance candidate should win, duplicates should collapse, and the final snapshot must remain within the requested budget.",
                "No AI request is sent. Every stage uses provider-neutral Core contracts and the unified policy model.",
                RunContextAssemblyTestAsync,
                "End-to-end bounded context pipeline",
                "Instruction authority remains in the separate 0.954 instruction-governance subsystem; this example verifies context evidence assembly only.");
        }

        private async Task RunContextAssemblyTestAsync(string unused)
        {
            var source = new ExampleContextSource(
                "memory",
                "assembly-memory",
                CreateAssemblyItem("blocked", 10, 1, 0.1d),
                CreateAssemblyItem("customer", 15, 2, 1d),
                CreateAssemblyItem("customer", 15, 2, 0.9d),
                CreateAssemblyItem("secondary", 15, 2, 0.7d));

            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-blocked",
                Name = "Deny blocked context",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.include" },
                ResourceTypes = { "memory" },
                ResourceIds = { "blocked" },
                Reason = "Deterministic end-to-end example exclusion."
            });
            policy.Validate();

            var capabilities = AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy());
            var admission = new ContextPolicyAdmissionEvaluator(
                new DefaultAiPolicyEngine(policy),
                capabilities);
            var assembler = new ContextAssembler(
                new ContextPolicyAssembler(new ContextAcquirer(), admission),
                new ContextRanker(),
                new ContextCompactor());

            var result = await assembler.AssembleAsync(
                new[]
                {
                    new ContextRetrievalSource
                    {
                        Source = source,
                        Query = "customer",
                        MaxItems = 4
                    }
                },
                new ContextBudget
                {
                    MaxItems = 2,
                    MaxCharacters = 30,
                    MaxEstimatedTokens = 4
                },
                new ContextAdmissionContext { AgentProfileId = "assistant" },
                CancellationToken.None);

            if (result.Snapshot.Items.Count != 2 ||
                result.Snapshot.Items[0].Id != "customer" ||
                result.Snapshot.Items[1].Id != "secondary")
                throw new InvalidOperationException("End-to-end context assembly did not produce the deterministic ranked/deduplicated result.");

            if (result.Snapshot.UsedCharacters > 30 ||
                (result.Snapshot.UsedEstimatedTokens.HasValue && result.Snapshot.UsedEstimatedTokens.Value > 4))
                throw new InvalidOperationException("Final context snapshot exceeded the requested global budget.");

            var blocked = FindAssemblyDecision(result.AdmissionDecisions, "blocked");
            if (blocked == null || blocked.Allowed || blocked.PolicyDecision == null || !blocked.PolicyDecision.IsDenied)
                throw new InvalidOperationException("Policy-denied context was not excluded before ranking.");

            if (result.Snapshot.Items[0].Provenance.SourceId != "assembly-source")
                throw new InvalidOperationException("Context provenance was not preserved through the complete pipeline.");

            Write(
                "CONTEXT ASSEMBLY",
                "End-to-end bounded context assembly succeeded." + Environment.NewLine +
                "Policy admission before ranking: verified." + Environment.NewLine +
                "Deterministic ranking/deduplication: verified." + Environment.NewLine +
                "Final item/character/token budget: verified." + Environment.NewLine +
                "Provenance preservation: verified." + Environment.NewLine +
                "Provider request: none.");
        }

        private static ContextAdmissionDecision FindAssemblyDecision(
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

        private static ContextItem CreateAssemblyItem(string id, int characters, int tokens, double relevance)
        {
            return new ContextItem
            {
                Id = id,
                Source = "ContextAssemblyExample",
                Type = "record",
                Payload = id + "-payload",
                Provenance = new ContextProvenance
                {
                    SourceKind = "memory",
                    SourceId = "assembly-source",
                    SourceVersion = "1",
                    Evidence = "Deterministic end-to-end context assembly example"
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 1d,
                Importance = 1d,
                Relevance = relevance,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class ExampleContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public ExampleContextSource(string kind, string id, params ContextItem[] items)
            {
                Kind = kind;
                Id = id;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = Math.Min(request.MaxItems, _items.Count);
                var result = new List<ContextItem>(count);
                for (var i = 0; i < count; i++)
                    result.Add(_items[i]);
                return Task.FromResult<IReadOnlyList<ContextItem>>(result);
            }
        }
    }
}
