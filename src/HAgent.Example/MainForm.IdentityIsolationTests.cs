using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestIdentityIsolationAsync(string unused)
        {
            var memory = new InMemoryMemoryStore();
            var tenantOneUserA = new AgentIdentityContext(
                deploymentId: "deployment-isolation-1",
                tenantId: "tenant-a",
                principalId: "principal-a1",
                userId: "user-42");
            var tenantOneUserB = new AgentIdentityContext(
                deploymentId: "deployment-isolation-1",
                tenantId: "tenant-a",
                principalId: "principal-b1",
                userId: "user-84");
            var tenantTwoUserA = new AgentIdentityContext(
                deploymentId: "deployment-isolation-1",
                tenantId: "tenant-b",
                principalId: "principal-a2",
                userId: "user-42");

            var ownerA = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, tenantOneUserA);
            var ownerB = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, tenantOneUserB);
            var ownerTenantA = AgentResourceOwnership.GetOwnerId(AgentResourceScope.Tenant, tenantOneUserA);
            var ownerTenantB = AgentResourceOwnership.GetOwnerId(AgentResourceScope.Tenant, tenantTwoUserA);

            if (string.Equals(ownerA, ownerB, StringComparison.Ordinal))
                throw new InvalidOperationException("Two users in the same tenant must have distinct user ownership keys.");
            if (string.Equals(ownerA, AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, tenantTwoUserA), StringComparison.Ordinal))
                throw new InvalidOperationException("The same user ID in different tenants must not share a user ownership key.");
            if (string.Equals(ownerTenantA, ownerTenantB, StringComparison.Ordinal))
                throw new InvalidOperationException("Different tenants must have distinct tenant ownership keys.");

            await memory.AddAsync(new MemoryEntry
            {
                Scope = MemoryScope.User,
                OwnerId = ownerA,
                Content = "Private user memory A"
            }, CancellationToken.None).ConfigureAwait(true);

            await memory.AddAsync(new MemoryEntry
            {
                Scope = MemoryScope.User,
                OwnerId = ownerB,
                Content = "Private user memory B"
            }, CancellationToken.None).ConfigureAwait(true);

            var visibleToA = await memory.SearchAsync(new MemoryQuery
            {
                Scope = MemoryScope.User,
                OwnerId = ownerA,
                MaxResults = 10
            }, CancellationToken.None).ConfigureAwait(true);

            var visibleToB = await memory.SearchAsync(new MemoryQuery
            {
                Scope = MemoryScope.User,
                OwnerId = ownerB,
                MaxResults = 10
            }, CancellationToken.None).ConfigureAwait(true);

            var visibleWithoutOwner = await memory.SearchAsync(new MemoryQuery
            {
                Scope = MemoryScope.User,
                MaxResults = 10
            }, CancellationToken.None).ConfigureAwait(true);

            if (visibleToA.Count != 1 || visibleToA[0].Content != "Private user memory A")
                throw new InvalidOperationException("User A did not see exactly its own private memory.");
            if (visibleToB.Count != 1 || visibleToB[0].Content != "Private user memory B")
                throw new InvalidOperationException("User B did not see exactly its own private memory.");
            if (visibleWithoutOwner.Count != 2)
                throw new InvalidOperationException("The test store should retain both records when no owner filter is supplied; authorization must prevent such unrestricted queries in a private-resource path.");

            var taskOwner = AgentResourceOwnership.GetOwnerId(AgentResourceScope.Execution, tenantOneUserA, "execution-42");
            var runtimeOwner = AgentResourceOwnership.GetOwnerId(AgentResourceScope.Runtime, tenantOneUserA, "runtime-42");
            if (string.Equals(taskOwner, runtimeOwner, StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime and execution ownership keys must remain scope-distinct even for the same resource ID.");

            Write(
                "IDENTITY ISOLATION",
                "Identity/resource ownership contract test succeeded." + Environment.NewLine +
                "User A owner key: distinct and tenant-bound." + Environment.NewLine +
                "User B owner key: distinct and tenant-bound." + Environment.NewLine +
                "Same user ID across tenants: isolated." + Environment.NewLine +
                "Tenant A vs Tenant B: isolated." + Environment.NewLine +
                "User A private memory visible: yes." + Environment.NewLine +
                "User B private memory visible: yes." + Environment.NewLine +
                "Unfiltered store behavior: both records retained (authorization remains a policy boundary)." + Environment.NewLine +
                "Runtime vs execution resource scopes: distinct.");
        }
    }
}
