using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Canonical provider-neutral context assembly pipeline.
    /// Order is policy admission, bounded retrieval, ranking/deduplication, then final bounded compaction.
    /// </summary>
    public sealed class ContextAssembler : IContextAssembler
    {
        private readonly ContextPolicyAssembler _policyAssembler;
        private readonly IContextRanker _ranker;
        private readonly IContextCompactor _compactor;

        public ContextAssembler(
            ContextPolicyAssembler policyAssembler,
            IContextRanker ranker,
            IContextCompactor compactor)
        {
            _policyAssembler = policyAssembler ?? throw new ArgumentNullException(nameof(policyAssembler));
            _ranker = ranker ?? throw new ArgumentNullException(nameof(ranker));
            _compactor = compactor ?? throw new ArgumentNullException(nameof(compactor));
        }

        public async Task<ContextAssemblyResult> AssembleAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextBudget budget,
            ContextAdmissionContext admissionContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (admissionContext == null) throw new ArgumentNullException(nameof(admissionContext));
            budget.Validate();
            admissionContext.Validate();

            cancellationToken.ThrowIfCancellationRequested();

            var policyResult = await _policyAssembler.RetrieveCandidatesAsync(
                sources,
                admissionContext,
                cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var ranked = _ranker.Rank(policyResult.Candidates);
            cancellationToken.ThrowIfCancellationRequested();

            var compaction = _compactor.Compact(
                ranked,
                new ContextCompactionOptions
                {
                    TargetBudget = budget.Clone(),
                    CaptureDiagnostics = true,
                    ContinueAfterExcludedCandidate = true
                });

            cancellationToken.ThrowIfCancellationRequested();

            return new ContextAssemblyResult(
                compaction.Snapshot,
                policyResult.Decisions,
                compaction);
        }
    }
}
