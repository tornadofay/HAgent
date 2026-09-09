using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class SkillResourceTests
    {
        [Fact]
        public void SkillDefinition_PreservesVersionedContractAndCloneIsolation()
        {
            var skill = CreateSkill("skill-42", 3, AgentResourceScope.User, "owner-42");
            skill.Inputs.Add(new AiSkillParameterContract { Name = "request", Type = "string", Required = true, Schema = "text" });
            skill.Outputs.Add(new AiSkillParameterContract { Name = "answer", Type = "string", Required = true });
            skill.Preconditions.Add(new AiSkillPrecondition { Id = "ready", Description = "Request is actionable", Expression = "request != empty" });
            skill.Steps.Add(new AiSkillProcedureStep { Id = "inspect", Order = 10, Title = "Inspect", Instruction = "Inspect the request." });
            skill.Dependencies.Add(new AiSkillDependencyReference { Kind = AiSkillDependencyKind.Knowledge, ResourceId = "knowledge-42", Version = 2, Required = true });
            skill.Dependencies.Add(new AiSkillDependencyReference { Kind = AiSkillDependencyKind.Tool, ResourceId = "tool-42", Required = true });
            skill.Constraints["timeout"] = "30s";
            skill.Metadata["domain"] = "support";
            skill.Status = AiSkillLifecycleStatus.Published;
            skill.Validate();

            var clone = skill.Clone();
            clone.Metadata["domain"] = "other";
            clone.Dependencies[0].Version = 9;

            Assert.True(skill.IsAuthoritative);
            Assert.Equal(3, skill.Version);
            Assert.Equal("support", skill.Metadata["domain"]);
            Assert.Equal(2, skill.Dependencies[0].Version);
        }

        [Fact]
        public void SkillReference_RequiresExplicitScopeOwnerAndSupportsVersionPinning()
        {
            var reference = new AiSkillReference { SkillId = "skill-42", Scope = AgentResourceScope.User, OwnerId = "owner-42", Version = 7, Required = true };
            reference.Validate();
            Assert.Throws<ArgumentException>(() => new AiSkillReference { SkillId = "skill-42", Scope = AgentResourceScope.User }.Validate());
        }

        [Fact]
        public void ExecutionSnapshot_CapturesSkillVersionAndClonesDefinition()
        {
            var skill = CreateSkill("skill-42", 5, AgentResourceScope.Global, null);
            skill.Status = AiSkillLifecycleStatus.Published;
            var reference = new AiSkillReference { SkillId = skill.Id, Scope = skill.Scope, Version = skill.Version, Required = true };
            var snapshot = new AiSkillExecutionSnapshot(new[] { new AiSkillBinding { Reference = reference, Definition = skill } });

            skill.Version = 6;
            skill.Name = "mutated";
            snapshot.Bindings[0].Definition.Metadata["snapshot"] = "owned";

            Assert.Equal(5, snapshot.Bindings[0].Definition.Version);
            Assert.Equal("skill-42", snapshot.Bindings[0].Definition.Id);
            Assert.Equal("skill-42", snapshot.Bindings[0].Reference.SkillId);
        }

        [Fact]
        public async Task GovernedSkillResolver_RequiresPublishedAuthorizedVersion()
        {
            var identity = new AgentIdentityContext(deploymentId: "deployment-42", tenantId: "tenant-42", userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var policy = new AiPolicySet { Version = "skill-policy-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-skill-42",
                Name = "Allow skill 42",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "skill.invoke" },
                ResourceTypes = { "skill" },
                ResourceIds = { "skill-42" }
            });
            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("skill", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(new DefaultAiPolicyEngine(policy), AiResourceCapabilitySnapshot.Resolve(capabilities));
            var source = new RecordingSkillSource(CreateSkill("skill-42", 4, AgentResourceScope.User, ownerId));
            source.Skill.Status = AiSkillLifecycleStatus.Published;

            var set = new AiSkillSet { Name = "Support skills" };
            set.References.Add(new AiSkillReference { SkillId = "skill-42", Scope = AgentResourceScope.User, OwnerId = ownerId, Version = 4, Required = true });
            var snapshot = await new AiGovernedSkillResolver(source, governance).ResolveAsync(set, identity, "agent-42", "runtime-42", "execution-42", CancellationToken.None);

            Assert.Single(snapshot.Bindings);
            Assert.Equal(4, snapshot.Bindings[0].Definition.Version);
            Assert.Equal(1, source.CallCount);
        }

        [Fact]
        public async Task GovernedSkillResolver_DeniesUnpublishedOrCrossOwnerSkillsBeforeSourceAccess()
        {
            var identity = new AgentIdentityContext(deploymentId: "deployment-42", tenantId: "tenant-42", userId: "user-42");
            var otherIdentity = new AgentIdentityContext(deploymentId: "deployment-42", tenantId: "tenant-42", userId: "user-99");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, otherIdentity);
            var policy = new AiPolicySet { Version = "skill-policy-43" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-only-user-42",
                Name = "Allow user 42 skill",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "skill.invoke" },
                ResourceTypes = { "skill" },
                ResourceIds = { "skill-43" }
            });
            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("skill", AiResourceCapabilityState.Enabled);
            var source = new RecordingSkillSource(CreateSkill("skill-43", 1, AgentResourceScope.User, ownerId));
            source.Skill.Status = AiSkillLifecycleStatus.Published;
            var resolver = new AiGovernedSkillResolver(source, new AiResourceGovernanceEvaluator(new DefaultAiPolicyEngine(policy), AiResourceCapabilitySnapshot.Resolve(capabilities)));
            var set = new AiSkillSet { Name = "Blocked skills" };
            set.References.Add(new AiSkillReference { SkillId = "skill-43", Scope = AgentResourceScope.User, OwnerId = ownerId, Required = false });

            var result = await resolver.ResolveAsync(set, identity, "agent-42", "runtime-42", "execution-43", CancellationToken.None);
            Assert.Empty(result.Bindings);
            Assert.Equal(0, source.CallCount);
        }

        private static AiSkillDefinition CreateSkill(string id, long version, AgentResourceScope scope, string ownerId)
        {
            var now = DateTime.UtcNow;
            return new AiSkillDefinition
            {
                Id = id,
                Version = version,
                Scope = scope,
                OwnerId = ownerId,
                Name = id,
                Description = "Reusable skill definition for " + id,
                Status = AiSkillLifecycleStatus.Draft,
                CreatedUtc = now,
                UpdatedUtc = now,
                Provenance = new AiSkillProvenance { Source = "tests", SourceId = id }
            };
        }

        private sealed class RecordingSkillSource : IAiSkillDefinitionSource
        {
            public RecordingSkillSource(AiSkillDefinition skill) { Skill = skill; }
            public AiSkillDefinition Skill { get; set; }
            public int CallCount { get; private set; }
            public Task<AiSkillDefinition> GetAsync(AiSkillReference reference, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                return Task.FromResult(Skill == null ? null : Skill.Clone());
            }
        }
    }
}
