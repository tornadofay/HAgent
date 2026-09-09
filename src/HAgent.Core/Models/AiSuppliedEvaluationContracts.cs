using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public sealed class AiEvaluationRating
    {
        public AiEvaluationRating()
        {
            Outcome = AiEvaluationOutcome.NeedsReview;
            Label = string.Empty;
            Reason = string.Empty;
            Evidence = new List<AiEvaluationInputReference>();
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public AiEvaluationOutcome Outcome { get; set; }
        public decimal? Score { get; set; }
        public double? Confidence { get; set; }
        public string Label { get; set; }
        public string Reason { get; set; }
        [JsonInclude]
        public IList<AiEvaluationInputReference> Evidence { get; private set; }
        [JsonInclude]
        public IDictionary<string, string> Metadata { get; private set; }

        public AiEvaluationRating Clone()
        {
            var clone = new AiEvaluationRating
            {
                Outcome = Outcome,
                Score = Score,
                Confidence = Confidence,
                Label = Label,
                Reason = Reason
            };
            foreach (var evidence in Evidence ?? new List<AiEvaluationInputReference>())
                if (evidence != null) clone.Evidence.Add(evidence.Clone());
            foreach (var pair in Metadata ?? new Dictionary<string, string>())
                clone.Metadata[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            if (Score.HasValue && (Score.Value < 0m || Score.Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(Score));
            if (Confidence.HasValue && (Confidence.Value < 0d || Confidence.Value > 1d || double.IsNaN(Confidence.Value) || double.IsInfinity(Confidence.Value)))
                throw new ArgumentOutOfRangeException(nameof(Confidence));
            Require(Label, nameof(Label), 256, false);
            Require(Reason, nameof(Reason), 2048, false);

            if (Evidence == null) throw new ArgumentNullException(nameof(Evidence));
            if (Evidence.Count > 32) throw new ArgumentOutOfRangeException(nameof(Evidence));
            foreach (var evidence in Evidence)
            {
                if (evidence == null) throw new ArgumentException("Evaluation evidence cannot contain null entries.", nameof(Evidence));
                evidence.Validate();
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

    public sealed class AiSuppliedRatingEvaluator : IAiEvaluator
    {
        private readonly AiEvaluationRating _rating;
        private readonly string _id;
        private readonly AiEvaluatorKind _kind;
        private readonly string _version;

        public AiSuppliedRatingEvaluator(AiEvaluationRating rating, AiEvaluatorKind kind, string id, string version = "1")
        {
            if (rating == null) throw new ArgumentNullException(nameof(rating));
            if (kind != AiEvaluatorKind.Human && kind != AiEvaluatorKind.Application)
                throw new ArgumentException("Supplied ratings must use Human or Application evaluator kind.", nameof(kind));
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Evaluator id is required.", nameof(id));
            if (id.Length > 256) throw new ArgumentOutOfRangeException(nameof(id));
            if (version != null && version.Length > 128) throw new ArgumentOutOfRangeException(nameof(version));

            rating.Validate();
            _rating = rating.Clone();
            _kind = kind;
            _id = id;
            _version = version ?? string.Empty;
        }

        public string Id { get { return _id; } }
        public AiEvaluatorKind Kind { get { return _kind; } }
        public string Version { get { return _version; } }

        public Task<AiEvaluation> EvaluateAsync(AiEvaluationRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var evaluation = new AiEvaluation
            {
                TargetKind = request.TargetKind,
                TargetId = request.TargetId,
                Outcome = _rating.Outcome,
                EvaluatorId = Id,
                EvaluatorKind = Kind,
                EvaluatorVersion = Version,
                Score = _rating.Score,
                Confidence = _rating.Confidence,
                Label = _rating.Label,
                Reason = _rating.Reason,
                AgentId = request.AgentId,
                RuntimeInstanceId = request.RuntimeInstanceId,
                ExecutionId = request.ExecutionId,
                GoalId = request.GoalId,
                PlanId = request.PlanId,
                TraceId = request.TraceId
            };

            foreach (var evidence in _rating.Evidence)
                evaluation.Evidence.Add(evidence.Clone());
            foreach (var pair in _rating.Metadata)
                evaluation.Metadata[pair.Key] = pair.Value;
            evaluation.Metadata["rating.source"] = Kind.ToString();
            evaluation.Validate();
            return Task.FromResult(evaluation);
        }
    }
}
