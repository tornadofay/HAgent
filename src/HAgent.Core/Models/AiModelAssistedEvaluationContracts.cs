using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Provider-neutral boundary for model-assisted evaluation.
    /// Implementations own model/provider interaction; Core only consumes the bounded judge contract.
    /// </summary>
    public interface IAiEvaluationJudge
    {
        string Id { get; }
        string Version { get; }

        Task<HAgent.Models.AiEvaluationRating> JudgeAsync(
            HAgent.Models.AiEvaluationJudgeRequest request,
            CancellationToken cancellationToken);
    }
}

namespace HAgent.Models
{
    /// <summary>
    /// Detached evaluation snapshot supplied to a model-assisted judge.
    /// The incoming request is cloned so caller mutation cannot affect an active judge call.
    /// </summary>
    public sealed class AiEvaluationJudgeRequest
    {
        public AiEvaluationJudgeRequest(AiEvaluationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            EvaluationRequest = request.Clone();
        }

        public AiEvaluationRequest EvaluationRequest { get; private set; }

        public AiEvaluationJudgeRequest Clone()
        {
            return new AiEvaluationJudgeRequest(EvaluationRequest);
        }

        public void Validate()
        {
            if (EvaluationRequest == null)
                throw new ArgumentNullException(nameof(EvaluationRequest));
            EvaluationRequest.Validate();
        }
    }

    /// <summary>
    /// Provider-neutral model-assisted evaluator. It delegates model-specific grading
    /// to an injected judge and converts the bounded judge rating into normal evaluation evidence.
    /// </summary>
    public sealed class AiModelAssistedEvaluationEvaluator : IAiEvaluator
    {
        private readonly IAiEvaluationJudge _judge;
        private readonly string _id;
        private readonly string _version;
        private readonly string _judgeId;
        private readonly string _judgeVersion;

        public AiModelAssistedEvaluationEvaluator(
            IAiEvaluationJudge judge,
            string id,
            string version = "1")
        {
            _judge = judge ?? throw new ArgumentNullException(nameof(judge));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Evaluator id is required.", nameof(id));
            if (id.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (version != null && version.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(version));
            if (string.IsNullOrWhiteSpace(_judge.Id))
                throw new ArgumentException("Judge id is required.", nameof(judge));
            if (_judge.Id.Length > 256)
                throw new ArgumentException("Judge id must be at most 256 characters.", nameof(judge));
            if (_judge.Version != null && _judge.Version.Length > 128)
                throw new ArgumentException("Judge version must be at most 128 characters.", nameof(judge));

            _id = id;
            _version = version ?? string.Empty;
            _judgeId = _judge.Id;
            _judgeVersion = _judge.Version ?? string.Empty;
        }

        public string Id { get { return _id; } }
        public AiEvaluatorKind Kind { get { return AiEvaluatorKind.ModelAssisted; } }
        public string Version { get { return _version; } }

        public async Task<AiEvaluation> EvaluateAsync(
            AiEvaluationRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            // Keep one detached snapshot owned by the evaluator for the final result.
            // The judge receives its own detached copy so judge-side mutation cannot alter
            // the identity/correlation facts used to construct the result.
            var evaluationRequestSnapshot = request.Clone();
            var judgeRequest = new AiEvaluationJudgeRequest(evaluationRequestSnapshot);
            judgeRequest.Validate();

            var rating = await _judge.JudgeAsync(judgeRequest, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (rating == null)
                throw new InvalidOperationException("The model-assisted evaluation judge returned no rating.");
            rating.Validate();

            var evaluation = new AiEvaluation
            {
                TargetKind = evaluationRequestSnapshot.TargetKind,
                TargetId = evaluationRequestSnapshot.TargetId,
                Outcome = rating.Outcome,
                EvaluatorId = Id,
                EvaluatorKind = Kind,
                EvaluatorVersion = Version,
                Score = rating.Score,
                Confidence = rating.Confidence,
                Label = rating.Label,
                Reason = rating.Reason,
                AgentId = evaluationRequestSnapshot.AgentId,
                RuntimeInstanceId = evaluationRequestSnapshot.RuntimeInstanceId,
                ExecutionId = evaluationRequestSnapshot.ExecutionId,
                GoalId = evaluationRequestSnapshot.GoalId,
                PlanId = evaluationRequestSnapshot.PlanId,
                TraceId = evaluationRequestSnapshot.TraceId
            };

            foreach (var evidence in rating.Evidence ?? new List<AiEvaluationInputReference>())
                evaluation.Evidence.Add(evidence.Clone());
            foreach (var pair in rating.Metadata ?? new Dictionary<string, string>())
                evaluation.Metadata[pair.Key] = pair.Value;

            evaluation.Metadata["evaluation.source"] = "model-assisted";
            evaluation.Metadata["evaluation.authoritative"] = "false";
            evaluation.Metadata["judge.id"] = _judgeId;
            if (!string.IsNullOrEmpty(_judgeVersion))
                evaluation.Metadata["judge.version"] = _judgeVersion;

            evaluation.Validate();
            return evaluation;
        }
    }
}
