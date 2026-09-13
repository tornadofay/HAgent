using System;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddGoalIntentionContractsTab()
        {
            AddApiTab(
                "GOAL & INTENTION CONTRACTS",
                "Run goal/intention contract test",
                "Verifies 0.9591 Slice 1 durable goal and intention contracts: independent identities, status and priority metadata, constraints, provenance, revision timestamps, adoption metadata, and explicit host-versus-agent authority.",
                "Goals and intentions must remain distinct durable concepts; host-supplied goal state must remain identifiable as authoritative host input, while agent-inferred state must carry explicit non-host provenance. Intention status changes must preserve a reason and revision boundary.",
                "Goal and intention contract verification.",
                TestGoalIntentionContracts,
                "Durable goal/intention contract boundary",
                "Pure provider-free contract exercise; no external provider or persistence backend is contacted.");
        }

        private void TestGoalIntentionContracts(string message)
        {
            var created = DateTimeOffset.UtcNow;
            var hostGoal = new AiGoal
            {
                Title = string.IsNullOrWhiteSpace(message) ? "Host-supplied goal" : message,
                Description = "A goal explicitly supplied by the host.",
                Priority = AiGoalPriority.High,
                Status = AiGoalStatus.Active,
                CreatedAt = created,
                UpdatedAt = created,
                Revision = 1L,
                Provenance = new AiGoalProvenance
                {
                    Authority = AiGoalAuthority.HostSupplied,
                    Source = "Example host",
                    Evidence = "Host supplied state must remain distinguishable from agent inference."
                }
            };
            hostGoal.Constraints.Add("Respect host authorization.");
            hostGoal.Validate();

            var inferredGoal = new AiGoal
            {
                Title = "Inferred supporting goal",
                Description = "A goal proposed from execution evidence.",
                Status = AiGoalStatus.Draft,
                CreatedAt = created,
                UpdatedAt = created,
                Provenance = new AiGoalProvenance
                {
                    Authority = AiGoalAuthority.AgentInferred,
                    Source = "Example execution observation",
                    SourceExecutionId = "example-execution-1",
                    Evidence = "Observed execution behavior."
                }
            };
            inferredGoal.Validate();

            var intention = new AiIntention
            {
                GoalId = hostGoal.Id,
                Description = "Adopt an intention for the host-supplied goal.",
                Status = AiIntentionStatus.Adopted,
                Priority = hostGoal.Priority,
                CreatedAt = created,
                UpdatedAt = created.AddSeconds(1),
                AdoptedAt = created.AddSeconds(1),
                Revision = 1L,
                Provenance = hostGoal.Provenance.Clone()
            };
            intention.Constraints.Add("Do not exceed the goal constraints.");
            intention.Validate();

            var statusChange = new AiIntentionStatusChange(
                intention.Id,
                2L,
                AiIntentionStatus.Adopted,
                AiIntentionStatus.Suspended,
                "Host requested temporary suspension.",
                "example-host-review",
                created.AddSeconds(2),
                AiGoalAuthority.HostSupplied);

            var clonedHostGoal = hostGoal.Clone();
            if (hostGoal.Id == intention.Id || hostGoal.Id == inferredGoal.Id)
                throw new InvalidOperationException("Goal and intention identities must remain distinct.");
            if (hostGoal.Provenance.Authority != AiGoalAuthority.HostSupplied)
                throw new InvalidOperationException("Host-supplied goal authority was not preserved.");
            if (inferredGoal.Provenance.Authority != AiGoalAuthority.AgentInferred)
                throw new InvalidOperationException("Agent-inferred goal authority was incorrectly treated as host authority.");
            if (clonedHostGoal.Provenance.Authority != AiGoalAuthority.HostSupplied)
                throw new InvalidOperationException("Goal cloning did not preserve provenance authority.");
            if (statusChange.Revision != 2L || string.IsNullOrWhiteSpace(statusChange.Reason))
                throw new InvalidOperationException("Intention status change did not preserve its revision and reason.");

            Write("GOAL & INTENTION CONTRACTS",
                "Contract test succeeded." + Environment.NewLine +
                "Host goal ID: " + hostGoal.Id + Environment.NewLine +
                "Inferred goal ID: " + inferredGoal.Id + Environment.NewLine +
                "Intention ID: " + intention.Id + Environment.NewLine +
                "Goal authority: " + hostGoal.Provenance.Authority + Environment.NewLine +
                "Inferred authority: " + inferredGoal.Provenance.Authority + Environment.NewLine +
                "Intention status: " + intention.Status + Environment.NewLine +
                "Intention revision: " + intention.Revision + Environment.NewLine +
                "Status change: " + statusChange.PreviousStatus + " -> " + statusChange.NewStatus + Environment.NewLine +
                "Status-change reason preserved: yes");
        }
    }
}
