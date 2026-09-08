using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Applies the canonical policy/resource admission boundary to context sources and candidates.
    /// Final ranking and bounded assembly are separate stages.
    /// </summary>
    public sealed class ContextPolicyAssembler
    {
        private readonly IContextAcquirer _acquirer;
        private readonly IContextAdmissionEvaluator _admissionEvaluator;

        public ContextPolicyAssembler(
            IContextAcquirer acquirer,
            IContextAdmissionEvaluator admissionEvaluator)
        {
            _acquirer = acquirer ?? throw new ArgumentNullException(nameof(acquirer));
            _admissionEvaluator = admissionEvaluator ?? throw new ArgumentNullException(nameof(admissionEvaluator));
        }

        /// <summary>
        /// Legacy bounded policy-aware acquisition used by focused admission scenarios.
        /// </summary>
        public async Task<ContextPolicyAssemblyResult> AcquireAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextBudget budget,
            ContextAdmissionContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var retrieval = await RetrieveCandidatesAsync(sources, context, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var snapshot = await _acquirer.AcquireAsync(
                new[] { new CandidateListContextSource(retrieval.Candidates) },
                new ContextSourceRequest { Query = "policy-filtered", MaxItems = Math.Max(1, retrieval.Candidates.Count) },
                budget,
                cancellationToken).ConfigureAwait(false);

            return new ContextPolicyAssemblyResult(snapshot, retrieval.Decisions);
        }

        /// <summary>
        /// Retrieves per-source bounded candidates and applies source/item admission without consuming the final assembly budget.
        /// </summary>
        public async Task<ContextPolicyRetrievalResult> RetrieveCandidatesAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextAdmissionContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.Validate();

            var decisions = new List<ContextAdmissionDecision>();
            var candidates = new List<ContextItem>();

            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (source == null)
                    throw new ArgumentException("Context retrieval sources cannot contain null values.", nameof(sources));

                var sourceDecision = _admissionEvaluator.EvaluateSource(source, context);
                decisions.Add(sourceDecision.Clone());
                if (!sourceDecision.Allowed)
                    continue;

                var request = source.CreateRequest();
                var items = await source.Source.GetCandidatesAsync(request, cancellationToken).ConfigureAwait(false);
                if (items == null)
                    throw new InvalidOperationException("A context source returned a null candidate collection.");

                var returned = 0;
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (item == null)
                        throw new InvalidOperationException("A context source returned a null context item.");
                    if (returned >= request.MaxItems)
                        break;
                    returned++;

                    item.Validate();
                    var decision = _admissionEvaluator.EvaluateItem(source, item, context);
                    decisions.Add(decision.Clone());
                    if (decision.Allowed)
                        candidates.Add(item.Clone());
                }
            }

            return new ContextPolicyRetrievalResult(candidates, decisions);
        }

        private sealed class CandidateListContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public CandidateListContextSource(IReadOnlyList<ContextItem> items)
            {
                _items = items ?? throw new ArgumentNullException(nameof(items));
                Id = "policy-filtered";
                Kind = "policy-filtered";
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
    }
}
