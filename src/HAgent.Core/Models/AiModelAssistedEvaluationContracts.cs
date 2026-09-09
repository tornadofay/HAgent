using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

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

            // The judge receives a detached request snapshot. It may inspect or mutate
            // the snapshot internally without changing the caller-owned request.
            var judgeRequest = new AiEvaluationJudgeRequest(request);
            judgeRequest.Validate();

            var rating = await _judge.JudgeAsync(judgeRequest, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (rating == null)
                throw new InvalidOperationException("The model-assisted evaluation judge returned no rating.");
            rating.Validate();

            var evaluation = new AiEvaluation
            {
                TargetKind = request.TargetKind,
                TargetId = request.TargetId,
                Outcome = rating.Outcome,
                EvaluatorId = Id,
                EvaluatorKind = Kind,
                EvaluatorVersion = Version,
                Score = rating.Score,
                Confidence = rating.Confidence,
                Label = rating.Label,
                Reason = rating.Reason,
                AgentId = request.AgentId,
                RuntimeInstanceId = request.RuntimeInstanceId,
                ExecutionId = request.ExecutionId,
                GoalId = request.GoalId,
                PlanId = request.PlanId,
                TraceId = request.TraceId
            };

            foreach (var evidence in rating.Evidence ?? new List<AiEvaluationInputReference>())
                evaluation.Evidence.Add(evidence.Clone());
            foreach (var pair in rating.Metadata ?? new Dictionary<string, string>())
                evaluation.Metadata[pair.Key] = pair.Value;

            evaluation.Metadata["evaluation.source"] = "model-assisted";
            evaluation.Metadata["evaluation.authoritative"] = "false";
            evaluation.Metadata["judge.id"] = _judge.Id;
            if (!string.IsNullOrEmpty(_judge.Version))
                evaluation.Metadata["judge.version"] = _judge.Version;

            evaluation.Validate();
            return evaluation;
        }
    }
}
