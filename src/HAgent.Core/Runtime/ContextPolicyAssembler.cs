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
    /// Host-owned data authorization is invoked only for sources that explicitly opt into that boundary.
    /// Final ranking and bounded assembly are separate stages.
    /// </summary>
    public sealed class ContextPolicyAssembler
    {
        private readonly IContextAcquirer _acquirer;
        private readonly IContextAdmissionEvaluator _admissionEvaluator;
        private readonly IDataAccessAuthorizer _hostAuthorizer;

        public ContextPolicyAssembler(
            IContextAcquirer acquirer,
            IContextAdmissionEvaluator admissionEvaluator,
            IDataAccessAuthorizer hostAuthorizer = null)
        {
            _acquirer = acquirer ?? throw new ArgumentNullException(nameof(acquirer));
            _admissionEvaluator = admissionEvaluator ?? throw new ArgumentNullException(nameof(admissionEvaluator));
            _hostAuthorizer = hostAuthorizer;
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

            var retrieval = await RetrieveCandidatesAsync(sources, context, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var snapshot = await _acquirer.AcquireAsync(
                new[] { new CandidateListContextSource(retrieval.Candidates) },
                new ContextSourceRequest { Query = "policy-filtered", MaxItems = Math.Max(1, retrieval.Candidates.Count) },
                budget,
                cancellationToken).ConfigureAwait(false);

            return new ContextPolicyAssemblyResult(snapshot, retrieval.Decisions);
        }

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
                if (!sourceDecision.Allowed)
                {
                    decisions.Add(sourceDecision.Clone());
                    continue;
                }

                var hostAuthorized = await AuthorizeHostDataSourceAsync(source, context, sourceDecision, cancellationToken)
                    .ConfigureAwait(false);
                decisions.Add(hostAuthorized.Clone());
                if (!hostAuthorized.Allowed)
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

        private async Task<ContextAdmissionDecision> AuthorizeHostDataSourceAsync(
            ContextRetrievalSource source,
            ContextAdmissionContext context,
            ContextAdmissionDecision sourceDecision,
            CancellationToken cancellationToken)
        {
            var dataSource = source.Source as IContextDataAuthorizationSource;
            if (dataSource == null)
            {
                sourceDecision.HostAuthorizationState = ContextHostAuthorizationState.NotRequired;
                sourceDecision.HostAuthorizationReason = "Host data authorization is not required for this source.";
                return sourceDecision;
            }

            sourceDecision.HostAuthorizationState = ContextHostAuthorizationState.Denied;
            if (_hostAuthorizer == null)
            {
                sourceDecision.Allowed = false;
                sourceDecision.Reason = "Context source requires host data authorization, but no host authorizer is available.";
                sourceDecision.HostAuthorizationReason = "Host authorization is unavailable; access was denied.";
                return sourceDecision;
            }

            var request = CreateHostAuthorizationRequest(source, dataSource, context);
            cancellationToken.ThrowIfCancellationRequested();

            bool authorized;
            try
            {
                authorized = await _hostAuthorizer.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                sourceDecision.Allowed = false;
                sourceDecision.Reason = "Context source host authorization could not be completed; access was denied.";
                sourceDecision.HostAuthorizationReason = "Host authorization failed closed.";
                return sourceDecision;
            }

            if (!authorized)
            {
                sourceDecision.Allowed = false;
                sourceDecision.Reason = "Context source was denied by host data authorization.";
                sourceDecision.HostAuthorizationReason = "Host authorization denied access.";
                return sourceDecision;
            }

            sourceDecision.HostAuthorizationState = ContextHostAuthorizationState.Allowed;
            sourceDecision.HostAuthorizationReason = "Host authorization allowed access.";
            return sourceDecision;
        }

        private static DataAuthorizationRequest CreateHostAuthorizationRequest(
            ContextRetrievalSource source,
            IContextDataAuthorizationSource dataSource,
            ContextAdmissionContext context)
        {
            var query = dataSource.AuthorizationQuery;
            if (query != null)
                query.Validate();

            return new DataAuthorizationRequest
            {
                Operation = dataSource.AuthorizationOperation,
                SourceId = source.Source.Id,
                RuntimeIdentity = context.RuntimeInstanceId ?? string.Empty,
                Identity = context.Identity == null ? new AgentIdentityContext() : context.Identity.Clone(),
                RuntimeContext = CloneRuntimeContext(dataSource.AuthorizationRuntimeContext),
                Query = CloneQuery(query)
            };
        }

        private static IReadOnlyDictionary<string, object> CloneRuntimeContext(
            IReadOnlyDictionary<string, object> runtimeContext)
        {
            var clone = new Dictionary<string, object>(StringComparer.Ordinal);
            if (runtimeContext == null)
                return clone;

            foreach (var pair in runtimeContext)
                clone[pair.Key] = pair.Value;
            return clone;
        }

        private static DataQueryRequest CloneQuery(DataQueryRequest query)
        {
            if (query == null)
                return null;

            var filters = new List<DataFilterCondition>();
            foreach (var filter in query.Filters)
            {
                filters.Add(new DataFilterCondition
                {
                    Field = filter.Field,
                    Operator = filter.Operator,
                    Value = filter.Value
                });
            }

            var sorts = new List<DataSort>();
            foreach (var sort in query.Sorts)
            {
                sorts.Add(new DataSort
                {
                    Field = sort.Field,
                    Descending = sort.Descending
                });
            }

            return new DataQueryRequest
            {
                Fields = new List<string>(query.Fields),
                Filters = filters,
                Sorts = sorts,
                Skip = query.Skip,
                Take = query.Take
            };
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
