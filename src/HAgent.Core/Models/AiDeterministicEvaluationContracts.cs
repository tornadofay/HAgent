using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public enum AiEvaluationObservationKind
    {
        Boolean,
        Decimal,
        Text
    }

    public enum AiDeterministicEvaluationRuleKind
    {
        SchemaValidity,
        RequiredFields,
        PolicyCompliance,
        ToolSuccess,
        Latency,
        Cost,
        TaskCompletion
    }

    public sealed class AiEvaluationObservation
    {
        public AiEvaluationObservation()
        {
            Kind = string.Empty;
            Id = string.Empty;
            ValueKind = AiEvaluationObservationKind.Text;
            Unit = string.Empty;
            TextValue = string.Empty;
        }

        public string Kind { get; set; }
        public string Id { get; set; }
        public AiEvaluationObservationKind ValueKind { get; set; }
        public bool? BooleanValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public string TextValue { get; set; }
        public string Unit { get; set; }

        public AiEvaluationObservation Clone()
        {
            return new AiEvaluationObservation
            {
                Kind = Kind,
                Id = Id,
                ValueKind = ValueKind,
                BooleanValue = BooleanValue,
                DecimalValue = DecimalValue,
                TextValue = TextValue,
                Unit = Unit
            };
        }

        public string GetDisplayValue()
        {
            switch (ValueKind)
            {
                case AiEvaluationObservationKind.Boolean:
                    return BooleanValue.HasValue ? BooleanValue.Value.ToString().ToLowerInvariant() : string.Empty;
                case AiEvaluationObservationKind.Decimal:
                    return DecimalValue.HasValue ? DecimalValue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                default:
                    return TextValue ?? string.Empty;
            }
        }

        public void Validate()
        {
            Require(Kind, nameof(Kind), 128);
            Require(Id, nameof(Id), 512);
            if (!string.IsNullOrEmpty(Unit) && Unit.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(Unit));

            switch (ValueKind)
            {
                case AiEvaluationObservationKind.Boolean:
                    if (!BooleanValue.HasValue || DecimalValue.HasValue || (TextValue != null && TextValue.Length > 0))
                        throw new ArgumentException("Boolean observations must contain exactly one boolean value.", nameof(BooleanValue));
                    break;
                case AiEvaluationObservationKind.Decimal:
                    if (!DecimalValue.HasValue || BooleanValue.HasValue || (TextValue != null && TextValue.Length > 0))
                        throw new ArgumentException("Decimal observations must contain exactly one decimal value.", nameof(DecimalValue));
                    break;
                case AiEvaluationObservationKind.Text:
                    if (BooleanValue.HasValue || DecimalValue.HasValue || TextValue == null)
                        throw new ArgumentException("Text observations must contain exactly one text value.", nameof(TextValue));
                    if (TextValue.Length > 2048)
                        throw new ArgumentOutOfRangeException(nameof(TextValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ValueKind));
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiDeterministicEvaluationEvaluator : IAiEvaluator
    {
        private const string MaxThresholdCriterion = "max";
        private readonly string _id;
        private readonly string _version;

        public AiDeterministicEvaluationEvaluator(AiDeterministicEvaluationRuleKind ruleKind, string id = null, string version = "1")
        {
            RuleKind = ruleKind;
            _id = string.IsNullOrWhiteSpace(id) ? "deterministic." + ruleKind.ToString() : id;
            _version = version ?? string.Empty;
            if (_id.Length > 256) throw new ArgumentOutOfRangeException(nameof(id));
            if (_version.Length > 128) throw new ArgumentOutOfRangeException(nameof(version));
        }

        public string Id { get { return _id; } }
        public AiEvaluatorKind Kind { get { return AiEvaluatorKind.Deterministic; } }
        public string Version { get { return _version; } }
        public AiDeterministicEvaluationRuleKind RuleKind { get; private set; }

        public Task<AiEvaluation> EvaluateAsync(AiEvaluationRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            AiEvaluationObservation observation = FindObservation(request, GetObservationKind(RuleKind));
            var result = CreateBaseEvaluation(request);
            result.Metadata["rule"] = RuleKind.ToString();

            if (observation == null)
            {
                result.Outcome = AiEvaluationOutcome.Inconclusive;
                result.Label = "missing-signal";
                result.Reason = "The deterministic evaluator did not receive the required bounded observation.";
                result.Confidence = 1d;
                return Task.FromResult(result);
            }

            result.Evidence.Add(new AiEvaluationInputReference
            {
                Kind = "observation",
                Id = observation.Id,
                Role = observation.Kind
            });
            if (observation.ValueKind != AiEvaluationObservationKind.Text)
                result.Metadata["observed"] = observation.GetDisplayValue();
            if (!string.IsNullOrEmpty(observation.Unit))
                result.Metadata["unit"] = observation.Unit;

            bool? passed = null;
            switch (RuleKind)
            {
                case AiDeterministicEvaluationRuleKind.SchemaValidity:
                case AiDeterministicEvaluationRuleKind.RequiredFields:
                case AiDeterministicEvaluationRuleKind.PolicyCompliance:
                case AiDeterministicEvaluationRuleKind.ToolSuccess:
                case AiDeterministicEvaluationRuleKind.TaskCompletion:
                    if (observation.ValueKind != AiEvaluationObservationKind.Boolean)
                        return Inconclusive(result, "The deterministic rule requires a boolean observation.");
                    passed = observation.BooleanValue;
                    break;
                case AiDeterministicEvaluationRuleKind.Latency:
                case AiDeterministicEvaluationRuleKind.Cost:
                    if (observation.ValueKind != AiEvaluationObservationKind.Decimal)
                        return Inconclusive(result, "The deterministic rule requires a decimal observation.");
                    passed = EvaluateThreshold(request, observation, result);
                    if (!passed.HasValue)
                        return Task.FromResult(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(RuleKind));
            }

            result.Outcome = passed.Value ? AiEvaluationOutcome.Passed : AiEvaluationOutcome.Failed;
            result.Score = passed.Value ? 1m : 0m;
            result.Confidence = 1d;
            result.Label = passed.Value ? "pass" : "fail";
            result.Reason = passed.Value
                ? "The deterministic observation satisfied the evaluation rule."
                : "The deterministic observation did not satisfy the evaluation rule.";
            return Task.FromResult(result);
        }

        private AiEvaluation CreateBaseEvaluation(AiEvaluationRequest request)
        {
            return new AiEvaluation
            {
                TargetKind = request.TargetKind,
                TargetId = request.TargetId,
                EvaluatorId = Id,
                EvaluatorKind = Kind,
                EvaluatorVersion = Version,
                AgentId = request.AgentId,
                RuntimeInstanceId = request.RuntimeInstanceId,
                ExecutionId = request.ExecutionId,
                GoalId = request.GoalId,
                PlanId = request.PlanId,
                TraceId = request.TraceId
            };
        }

        private static AiEvaluationObservation FindObservation(AiEvaluationRequest request, string kind)
        {
            AiEvaluationObservation match = null;
            foreach (var observation in request.Observations)
            {
                if (!string.Equals(observation.Kind, kind, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (match != null)
                    throw new ArgumentException("An evaluation request cannot contain multiple observations for the same deterministic signal.", nameof(request));
                match = observation;
            }
            return match;
        }

        private static string GetObservationKind(AiDeterministicEvaluationRuleKind ruleKind)
        {
            switch (ruleKind)
            {
                case AiDeterministicEvaluationRuleKind.SchemaValidity: return "schema.valid";
                case AiDeterministicEvaluationRuleKind.RequiredFields: return "required-fields.complete";
                case AiDeterministicEvaluationRuleKind.PolicyCompliance: return "policy.compliant";
                case AiDeterministicEvaluationRuleKind.ToolSuccess: return "tool.success";
                case AiDeterministicEvaluationRuleKind.Latency: return "latency.ms";
                case AiDeterministicEvaluationRuleKind.Cost: return "cost";
                case AiDeterministicEvaluationRuleKind.TaskCompletion: return "task.completed";
                default: throw new ArgumentOutOfRangeException(nameof(ruleKind));
            }
        }

        private static bool? EvaluateThreshold(AiEvaluationRequest request, AiEvaluationObservation observation, AiEvaluation result)
        {
            string thresholdText;
            if (!request.Criteria.TryGetValue(MaxThresholdCriterion, out thresholdText) || string.IsNullOrWhiteSpace(thresholdText))
                return Inconclusive(result, "The deterministic threshold rule requires a non-empty 'max' criterion.");

            decimal threshold;
            if (!decimal.TryParse(thresholdText, NumberStyles.Number, CultureInfo.InvariantCulture, out threshold) || threshold < 0m)
                return Inconclusive(result, "The deterministic threshold rule received an invalid non-negative 'max' criterion.");
            result.Metadata["threshold"] = threshold.ToString(CultureInfo.InvariantCulture);
            return observation.DecimalValue.Value <= threshold;
        }

        private static bool? Inconclusive(AiEvaluation result, string reason)
        {
            result.Outcome = AiEvaluationOutcome.Inconclusive;
            result.Score = null;
            result.Confidence = 1d;
            result.Label = "inconclusive";
            result.Reason = reason;
            return null;
        }
    }
}
