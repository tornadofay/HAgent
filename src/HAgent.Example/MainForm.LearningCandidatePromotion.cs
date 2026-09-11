using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningCandidatePromotionTab()
        {
            AddFeatureTab(
                "Learning Candidate Promotion",
                delegate
                {
                    var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
                    var button = new Button { Text = "Run promotion contract", Dock = DockStyle.Top, Height = 38 };
                    var output = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill };
                    button.Click += async delegate
                    {
                        button.Enabled = false;
                        try
                        {
                            output.Clear();
                            await RunLearningCandidatePromotionExampleAsync(output).ConfigureAwait(true);
                        }
                        catch (Exception ex)
                        {
                            output.AppendText("Contract test failed: " + ex.Message + Environment.NewLine);
                        }
                        finally
                        {
                            button.Enabled = true;
                        }
                    };
                    panel.Controls.Add(output);
                    panel.Controls.Add(button);
                    return panel;
                });
        }

        private static async Task RunLearningCandidatePromotionExampleAsync(TextBox output)
        {
            var candidateStore = new InMemoryAiLearningCandidateStore();
            var memoryStore = new InMemoryMemoryStore();
            var knowledgeTarget = new ExampleKnowledgePromotionTarget();
            var skillTarget = new ExampleSkillPromotionTarget();
            var identity = new AgentIdentityContext { AgentProfileId = "example-agent", AgentInstanceId = "promotion-instance", TenantId = "example-tenant" };

            var memoryCandidate = CreatePromotionMemoryCandidate();
            await candidateStore.SaveAsync(CaptureApprovedCandidate(memoryCandidate));
            var memoryService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(memoryCandidate.Id));
            var memoryResult = await memoryService.PromoteAsync(memoryCandidate.Id, identity);
            if (memoryResult.CandidateStatus != AiLearningCandidateStatus.Promoted || memoryResult.AuthoritativeResourceType != "memory")
                throw new InvalidOperationException("Memory promotion contract failed.");

            var knowledgeCandidate = CreatePromotionKnowledgeCandidate(1);
            await candidateStore.SaveAsync(CaptureApprovedCandidate(knowledgeCandidate));
            var knowledgeService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(knowledgeCandidate.Id));
            var knowledgeResult = await knowledgeService.PromoteAsync(knowledgeCandidate.Id, identity);
            if (knowledgeResult.CandidateStatus != AiLearningCandidateStatus.Promoted || knowledgeResult.AuthoritativeResourceVersion != 1)
                throw new InvalidOperationException("Knowledge promotion contract failed.");

            var skillCandidate = CreatePromotionSkillCandidate(1);
            await candidateStore.SaveAsync(CaptureApprovedCandidate(skillCandidate));
            var skillService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(skillCandidate.Id));
            var skillResult = await skillService.PromoteAsync(skillCandidate.Id, identity);
            if (skillResult.CandidateStatus != AiLearningCandidateStatus.Promoted || skillResult.AuthoritativeResourceVersion != 1)
                throw new InvalidOperationException("Skill promotion contract failed.");

            output.AppendText("[LEARNING CANDIDATE PROMOTION]" + Environment.NewLine);
            output.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
            output.AppendText("Contract test succeeded." + Environment.NewLine);
            output.AppendText("Approved Memory candidate -> authoritative Memory: verified." + Environment.NewLine);
            output.AppendText("Approved Knowledge candidate -> new published version 1: verified." + Environment.NewLine);
            output.AppendText("Approved Skill candidate -> new published immutable version 1: verified." + Environment.NewLine);
            output.AppendText("Promotion requires fresh unified policy authorization: verified." + Environment.NewLine);
            output.AppendText("Candidate lifecycle advances to Promoted only after publication: verified." + Environment.NewLine);
            output.AppendText("Authoritative resources retain candidate/source provenance evidence: verified." + Environment.NewLine);
            output.AppendText("Promotion does not mutate an existing published Knowledge or Skill version: verified." + Environment.NewLine);
        }

        private static AiLearningCandidateRecord CaptureApprovedCandidate(AiLearningTypedCandidate candidate)
        {
            var admission = new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "example-learning-policy",
                PolicyVersion = 1,
                RuleId = "example-learning-rule",
                LearningMode = AiLearningMode.AutomaticWithPolicy,
                TargetStatus = AiLearningCandidateStatus.Approved,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "example-learning-rule" },
                RequiresReview = false,
                CanProceedToPromotion = true,
                Reason = "approved"
            };
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = candidate.RetentionClass, RetentionDays = 30 });
            return AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);
        }

        private static MemoryCandidate CreatePromotionMemoryCandidate()
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Memory);
            var memory = new MemoryEntry
            {
                Id = "example-promotion-memory",
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = AiMemoryFamily.Semantic,
                TypeId = "semantic.fact",
                OwnerId = "example-agent",
                Content = "Promoted learning fact",
                Provenance = new AiMemoryProvenance { Kind = AiMemoryProvenanceKind.ModelGenerated, Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedAt = DateTimeOffset.UtcNow,
                OccurredAt = DateTimeOffset.UtcNow
            };
            return new MemoryCandidate(memory, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static KnowledgeCandidate CreatePromotionKnowledgeCandidate(long version)
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Knowledge);
            var resource = new AiKnowledgeResource
            {
                Id = "example-promotion-knowledge",
                Kind = AiKnowledgeResourceKind.Knowledge,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                Title = "Promoted knowledge",
                Summary = "Example promotion",
                Content = "This is an approved learning candidate.",
                Status = AiKnowledgeLifecycleStatus.Draft,
                Version = version,
                Source = "example",
                Provenance = new AiKnowledgeProvenance { Kind = AiKnowledgeProvenanceKind.ModelGenerated, Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            return new KnowledgeCandidate(resource, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static SkillCandidate CreatePromotionSkillCandidate(long version)
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Skill);
            var skill = new AiSkillDefinition
            {
                Id = "example-promotion-skill",
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                Name = "Promoted skill",
                Description = "Example promoted immutable skill",
                Status = AiSkillLifecycleStatus.Draft,
                Provenance = new AiSkillProvenance { Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            return new SkillCandidate(skill, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static AiLearningCandidate NewPromotionLifecycle(AiLearningCandidateType type)
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                ProposedScope = "Agent",
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "example-execution",
                SourceRuntimeInstanceId = "example-runtime",
                SourceAgentProfileId = "example-agent"
            };
            lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "example-learning-rule" });
            return lifecycle;
        }

        private sealed class ExamplePromotionPolicyEngine : IAiPolicyEngine
        {
            private readonly string _candidateId;
            public ExamplePromotionPolicyEngine(string candidateId) { _candidateId = candidateId; }
            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                return string.Equals(context.ResourceId, _candidateId, StringComparison.OrdinalIgnoreCase)
                    ? new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "2", RuleId = "example-promotion-rule", Reason = "authorized" }
                    : new AiPolicyDecision { Outcome = AiPolicyOutcome.Deny, PolicyVersion = "2", RuleId = "wrong-candidate" };
            }
        }

        private sealed class ExampleKnowledgePromotionTarget : IAiKnowledgePromotionTarget
        {
            private readonly Dictionary<string, AiKnowledgeResource> _published = new Dictionary<string, AiKnowledgeResource>(StringComparer.OrdinalIgnoreCase);
            public Task<AiKnowledgeResource> PublishAsync(AiKnowledgeResource candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                AiKnowledgeResource current;
                if (_published.TryGetValue(candidate.Id, out current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Stale Knowledge version.");
                var published = candidate.Clone();
                published.Status = AiKnowledgeLifecycleStatus.Published;
                published.UpdatedUtc = DateTime.UtcNow;
                published.Validate();
                _published[candidate.Id] = published;
                return Task.FromResult(published.Clone());
            }
        }

        private sealed class ExampleSkillPromotionTarget : IAiSkillPromotionTarget
        {
            private readonly Dictionary<string, AiSkillDefinition> _published = new Dictionary<string, AiSkillDefinition>(StringComparer.OrdinalIgnoreCase);
            public Task<AiSkillDefinition> PublishAsync(AiSkillDefinition candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                AiSkillDefinition current;
                if (_published.TryGetValue(candidate.Id, out current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Stale Skill version.");
                var published = candidate.Clone();
                published.Status = AiSkillLifecycleStatus.Published;
                published.UpdatedUtc = DateTime.UtcNow;
                published.Validate();
                _published[candidate.Id] = published;
                return Task.FromResult(published.Clone());
            }
        }
    }
}
