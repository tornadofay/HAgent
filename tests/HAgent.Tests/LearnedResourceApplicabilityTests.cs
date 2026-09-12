using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearnedResourceApplicabilityTests
    {
        [Fact]
        public async Task Evaluator_ReturnsApplicableWhenScopeAndAllPreconditionsMatch()
        {
            var target = CreateTarget();
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "invoice",
                EvidenceReferenceId = "ctx-document"
            });
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "language",
                Operator = AiApplicabilityConditionOperator.OneOf,
                AllowedValues = { "ar", "en" }
            });
            target.Evidence.Add(new AiApplicabilityEvidenceReference
            {
                Kind = "execution-context",
                Id = "ctx-document",
                Role = "precondition"
            });

            var request = new AiApplicabilityRequest
            {
                Target = target,
                Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
            };
            request.Context.Facts["document.type"] = "invoice";
            request.Context.Facts["language"] = "ar";

            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(request, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.Applicable, result.Outcome);
            Assert.Equal(2, result.Conditions.Count);
            Assert.Single(result.Evidence);
            Assert.Equal("ctx-document", result.Conditions[0].EvidenceReferenceId);
        }

        [Fact]
        public async Task Evaluator_ReturnsNotApplicableWhenPreconditionFails()
        {
            var target = CreateTarget();
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "invoice"
            });

            var context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent };
            context.Facts["document.type"] = "quotation";

            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                new AiApplicabilityRequest { Target = target, Context = context }, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.NotApplicable, result.Outcome);
        }

        [Fact]
        public async Task Evaluator_ReturnsUncertainWhenRequiredFactIsMissing()
        {
            var target = CreateTarget();
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "invoice"
            });

            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
                }, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.Uncertain, result.Outcome);
            Assert.Single(result.Conditions);
            Assert.False(result.Conditions[0].EvidenceAvailable);
        }

        [Fact]
        public async Task Evaluator_ReturnsUncertainWhenNotEqualsFactIsMissing()
        {
            var target = CreateTarget();
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.NotEquals,
                ExpectedValue = "credit-note"
            });

            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
                }, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.Uncertain, result.Outcome);
            Assert.Single(result.Conditions);
            Assert.False(result.Conditions[0].EvidenceAvailable);
            Assert.False(result.Conditions[0].Satisfied);
        }

        [Fact]
        public async Task Evaluator_ReturnsInvalidatedBeforePreconditions()
        {
            var target = CreateTarget();
            target.IsInvalidated = true;
            target.InvalidationReason = "Direct contradiction observed.";
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "document.type",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "invoice"
            });

            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
                }, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.Invalidated, result.Outcome);
            Assert.Equal("Direct contradiction observed.", result.Reason);
            Assert.Empty(result.Conditions);
        }

        [Fact]
        public async Task Evaluator_RejectsScopeMismatchWithoutTreatingItAsAuthorization()
        {
            var target = CreateTarget();
            var result = await new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                new AiApplicabilityRequest
                {
                    Target = target,
                    Context = new AiApplicabilityContext { Scope = AgentResourceScope.User }
                }, CancellationToken.None);

            Assert.Equal(AiApplicabilityOutcome.NotApplicable, result.Outcome);
        }

        [Fact]
        public async Task Evaluator_CancellationIsObservedBeforeEvaluation()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                new AiDeterministicApplicabilityEvaluator().EvaluateAsync(
                    new AiApplicabilityRequest
                    {
                        Target = CreateTarget(),
                        Context = new AiApplicabilityContext { Scope = AgentResourceScope.Agent }
                    }, source.Token));
        }

        [Fact]
        public void Contracts_ValidateBoundsAndKeepAuthorizationSeparate()
        {
            var target = CreateTarget();
            target.Preconditions.Add(new AiApplicabilityCondition
            {
                Key = "authorized",
                Operator = AiApplicabilityConditionOperator.Equals,
                ExpectedValue = "false"
            });

            target.Validate();
            Assert.Equal(AiApplicabilityConditionOperator.Equals, target.Preconditions[0].Operator);
            Assert.False(target.IsInvalidated);
        }

        private static AiApplicabilityTarget CreateTarget()
        {
            return new AiApplicabilityTarget
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-example",
                Version = 2,
                Scope = AgentResourceScope.Agent
            };
        }
    }
}
