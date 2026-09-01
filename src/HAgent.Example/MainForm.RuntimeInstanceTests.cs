using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeInstanceTab()
        {
            AddApiTab(
                "Runtime Instances",
                "Run runtime instance test",
                "Verifies that one reusable agent profile can create multiple independent runtime identities without changing the stored profile.",
                "Two instances from the same profile should have distinct identities, the same profile reference, the requested scopes, and independent retirement state.",
                "Runtime instance verification.",
                TestRuntimeInstanceAsync,
                "Profile → instances",
                "Provider-independent deterministic model test; no network or storage mutation.");

            AddApiTab(
                "RUNTIME CONCURRENCY",
                "Run runtime concurrency test",
                "Runs two independent runtime instances concurrently against a local adapter and verifies that execution identities and results remain isolated.",
                "Both executions should overlap, complete successfully, and retain distinct instance, execution, and correlation identities.",
                "Runtime concurrency verification.",
                TestRuntimeConcurrencyAsync,
                "Two independent instances",
                "Uses only a local adapter; no external provider is contacted.");
        }

        private Task TestRuntimeInstanceAsync(string message)
        {
            var profile = new AiAgent
            {
                Id = "runtime-profile-42",
                Name = "Runtime Instance Test Profile"
            };

            var first = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Session);
            var second = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);

            if (string.Equals(first.InstanceId, second.InstanceId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances created from one profile must have distinct instance IDs.");
            if (!string.Equals(first.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(second.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances did not preserve the reusable profile ID.");
            if (first.Scope != AgentRuntimeScope.Session)
                throw new InvalidOperationException("The first runtime instance did not preserve its requested scope.");
            if (second.Scope != AgentRuntimeScope.Task)
                throw new InvalidOperationException("The second runtime instance did not preserve its requested scope.");
            if (first.State != AgentRuntimeInstanceState.Active || second.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("New runtime instances must start active.");
            if (!string.Equals(profile.Id, "runtime-profile-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime instance creation mutated the reusable profile identity.");
            if (!string.Equals(first.MemoryOwnerId, first.InstanceId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(second.MemoryOwnerId, second.InstanceId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(first.MemoryOwnerId, second.MemoryOwnerId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances must have independent memory owners.");

            first.Retire();
            if (first.State != AgentRuntimeInstanceState.Retired)
                throw new InvalidOperationException("Retiring the first runtime instance did not change its state.");
            if (second.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Retiring one runtime instance must not retire another instance from the same profile.");

            Write("RUNTIME INSTANCES",
                "Contract test succeeded." + Environment.NewLine +
                "Profile: " + profile.Id + Environment.NewLine +
                "Instance 1: " + first.InstanceId + Environment.NewLine +
                "Instance 1 scope: " + first.Scope + Environment.NewLine +
                "Instance 1 memory owner: " + first.MemoryOwnerId + Environment.NewLine +
                "Instance 1 state after retire: " + first.State + Environment.NewLine +
                "Instance 2: " + second.InstanceId + Environment.NewLine +
                "Instance 2 scope: " + second.Scope + Environment.NewLine +
                "Instance 2 memory owner: " + second.MemoryOwnerId + Environment.NewLine +
                "Instance 2 state: " + second.State + Environment.NewLine +
                "Independent memory owners: yes" + Environment.NewLine +
                "Profile remained reusable: yes.");

            return Task.CompletedTask;
        }
    }
}
