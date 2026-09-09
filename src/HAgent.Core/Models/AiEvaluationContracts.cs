using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HAgent.Models
{
    public enum AiEvaluationTargetKind
    {
        Execution,
        Response,
        ToolOutcome,
        GoalOutcome,
        PlanOutcome,
        MemoryKnowledgeUsefulness,
        LearningCandidate
    }

    public enum AiEvaluationOutcome
    {
        NotEvaluated,
        Passed,
        Failed,
        Inconclusive,
        NeedsReview
    }

    public enum AiEvaluatorKind
    {
        Deterministic,
        Human,
        Application,
        ModelAssisted
    }

    public sealed class AiEvaluationInputReference
    {
        public AiEvaluationInputReference()
        {
            Kind = string.Empty;
            Id = string.Empty;
            Role = string.Empty;
        }

        public string Kind { get; set; }
        public string Id { get; set; }
        public string Role { get; set; }

        public AiEvaluationInputReference Clone()
        {
            return new AiEvaluationInputReference
            {
                Kind = Kind,
                Id = Id,
                Role = Role
            };
        }

        public void Validate()
        {
            Require(Kind, nameof(Kind), 128);
            Require(Id, nameof(Id), 512);
            if (Role != null && Role.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Role));
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationRequest
    {
        public AiEvaluationRequest()
        {
            Id = Guid.NewGuid().ToString("N");
            TargetKind = AiEvaluationTargetKind.Execution;
            TargetId = string.Empty;
            AgentId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            GoalId = string.Empty;
            PlanId = string.Empty;
            TraceId = string.Empty;
            Inputs = new List<AiEvaluationInputReference>();
            Criteria = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            RequestedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public AiEvaluationTargetKind TargetKind { get; set; }
        public string TargetId { get; set; }
        public string AgentId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string GoalId { get; set; }
        public string PlanId { get; set; }
        public string TraceId { get; set; }
        [JsonInclude]
        public IList<AiEvaluationInputReference> Inputs { get; private set; }
        [JsonInclude]
        public IDictionary<string, string> Criteria { get; private set; }
        public DateTimeOffset RequestedAt { get; private set; }

        public AiEvaluationRequest Clone()
        {
            var clone = new AiEvaluationRequest
            {
                Id = Id,
                TargetKind = TargetKind,
                TargetId = TargetId,
                AgentId = AgentId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                GoalId = GoalId,
                PlanId = PlanId,
                TraceId = TraceId
            };
            foreach (var input in Inputs ?? new List<AiEvaluationInputReference>())
                if (input != null) clone.Inputs.Add(input.Clone());
            foreach (var pair in Criteria ?? new Dictionary<string, string>())
                clone.Criteria[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            Require(TargetId, nameof(TargetId), 512);
            Require(AgentId, nameof(AgentId), 512, false);
            Require(RuntimeInstanceId, nameof(RuntimeInstanceId), 512, false);
            Require(ExecutionId, nameof(ExecutionId), 512, false);
            Require(GoalId, nameof(GoalId), 512, false);
            Require(PlanId, nameof(PlanId), 512, false);
            Require(TraceId, nameof(TraceId), 128, false);
            if (Inputs == null) throw new ArgumentNullException(nameof(Inputs));
            if (Inputs.Count > 32) throw new ArgumentOutOfRangeException(nameof(Inputs));
            foreach (var input in Inputs)
            {
                if (input == null) throw new ArgumentException("Evaluation inputs cannot contain null entries.", nameof(Inputs));
                input.Validate();
            }
            if (Criteria == null) throw new ArgumentNullException(nameof(Criteria));
            if (Criteria.Count > 32) throw new ArgumentOutOfRangeException(nameof(Criteria));
            foreach (var pair in Criteria)
            {
                Require(pair.Key, nameof(Criteria), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Criteria));
            }
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluation
    {
        public AiEvaluation()
        {
            Id = Guid.NewGuid().ToString("N");
            TargetKind = AiEvaluationTargetKind.Execution;
            TargetId = string.Empty;
            Outcome = AiEvaluationOutcome.NotEvaluated;
            EvaluatorId = string.Empty;
            EvaluatorKind = AiEvaluatorKind.Deterministic;
            EvaluatorVersion = string.Empty;
            Label = string.Empty;
            Reason = string.Empty;
            Evidence = new List<AiEvaluationInputReference>();
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Confidence = null;
            EvaluatedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public AiEvaluationTargetKind TargetKind { get; set; }
        public string TargetId { get; set; }
        public AiEvaluationOutcome Outcome { get; set; }
        public string EvaluatorId { get; set; }
        public AiEvaluatorKind EvaluatorKind { get; set; }
        public string EvaluatorVersion { get; set; }
        public decimal? Score { get; set; }
        public double? Confidence { get; set; }
        public string Label { get; set; }
        public string Reason { get; set; }
        public string AgentId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string GoalId { get; set; }
        public string PlanId { get; set; }
        public string TraceId { get; set; }
        public DateTimeOffset EvaluatedAt { get; private set; }
        [JsonInclude]
        public IList<AiEvaluationInputReference> Evidence { get; private set; }
        [JsonInclude]
        public IDictionary<string, string> Metadata { get; private set; }

        public AiEvaluation Clone()
        {
            var clone = new AiEvaluation
            {
                Id = Id,
                TargetKind = TargetKind,
                TargetId = TargetId,
                Outcome = Outcome,
                EvaluatorId = EvaluatorId,
                EvaluatorKind = EvaluatorKind,
                EvaluatorVersion = EvaluatorVersion,
                Score = Score,
                Confidence = Confidence,
                Label = Label,
                Reason = Reason,
                AgentId = AgentId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                GoalId = GoalId,
                PlanId = PlanId,
                TraceId = TraceId
            };
            foreach (var item in Evidence ?? new List<AiEvaluationInputReference>())
                if (item != null) clone.Evidence.Add(item.Clone());
            foreach (var pair in Metadata ?? new Dictionary<string, string>())
                clone.Metadata[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            Require(TargetId, nameof(TargetId), 512);
            Require(EvaluatorId, nameof(EvaluatorId), 256);
            Require(EvaluatorVersion, nameof(EvaluatorVersion), 128, false);
            Require(Label, nameof(Label), 256, false);
            Require(Reason, nameof(Reason), 2048, false);
            Require(AgentId, nameof(AgentId), 512, false);
            Require(RuntimeInstanceId, nameof(RuntimeInstanceId), 512, false);
            Require(ExecutionId, nameof(ExecutionId), 512, false);
            Require(GoalId, nameof(GoalId), 512, false);
            Require(PlanId, nameof(PlanId), 512, false);
            Require(TraceId, nameof(TraceId), 128, false);
            if (Score.HasValue && (Score.Value < 0m || Score.Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(Score));
            if (Confidence.HasValue && (Confidence.Value < 0d || Confidence.Value > 1d || double.IsNaN(Confidence.Value) || double.IsInfinity(Confidence.Value)))
                throw new ArgumentOutOfRangeException(nameof(Confidence));
            if (Evidence == null) throw new ArgumentNullException(nameof(Evidence));
            if (Evidence.Count > 32) throw new ArgumentOutOfRangeException(nameof(Evidence));
            foreach (var item in Evidence)
            {
                if (item == null) throw new ArgumentException("Evaluation evidence cannot contain null entries.", nameof(Evidence));
                item.Validate();
            }
            if (Metadata == null) throw new ArgumentNullException(nameof(Metadata));
            if (Metadata.Count > 32) throw new ArgumentOutOfRangeException(nameof(Metadata));
            foreach (var pair in Metadata)
            {
                Require(pair.Key, nameof(Metadata), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Metadata));
            }
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
