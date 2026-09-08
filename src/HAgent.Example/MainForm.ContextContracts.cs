using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
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

            var snapshot = new ContextSnapshot(new[] { item }, budget);
            if (snapshot.Items.Count != 1 || snapshot.Budget.MaxItems != 10)
                throw new InvalidOperationException("Context snapshot did not retain the validated contract data.");

            snapshot.Items[0].Validate();
            Console.WriteLine("[CONTEXT CONTRACTS] Contract validation, bounded metadata, cloning, and snapshot creation succeeded.");
            return Task.FromResult(0);
        }
    }
}
