using System;
using System.Collections.Generic;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public class ContextContractTests
    {
        [Fact]
        public void ContextItem_ValidStructuredPayloadAndMetadata_PassesValidation()
        {
            var item = new ContextItem
            {
                Id = "context-1",
                Source = "host.form",
                Type = "application.state",
                Payload = new Dictionary<string, object> { { "count", 3 } },
                Trust = 0.9d,
                Importance = 0.8d,
                Relevance = 0.7d,
                EstimatedCharacters = 32,
                EstimatedTokens = 8,
                Scope = new ContextScope { ScopeType = "Runtime", ScopeId = "runtime-1" },
                Provenance = new ContextProvenance
                {
                    SourceKind = "WinForms",
                    SourceId = "form-1",
                    SourceVersion = "v1",
                    Evidence = "host supplied",
                    CapturedAt = DateTimeOffset.UtcNow
                }
            };

            item.Validate();

            Assert.Equal("application.state", item.Type);
            Assert.Equal(3, ((Dictionary<string, object>)item.Payload)["count"]);
            Assert.Equal("Runtime", item.Scope.ScopeType);
        }

        [Fact]
        public void ContextItem_CloneCopiesContractMetadataWithoutSharingScopeOrProvenance()
        {
            var item = new ContextItem
            {
                Id = "context-2",
                Source = "memory",
                Type = "memory.fact",
                Payload = "value",
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-1" },
                Provenance = new ContextProvenance { SourceKind = "Memory", SourceId = "memory-1" }
            };

            var clone = item.Clone();
            clone.Scope.ScopeId = "user-2";
            clone.Provenance.SourceId = "memory-2";

            Assert.NotSame(item, clone);
            Assert.NotSame(item.Scope, clone.Scope);
            Assert.NotSame(item.Provenance, clone.Provenance);
            Assert.Equal("user-1", item.Scope.ScopeId);
            Assert.Equal("memory-1", item.Provenance.SourceId);
        }

        [Theory]
        [InlineData(-0.1d, 0.5d, 0.5d)]
        [InlineData(0.5d, 1.1d, 0.5d)]
        [InlineData(0.5d, 0.5d, double.NaN)]
        public void ContextItem_InvalidScores_AreRejected(double trust, double importance, double relevance)
        {
            var item = CreateValidItem();
            item.Trust = trust;
            item.Importance = importance;
            item.Relevance = relevance;

            Assert.Throws<ArgumentOutOfRangeException>(() => item.Validate());
        }

        [Fact]
        public void ContextBudget_RejectsNonPositiveLimits()
        {
            var budget = new ContextBudget { MaxItems = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => budget.Validate());

            budget = new ContextBudget { MaxCharacters = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => budget.Validate());

            budget = new ContextBudget { MaxEstimatedTokens = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => budget.Validate());
        }

        [Fact]
        public void ContextBudget_CloneIsIndependent()
        {
            var budget = new ContextBudget
            {
                MaxItems = 20,
                MaxCharacters = 4000,
                MaxEstimatedTokens = 1000
            };

            var clone = budget.Clone();
            clone.MaxItems = 5;
            clone.MaxCharacters = 500;
            clone.MaxEstimatedTokens = 125;

            Assert.Equal(20, budget.MaxItems);
            Assert.Equal(4000, budget.MaxCharacters);
            Assert.Equal(1000, budget.MaxEstimatedTokens);
        }

        [Fact]
        public void ContextSourceRequest_RejectsOversizedBounds()
        {
            var request = new ContextSourceRequest { MaxItems = 1001 };
            Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());

            request = new ContextSourceRequest { Query = new string('x', 4097) };
            Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());
        }

        [Fact]
        public void ContextSourceRequest_CloneIsIndependent()
        {
            var request = new ContextSourceRequest { Query = "customer", MaxItems = 10 };
            var clone = request.Clone();
            clone.Query = "order";
            clone.MaxItems = 2;

            Assert.Equal("customer", request.Query);
            Assert.Equal(10, request.MaxItems);
        }

        [Fact]
        public void ContextItem_RejectsMissingAuthorityMetadata()
        {
            var item = CreateValidItem();
            item.Provenance = null;
            Assert.Throws<ArgumentException>(() => item.Validate());

            item = CreateValidItem();
            item.Scope = null;
            Assert.Throws<ArgumentException>(() => item.Validate());
        }

        private static ContextItem CreateValidItem()
        {
            return new ContextItem
            {
                Id = "context-valid",
                Source = "test",
                Type = "test.value",
                Payload = "payload",
                Provenance = new ContextProvenance { SourceKind = "test", SourceId = "source-1" },
                Scope = new ContextScope { ScopeType = "Execution", ScopeId = "execution-1" }
            };
        }
    }
}
