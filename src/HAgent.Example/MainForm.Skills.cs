using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddSkillsTab()
        {
            AddApiTab(
                "Skills",
                "Run skill test",
                "Creates a reusable versioned Skill definition, references it from a skill set, snapshots the published version for execution, and gates resolution through the generic resource-governance boundary.",
                "The execution snapshot must retain the exact published skill version while an unauthorized cross-owner skill must be rejected before the definition source is accessed.",
                "Resolve the governed skill.",
                TestSkillsAsync,
                "Skill boundary",
                "Skill definitions describe reusable procedure and contracts. Executable handlers remain outside the persisted provider-neutral definition and are not serialized here.");
        }

        private async Task TestSkillsAsync(string unused)
        {
            var identity = new AgentIdentityContext(deploymentId: "deployment-42", tenantId: "tenant-42", userId: "user-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var skill = new AiSkillDefinition
            {
                Id = "skill-retention-42",
                Version = 4,
                Scope = AgentResourceScope.User,
                OwnerId = ownerId,
                Name = "Retention review",
                Description = "Review a retention request and return the applicable disposition.",
                Status = AiSkillLifecycleStatus.Published,
                Provenance = new AiSkillProvenance { Source = "administrator", SourceId = "skill-retention-42" }
            };
            skill.Inputs.Add(new AiSkillParameterContract { Name = "request", Type = "string", Required = true, Description = "Retention request." });
            skill.Outputs.Add(new AiSkillParameterContract { Name = "disposition", Type = "string", Required = true });
            skill.Preconditions.Add(new AiSkillPrecondition { Id = "request-valid", Description = "The request contains enough information for review." });
            skill.Steps.Add(new AiSkillProcedureStep { Id = "review", Order = 10, Title = "Review", Instruction = "Review the retention request against the applicable knowledge." });
            skill.Dependencies.Add(new AiSkillDependencyReference { Kind = AiSkillDependencyKind.Knowledge, ResourceId = "knowledge-retention-42", Version = 2, Required = true });
            skill.Dependencies.Add(new AiSkillDependencyReference { Kind = AiSkillDependencyKind.Tool, ResourceId = "tool-policy-42", Required = true });
            skill.Constraints["sideEffects"] = "none";
            skill.Validate();

            var policy = new AiPolicySet { Version = "skill-example-policy-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-skill-retention",
                Name = "Allow retention skill",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "skill.invoke" },
                ResourceTypes = { "skill" },
                ResourceIds = { skill.Id }
            });
            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("skill", AiResourceCapabilityState.Enabled);
            var governance = new AiResourceGovernanceEvaluator(new DefaultAiPolicyEngine(policy), AiResourceCapabilitySnapshot.Resolve(capabilities));
            var source = new ExampleSkillSource(skill);
            var set = new AiSkillSet { Id = "support-skills", Name = "Support skills" };
            set.References.Add(new AiSkillReference { SkillId = skill.Id, Version = skill.Version, Scope = skill.Scope, OwnerId = skill.OwnerId, Required = true });

            var executionSkills = await new AiGovernedSkillResolver(source, governance).ResolveAsync(set, identity, "agent-42", "runtime-42", "execution-42", CancellationToken.None);
            if (executionSkills.Bindings.Count != 1 || executionSkills.Bindings[0].Definition.Version != 4)
                throw new InvalidOperationException("Governed skill resolution returned an unexpected binding.");

            skill.Version = 5;
            skill.Name = "Mutated after snapshot";
            if (executionSkills.Bindings[0].Definition.Version != 4 || executionSkills.Bindings[0].Definition.Name != "Retention review")
                throw new InvalidOperationException("Skill execution snapshot did not retain the captured version/state.");

            var forbidden = new AiSkillSet { Id = "forbidden-skills", Name = "Forbidden skills" };
            forbidden.References.Add(new AiSkillReference { SkillId = skill.Id, Version = 4, Scope = AgentResourceScope.User, OwnerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, new AgentIdentityContext(deploymentId: "deployment-42", tenantId: "tenant-42", userId: "user-99")), Required = false });
            var blockedSource = new ExampleSkillSource(skill);
            var blocked = await new AiGovernedSkillResolver(blockedSource, governance).ResolveAsync(forbidden, identity, "agent-42", "runtime-42", "execution-43", CancellationToken.None);
            if (blocked.Bindings.Count != 0 || blockedSource.CallCount != 0)
                throw new InvalidOperationException("Unauthorized skill resolution crossed the governance boundary.");

            Write(
                "SKILLS",
                "Skills succeeded." + Environment.NewLine +
                "Reusable definition: verified." + Environment.NewLine +
                "Published version captured: 4." + Environment.NewLine +
                "Input/output contracts: verified." + Environment.NewLine +
                "Preconditions/procedure/dependencies/constraints: verified." + Environment.NewLine +
                "Execution snapshot isolation: verified." + Environment.NewLine +
                "Cross-owner governance denial before source access: verified." + Environment.NewLine +
                "Executable handler is not part of the definition contract.");
        }

        private sealed class ExampleSkillSource : IAiSkillDefinitionSource
        {
            private readonly AiSkillDefinition _skill;
            public ExampleSkillSource(AiSkillDefinition skill) { _skill = skill; }
            public int CallCount { get; private set; }
            public Task<AiSkillDefinition> GetAsync(AiSkillReference reference, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                return Task.FromResult(_skill == null ? null : _skill.Clone());
            }
        }
    }
}
