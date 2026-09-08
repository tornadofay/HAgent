using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddContextContractsTab()
        {
            AddApiTab(
                "Context Contracts",
                "Run context contract test",
                "Exercises the provider-neutral context contracts with bounded provenance, scope, trust, importance, freshness, size metadata, structured payloads, explicit budget dimensions, source requests, and clone isolation.",
                "Contract validation, bounded metadata, structured payload, explicit budget/source bounds, and nested clone isolation should all succeed without contacting an AI provider.",
                "No AI request is sent by this example.",
                RunContextContractsTestAsync,
                "Context contract boundary",
                "This slice establishes reusable Core contracts only. Ranking, compaction, caching, provider transport, and execution integration remain later slices.");
        }

        private Task RunContextContractsTestAsync(string unused)
        {
            var provenance = new ContextProvenance
            {
                SourceKind = "Host",
                SourceId = "customer-42",
                SourceVersion = "v1",
                Evidence = "Deterministic contract example"
            };
            provenance.Validate();

            var item = new ContextItem
            {
                Source = "Host",
                Type = "Customer",
                Payload = new Dictionary<string, object>
                {
                    { "Id", 42 },
                    { "Name", "Example Customer" }
                },
                Provenance = provenance,
                Trust = 0.9d,
                Importance = 0.8d,
                Relevance = 0.95d,
                EstimatedCharacters = 64,
                EstimatedTokens = 16,
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" }
            };
            item.Validate();

            var budget = new ContextBudget
            {
                MaxItems = 10,
                MaxCharacters = 1000,
                MaxEstimatedTokens = 250
            };
            budget.Validate();

            var request = new ContextSourceRequest
            {
                Query = "customer 42",
                MaxItems = 10
            };
            request.Validate();

            var clone = item.Clone();
            clone.Validate();
            if (clone == item || clone.Provenance == item.Provenance || clone.Scope == item.Scope)
                throw new InvalidOperationException("Context contract clone did not isolate nested contract objects.");

            var budgetClone = budget.Clone();
            budgetClone.Validate();
            if (budgetClone == budget)
                throw new InvalidOperationException("Context budget clone did not create a distinct contract instance.");

            var requestClone = request.Clone();
            requestClone.Validate();
            if (requestClone == request || requestClone.Query != request.Query || requestClone.MaxItems != request.MaxItems)
                throw new InvalidOperationException("Context source request clone did not preserve isolated contract data.");

            Console.WriteLine("[CONTEXT CONTRACTS] Contract validation, bounded metadata, cloning, structured payload, and source request validation succeeded.");
            return Task.FromResult(0);
        }
    }
}
