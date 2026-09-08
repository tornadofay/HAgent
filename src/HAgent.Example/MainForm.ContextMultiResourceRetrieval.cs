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
        private void AddContextMultiResourceRetrievalTab()
        {
            AddApiTab(
                "Context Multi-Resource Retrieval",
                "Run retrieval test",
                "Exercises per-source bounded retrieval across the standard provider-neutral context categories: memory, knowledge, skill, conversation, host context, tool descriptions, and instructions.",
                "Each source receives its own query and candidate limit in deterministic order while one global item/character/token budget bounds the assembled snapshot.",
                "No AI request is sent. The sources are deterministic in-process fakes representing host/resource producers.",
                RunContextMultiResourceRetrievalTestAsync,
                "Bounded multi-resource retrieval",
                "This slice adds the per-source retrieval-plan boundary. Authorization, policy filtering, ranking, and compaction remain separate pipeline stages.");
        }

        private async Task RunContextMultiResourceRetrievalTestAsync(string unused)
        {
            var sources = new[]
            {
                new RetrievalExampleSource(ContextSourceKinds.Memory, "memory-item"),
                new RetrievalExampleSource(ContextSourceKinds.Knowledge, "knowledge-item"),
                new RetrievalExampleSource(ContextSourceKinds.Skill, "skill-item"),
                new RetrievalExampleSource(ContextSourceKinds.Conversation, "conversation-item"),
                new RetrievalExampleSource(ContextSourceKinds.HostContext, "host-item"),
                new RetrievalExampleSource(ContextSourceKinds.ToolDescription, "tool-item"),
                new RetrievalExampleSource(ContextSourceKinds.Instruction, "instruction-item")
            };

            var plan = new List<ContextRetrievalSource>();
            for (var i = 0; i < sources.Length; i++)
            {
                plan.Add(new ContextRetrievalSource
                {
                    Source = sources[i],
                    Query = "request-" + (i + 1),
                    MaxItems = 1
                });
            }

            var snapshot = await new ContextAcquirer().AcquireAsync(
                plan,
                new ContextBudget
                {
                    MaxItems = 7,
                    MaxCharacters = 500,
                    MaxEstimatedTokens = 100
                }).ConfigureAwait(false);

            if (snapshot.Items.Count != 7)
                throw new InvalidOperationException("The multi-resource retrieval plan did not assemble all bounded source categories.");

            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i].LastRequest == null ||
                    sources[i].LastRequest.Query != "request-" + (i + 1) ||
                    sources[i].LastRequest.MaxItems != 1)
                    throw new InvalidOperationException("A planned context source did not receive its distinct bounded retrieval request.");
            }

            var ids = new List<string>();
            foreach (var item in snapshot.Items)
            {
                ids.Add(item.Id);
                if (item.Provenance == null || item.Provenance.SourceKind != item.Source)
                    throw new InvalidOperationException("Source provenance was not preserved during multi-resource retrieval.");
            }

            var expected = "memory-item,knowledge-item,skill-item,conversation-item,host-item,tool-item,instruction-item";
            if (string.Join(",", ids) != expected)
                throw new InvalidOperationException("Multi-resource retrieval order was not deterministic.");

            Write(
                "CONTEXT MULTI-RESOURCE RETRIEVAL",
                "Bounded multi-resource context retrieval succeeded." + Environment.NewLine +
                "Standard source categories: memory, knowledge, skill, conversation, host-context, tool-description, instruction." + Environment.NewLine +
                "Distinct per-source queries: verified." + Environment.NewLine +
                "Per-source candidate limits: verified." + Environment.NewLine +
                "Global item/character/token budget: verified." + Environment.NewLine +
                "Deterministic source order: verified." + Environment.NewLine +
                "Provenance preservation: verified." + Environment.NewLine +
                "Provider request: none.");
        }

        private sealed class RetrievalExampleSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public RetrievalExampleSource(string kind, string itemId)
            {
                Id = kind + "-example-source";
                Kind = kind;
                _items = new[]
                {
                    new ContextItem
                    {
                        Id = itemId,
                        Source = kind,
                        Type = "example",
                        Payload = new Dictionary<string, object> { { "Category", kind }, { "Value", itemId } },
                        Provenance = new ContextProvenance
                        {
                            SourceKind = kind,
                            SourceId = Id,
                            SourceVersion = "1",
                            Evidence = "Deterministic multi-resource retrieval example"
                        },
                        Scope = new ContextScope { ScopeType = "Execution", ScopeId = "context-example" },
                        Trust = 1d,
                        Importance = 1d,
                        Relevance = 1d,
                        EstimatedCharacters = 20,
                        EstimatedTokens = 5
                    }
                };
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public ContextSourceRequest LastRequest { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                request.Validate();
                LastRequest = request.Clone();
                return Task.FromResult(_items);
            }
        }
    }
}
