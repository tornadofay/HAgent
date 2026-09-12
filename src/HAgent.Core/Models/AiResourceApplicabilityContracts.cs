using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiApplicabilityOutcome
    {
        Applicable = 0,
        NotApplicable = 1,
        Uncertain = 2,
        Invalidated = 3
    }

    public enum AiApplicabilityConditionOperator
    {
        Exists = 0,
        Equals = 1,
        NotEquals = 2,
        OneOf = 3
    }

    public sealed class AiApplicabilityEvidenceReference
    {
        public string Kind { get; set; }
        public string Id { get; set; }
        public string Role { get; set; }
        public string Source { get; set; }

        public AiApplicabilityEvidenceReference Clone()
        {
            return new AiApplicabilityEvidenceReference
            {
                Kind = Kind,
                Id = Id,
                Role = Role,
                Source = Source
            };
        }

        public void Validate()
        {
            ValidateText(Kind, 128, true, nameof(Kind));
            ValidateText(Id, 512, true, nameof(Id));
            ValidateText(Role, 128, false, nameof(Role));
            ValidateText(Source, 256, false, nameof(Source));
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiApplicabilityCondition
    {
        public string Key { get; set; }
        public AiApplicabilityConditionOperator Operator { get; set; }
        public string ExpectedValue { get; set; }
        public string EvidenceReferenceId { get; set; }
        public IList<string> AllowedValues { get; private set; }

        public AiApplicabilityCondition()
        {
            AllowedValues = new List<string>();
        }

        public AiApplicabilityCondition Clone()
        {
            var clone = new AiApplicabilityCondition
            {
                Key = Key,
                Operator = Operator,
                ExpectedValue = ExpectedValue,
                EvidenceReferenceId = EvidenceReferenceId
            };
            foreach (var value in AllowedValues)
                clone.AllowedValues.Add(value);
            return clone;
        }

        public void Validate()
        {
            ValidateText(Key, 128, true, nameof(Key));
            if (!Enum.IsDefined(typeof(AiApplicabilityConditionOperator), Operator))
                throw new ArgumentException("Invalid applicability condition operator.", nameof(Operator));
            ValidateText(ExpectedValue, 2048, false, nameof(ExpectedValue));
            ValidateText(EvidenceReferenceId, 512, false, nameof(EvidenceReferenceId));
            if (AllowedValues == null || AllowedValues.Count > 32)
                throw new ArgumentException("Applicability AllowedValues exceeds its bounds.", nameof(AllowedValues));
            foreach (var value in AllowedValues)
            {
                ValidateText(value, 2048, true, "AllowedValues");
            }

            if ((Operator == AiApplicabilityConditionOperator.Equals ||
                 Operator == AiApplicabilityConditionOperator.NotEquals) &&
                string.IsNullOrWhiteSpace(ExpectedValue))
                throw new ArgumentException("Equality applicability conditions require ExpectedValue.", nameof(ExpectedValue));

            if (Operator == AiApplicabilityConditionOperator.OneOf && AllowedValues.Count == 0)
                throw new ArgumentException("OneOf applicability conditions require AllowedValues.", nameof(AllowedValues));
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiApplicabilityTarget
    {
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public AgentResourceScope Scope { get; set; }
        public bool IsInvalidated { get; set; }
        public string InvalidationReason { get; set; }
        public IList<AiApplicabilityCondition> Preconditions { get; private set; }
        public IList<AiApplicabilityEvidenceReference> Evidence { get; private set; }

        public AiApplicabilityTarget()
        {
            Preconditions = new List<AiApplicabilityCondition>();
            Evidence = new List<AiApplicabilityEvidenceReference>();
        }

        public AiApplicabilityTarget Clone()
        {
            var clone = new AiApplicabilityTarget
            {
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Version = Version,
                Scope = Scope,
                IsInvalidated = IsInvalidated,
                InvalidationReason = InvalidationReason
            };
            foreach (var condition in Preconditions)
                clone.Preconditions.Add(condition == null ? null : condition.Clone());
            foreach (var evidence in Evidence)
                clone.Evidence.Add(evidence == null ? null : evidence.Clone());
            return clone;
        }

        public void Validate()
        {
            ValidateText(ResourceType, 128, true, nameof(ResourceType));
            ValidateText(ResourceId, 512, true, nameof(ResourceId));
            if (Version.HasValue && Version.Value <= 0)
                throw new ArgumentException("Applicability target Version must be positive.", nameof(Version));
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentException("Invalid applicability target scope.", nameof(Scope));
            ValidateText(InvalidationReason, 1024, false, nameof(InvalidationReason));
            if (Preconditions == null || Preconditions.Count > 32)
                throw new ArgumentException("Applicability preconditions exceed their bounds.", nameof(Preconditions));
            foreach (var condition in Preconditions)
            {
                if (condition == null) throw new ArgumentException("Applicability preconditions cannot contain null entries.");
                condition.Validate();
            }
            if (Evidence == null || Evidence.Count > 64)
                throw new ArgumentException("Applicability evidence exceeds its bounds.", nameof(Evidence));
            foreach (var evidence in Evidence)
            {
                if (evidence == null) throw new ArgumentException("Applicability evidence cannot contain null entries.");
                evidence.Validate();
            }
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiApplicabilityContext
    {
        public AgentResourceScope? Scope { get; set; }
        public IDictionary<string, string> Facts { get; private set; }

        public AiApplicabilityContext()
        {
            Facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public AiApplicabilityContext Clone()
        {
            var clone = new AiApplicabilityContext { Scope = Scope };
            foreach (var pair in Facts)
                clone.Facts[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            if (Scope.HasValue && !Enum.IsDefined(typeof(AgentResourceScope), Scope.Value))
                throw new ArgumentException("Invalid applicability context scope.", nameof(Scope));
            if (Facts == null || Facts.Count > 128)
                throw new ArgumentException("Applicability context facts exceed their bounds.", nameof(Facts));
            foreach (var pair in Facts)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128)
                    throw new ArgumentException("Applicability context fact keys must be bounded and non-empty.");
                if (pair.Value == null || pair.Value.Length > 2048)
                    throw new ArgumentException("Applicability context fact values must be bounded and non-null.");
            }
        }
    }

    public sealed class AiApplicabilityConditionResult
    {
        public string Key { get; set; }
        public AiApplicabilityConditionOperator Operator { get; set; }
        public bool Satisfied { get; set; }
        public bool EvidenceAvailable { get; set; }
        public string ExpectedValue { get; set; }
        public string ObservedValue { get; set; }
        public string EvidenceReferenceId { get; set; }

        public AiApplicabilityConditionResult Clone()
        {
            return new AiApplicabilityConditionResult
            {
                Key = Key,
                Operator = Operator,
                Satisfied = Satisfied,
                EvidenceAvailable = EvidenceAvailable,
                ExpectedValue = ExpectedValue,
                ObservedValue = ObservedValue,
                EvidenceReferenceId = EvidenceReferenceId
            };
        }
    }

    public sealed class AiApplicabilityDecision
    {
        public AiApplicabilityOutcome Outcome { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }
        public IList<AiApplicabilityConditionResult> Conditions { get; private set; }
        public IList<AiApplicabilityEvidenceReference> Evidence { get; private set; }

        public AiApplicabilityDecision()
        {
            Conditions = new List<AiApplicabilityConditionResult>();
            Evidence = new List<AiApplicabilityEvidenceReference>();
            EvaluatedAtUtc = DateTimeOffset.UtcNow;
        }

        public AiApplicabilityDecision Clone()
        {
            var clone = new AiApplicabilityDecision
            {
                Outcome = Outcome,
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Version = Version,
                Scope = Scope,
                Reason = Reason,
                EvaluatedAtUtc = EvaluatedAtUtc
            };
            foreach (var condition in Conditions)
                clone.Conditions.Add(condition == null ? null : condition.Clone());
            foreach (var evidence in Evidence)
                clone.Evidence.Add(evidence == null ? null : evidence.Clone());
            return clone;
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiApplicabilityOutcome), Outcome))
                throw new ArgumentException("Invalid applicability outcome.", nameof(Outcome));
            ValidateText(ResourceType, 128, true, nameof(ResourceType));
            ValidateText(ResourceId, 512, true, nameof(ResourceId));
            if (Version.HasValue && Version.Value <= 0)
                throw new ArgumentException("Applicability decision Version must be positive.", nameof(Version));
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentException("Invalid applicability decision scope.", nameof(Scope));
            ValidateText(Reason, 1024, true, nameof(Reason));
            if (EvaluatedAtUtc == default(DateTimeOffset))
                throw new ArgumentException("Applicability decision timestamp is required.", nameof(EvaluatedAtUtc));
            if (Conditions == null || Conditions.Count > 32)
                throw new ArgumentException("Applicability condition results exceed their bounds.", nameof(Conditions));
            if (Evidence == null || Evidence.Count > 64)
                throw new ArgumentException("Applicability evidence exceeds its bounds.", nameof(Evidence));
            foreach (var evidence in Evidence)
            {
                if (evidence == null) throw new ArgumentException("Applicability evidence cannot contain null entries.");
                evidence.Validate();
            }
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiApplicabilityRequest
    {
        public AiApplicabilityTarget Target { get; set; }
        public AiApplicabilityContext Context { get; set; }

        public void Validate()
        {
            if (Target == null) throw new ArgumentNullException(nameof(Target));
            if (Context == null) throw new ArgumentNullException(nameof(Context));
            Target.Validate();
            Context.Validate();
        }
    }
}
