using System;
using System.Collections.Generic;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public class ContextContractsTests
    {
        [Fact]
        public void ContextItem_ValidatesAndClonesNestedMetadata()
        {
            var item = CreateItem();

            item.Validate();
            var clone = item.Clone();

            Assert.NotSame(item, clone);
            Assert.NotSame(item.Provenance, clone.Provenance);
            Assert.NotSame(item.Scope, clone.Scope);
            Assert.Equal(item.Source, clone.Source);
            Assert.Equal(item.Type, clone.Type);
            Assert.Equal(item.Trust, clone.Trust);
            Assert.Equal(item.Importance, clone.Importance);
            Assert.Equal(item.Relevance, clone.Relevance);
            Assert.Equal(item.EstimatedCharacters, clone.EstimatedCharacters);
            Assert.Equal(item.EstimatedTokens, clone.EstimatedTokens);
        }

        [Fact]
        public void ContextItem_RejectsInvalidBounds()
        {
            var item = CreateItem();
            item.Trust = 1.1d;
            Assert.Throws<ArgumentOutOfRangeException>(() => item.Validate());

            item = CreateItem();
            item.EstimatedCharacters = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => item.Validate());
        }

        [Fact]
        public void ContextBudget_ValidatesAndClones()
        {
            var budget = new ContextBudget
            {
                MaxItems = 10,
                MaxCharacters = 1000,
                MaxEstimatedTokens = 250
            };

            budget.Validate();
            var clone = budget.Clone();

            Assert.NotSame(budget, clone);
            Assert.Equal(budget.MaxItems, clone.MaxItems);
            Assert.Equal(budget.MaxCharacters, clone.MaxCharacters);
            Assert.Equal(budget.MaxEstimatedTokens, clone.MaxEstimatedTokens);
            clone.MaxItems = 20;
            Assert.Equal(10, budget.MaxItems);
        }

        [Fact]
        public void ContextSourceRequest_ValidatesBoundsAndClones()
        {
            var request = new ContextSourceRequest
            {
                Query = "customer 42",
                MaxItems = 10
            };

            request.Validate();
            var clone = request.Clone();

            Assert.NotSame(request, clone);
            Assert.Equal(request.Query, clone.Query);
            Assert.Equal(request.MaxItems, clone.MaxItems);

            request.MaxItems = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());
        }

        private static ContextItem CreateItem()
        {
            return new ContextItem
            {
                Id = "context-42",
                Source = "Host",
                Type = "Customer",
                Payload = new Dictionary<string, object>
                {
                    { "Id", 42 },
                    { "Name", "Example Customer" }
                },
                Provenance = new ContextProvenance
                {
                    SourceKind = "Host",
                    SourceId = "customer-42",
                    SourceVersion = "v1",
                    Evidence = "Unit test"
                },
                Scope = new ContextScope
                {
                    ScopeType = "User",
                    ScopeId = "user-42"
                },
                Trust = 0.9d,
                Importance = 0.8d,
                Relevance = 0.95d,
                EstimatedCharacters = 64,
                EstimatedTokens = 16
            };
        }
    }
}
