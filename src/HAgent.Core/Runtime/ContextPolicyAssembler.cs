using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Retrieves and assembles context only from sources and items admitted by the canonical policy/capability boundary.
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

        public async Task<ContextPolicyAssemblyResult> AcquireAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextBudget budget,
            ContextAdmissionContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (context == null) throw new ArgumentNullException(nameof(context));

            budget.Validate();
            context.Validate();

            var decisions = new List<ContextAdmissionDecision>();
            var admittedSources = new List<ContextRetrievalSource>();
            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (source == null)
                    throw new ArgumentException("Context retrieval sources cannot contain null values.", nameof(sources));

                var sourceDecision = _admissionEvaluator.EvaluateSource(source, context);
                decisions.Add(sourceDecision.Clone());
                if (sourceDecision.Allowed)
                    admittedSources.Add(new AdmittedContextRetrievalSource(source, _admissionEvaluator, context, decisions));
            }

            var snapshot = await _acquirer.AcquireAsync(admittedSources, budget, cancellationToken).ConfigureAwait(false);
            return new ContextPolicyAssemblyResult(snapshot, decisions.AsReadOnly());
        }

        private sealed class AdmittedContextRetrievalSource : ContextRetrievalSource
        {
            private readonly IContextAdmissionEvaluator _evaluator;
            private readonly ContextAdmissionContext _context;
            private readonly IList<ContextAdmissionDecision> _decisions;

            public AdmittedContextRetrievalSource(
                ContextRetrievalSource source,
                IContextAdmissionEvaluator evaluator,
                ContextAdmissionContext context,
                IList<ContextAdmissionDecision> decisions)
            {
                Source = new FilteringContextSource(source.Source, evaluator, source, context, decisions);
                Query = source.Query;
                MaxItems = source.MaxItems;
            }
        }

        private sealed class FilteringContextSource : IContextSource
        {
            private readonly IContextSource _inner;
            private readonly IContextAdmissionEvaluator _evaluator;
            private readonly ContextRetrievalSource _source;
            private readonly ContextAdmissionContext _context;
            private readonly IList<ContextAdmissionDecision> _decisions;

            public FilteringContextSource(
                IContextSource inner,
                IContextAdmissionEvaluator evaluator,
                ContextRetrievalSource source,
                ContextAdmissionContext context,
                IList<ContextAdmissionDecision> decisions)
            {
                _inner = inner;
                _evaluator = evaluator;
                _source = source;
                _context = context.Clone();
                _decisions = decisions;
            }

            public string Id { get { return _inner.Id; } }
            public string Kind { get { return _inner.Kind; } }

            public async Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var candidates = await _inner.GetCandidatesAsync(request, cancellationToken).ConfigureAwait(false);
                if (candidates == null)
                    throw new InvalidOperationException("A context source returned a null candidate collection.");

                var admitted = new List<ContextItem>();
                foreach (var item in candidates)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (item == null)
                        throw new InvalidOperationException("A context source returned a null context item.");

                    var decision = _evaluator.EvaluateItem(_source, item, _context);
                    _decisions.Add(decision.Clone());
                    if (decision.Allowed)
                        admitted.Add(item.Clone());
                }

                return admitted;
            }
        }
    }
}
