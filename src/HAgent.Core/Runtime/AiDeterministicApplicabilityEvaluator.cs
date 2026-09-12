using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiDeterministicApplicabilityEvaluator : IAiApplicabilityEvaluator
    {
        public Task<AiApplicabilityDecision> EvaluateAsync(
            AiApplicabilityRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var target = request.Target;
            var context = request.Context;
            var decision = CreateBaseDecision(target);

            CopyEvidence(target.Evidence, decision.Evidence);

            if (target.IsInvalidated)
                return Task.FromResult(Complete(decision, AiApplicabilityOutcome.Invalidated,
                    string.IsNullOrWhiteSpace(target.InvalidationReason)
                        ? "The resource has been explicitly invalidated."
                        : target.InvalidationReason));

            if (context.Scope.HasValue && context.Scope.Value != target.Scope)
                return Task.FromResult(Complete(decision, AiApplicabilityOutcome.NotApplicable,
                    "The applicability context scope does not match the resource scope."));

            if (!context.Scope.HasValue && target.Scope != AgentResourceScope.Global)
                return Task.FromResult(Complete(decision, AiApplicabilityOutcome.Uncertain,
                    "The resource requires scope-aware applicability evaluation, but the context did not provide a scope."));

            var uncertain = false;
            foreach (var condition in target.Preconditions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string observed;
                var hasFact = context.Facts.TryGetValue(condition.Key, out observed);
                var result = EvaluateCondition(condition, hasFact, observed);
                decision.Conditions.Add(result);

                if (!hasFact)
                {
                    uncertain = true;
                    continue;
                }

                if (!result.Satisfied)
                    return Task.FromResult(Complete(decision, AiApplicabilityOutcome.NotApplicable,
                        "A deterministic applicability precondition was not satisfied."));
            }

            return Task.FromResult(Complete(
                decision,
                uncertain ? AiApplicabilityOutcome.Uncertain : AiApplicabilityOutcome.Applicable,
                uncertain
                    ? "Applicability could not be established because required deterministic evidence was missing."
                    : "All bounded deterministic applicability preconditions were satisfied."));
        }

        private static AiApplicabilityDecision Complete(
            AiApplicabilityDecision decision,
            AiApplicabilityOutcome outcome,
            string reason)
        {
            decision.Outcome = outcome;
            decision.Reason = reason;
            decision.Validate();
            return decision;
        }

        private static AiApplicabilityDecision CreateBaseDecision(AiApplicabilityTarget target)
        {
            return new AiApplicabilityDecision
            {
                ResourceType = target.ResourceType,
                ResourceId = target.ResourceId,
                Version = target.Version,
                Scope = target.Scope
            };
        }

        private static void CopyEvidence(
            IEnumerable<AiApplicabilityEvidenceReference> source,
            IList<AiApplicabilityEvidenceReference> destination)
        {
            foreach (var evidence in source ?? Enumerable.Empty<AiApplicabilityEvidenceReference>())
                destination.Add(evidence.Clone());
        }

        private static AiApplicabilityConditionResult EvaluateCondition(
            AiApplicabilityCondition condition,
            bool hasFact,
            string observed)
        {
            var result = new AiApplicabilityConditionResult
            {
                Key = condition.Key,
                Operator = condition.Operator,
                ExpectedValue = condition.ExpectedValue,
                ObservedValue = observed,
                EvidenceReferenceId = condition.EvidenceReferenceId,
                EvidenceAvailable = hasFact
            };

            switch (condition.Operator)
            {
                case AiApplicabilityConditionOperator.Exists:
                    result.Satisfied = hasFact;
                    break;
                case AiApplicabilityConditionOperator.Equals:
                    result.Satisfied = hasFact && string.Equals(observed, condition.ExpectedValue, StringComparison.Ordinal);
                    break;
                case AiApplicabilityConditionOperator.NotEquals:
                    result.Satisfied = hasFact && !string.Equals(observed, condition.ExpectedValue, StringComparison.Ordinal);
                    break;
                case AiApplicabilityConditionOperator.OneOf:
                    result.Satisfied = hasFact && condition.AllowedValues.Contains(observed, StringComparer.Ordinal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition.Operator));
            }

            return result;
        }
    }
}
