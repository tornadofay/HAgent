using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HAgent.Models
{
    public enum AiPolicyScopeKind
    {
        System,
        Tenant,
        User,
        Workspace,
        Agent,
        Runtime,
        Execution,
        Resource,
        Tool,
        Provider,
        ExecutionTarget
    }

    public enum AiPolicyOutcome
    {
        NotApplicable,
        Allow,
        Deny,
        RequireApproval,
        Defer
    }

    public sealed class AiPolicySet
    {
        public AiPolicySet()
        {
            Version = "1";
            Rules = new List<AiPolicyRule>();
        }

        public string Version { get; set; }
        [JsonInclude]
        public IList<AiPolicyRule> Rules { get; private set; }

        public AiPolicySet Clone()
        {
            var clone = new AiPolicySet { Version = Version };
            foreach (var rule in Rules ?? new List<AiPolicyRule>())
                if (rule != null) clone.Rules.Add(rule.Clone());
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Version))
                throw new ArgumentException("Policy version is required.", nameof(Version));
            if (Version.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Version));

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in Rules ?? new List<AiPolicyRule>())
            {
                if (rule == null) throw new ArgumentException("Policy rules cannot contain null entries.", nameof(Rules));
                rule.Validate();
                if (!ids.Add(rule.Id))
                    throw new ArgumentException("Policy rule ids must be unique: " + rule.Id, nameof(Rules));
            }
        }
    }

    public sealed class AiPolicyRule
    {
        public AiPolicyRule()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = string.Empty;
            Scope = AiPolicyScopeKind.System;
            ScopeId = string.Empty;
            Priority = 0;
            Outcome = AiPolicyOutcome.NotApplicable;
            Reason = string.Empty;
            Operations = new List<string>();
            ResourceTypes = new List<string>();
            ResourceIds = new List<string>();
            ToolIds = new List<string>();
            ProviderIds = new List<string>();
            ExecutionTargetIds = new List<string>();
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public AiPolicyScopeKind Scope { get; set; }
        public string ScopeId { get; set; }
        public int Priority { get; set; }
        public AiPolicyOutcome Outcome { get; set; }
        public string Reason { get; set; }
        [JsonInclude]
        public IList<string> Operations { get; private set; }
        [JsonInclude]
        public IList<string> ResourceTypes { get; private set; }
        [JsonInclude]
        public IList<string> ResourceIds { get; private set; }
        [JsonInclude]
        public IList<string> ToolIds { get; private set; }
        [JsonInclude]
        public IList<string> ProviderIds { get; private set; }
        [JsonInclude]
        public IList<string> ExecutionTargetIds { get; private set; }
        [JsonInclude]
        public IDictionary<string, string> Attributes { get; private set; }

        public AiPolicyRule Clone()
        {
            var clone = new AiPolicyRule
            {
                Id = Id,
                Name = Name,
                Scope = Scope,
                ScopeId = ScopeId,
                Priority = Priority,
                Outcome = Outcome,
                Reason = Reason
            };
            Copy(Operations, clone.Operations);
            Copy(ResourceTypes, clone.ResourceTypes);
            Copy(ResourceIds, clone.ResourceIds);
            Copy(ToolIds, clone.ToolIds);
            Copy(ProviderIds, clone.ProviderIds);
            Copy(ExecutionTargetIds, clone.ExecutionTargetIds);
            foreach (var pair in Attributes ?? new Dictionary<string, string>())
                clone.Attributes[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 256);
            Require(Name, nameof(Name), 512, false);
            if (Scope != AiPolicyScopeKind.System && string.IsNullOrWhiteSpace(ScopeId))
                throw new ArgumentException("Non-system policy scopes require ScopeId.", nameof(ScopeId));
            if (Reason != null && Reason.Length > 2048)
                throw new ArgumentOutOfRangeException(nameof(Reason));
            ValidateValues(Operations, nameof(Operations), 256);
            ValidateValues(ResourceTypes, nameof(ResourceTypes), 256);
            ValidateValues(ResourceIds, nameof(ResourceIds), 512);
            ValidateValues(ToolIds, nameof(ToolIds), 512);
            ValidateValues(ProviderIds, nameof(ProviderIds), 256);
            ValidateValues(ExecutionTargetIds, nameof(ExecutionTargetIds), 512);
            if (Attributes == null) throw new ArgumentNullException(nameof(Attributes));
            if (Attributes.Count > 32) throw new ArgumentOutOfRangeException(nameof(Attributes));
            foreach (var pair in Attributes)
            {
                Require(pair.Key, nameof(Attributes), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Attributes));
            }
        }

        private static void Copy(IEnumerable<string> source, IList<string> target)
        {
            foreach (var value in source ?? Enumerable.Empty<string>())
                target.Add(value);
        }

        private static void ValidateValues(IEnumerable<string> values, string name, int maxLength)
        {
            foreach (var value in values ?? Enumerable.Empty<string>())
                Require(value, name, maxLength);
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiPolicyEvaluationContext
    {
        public AiPolicyEvaluationContext()
        {
            Operation = string.Empty;
            ResourceType = string.Empty;
            ResourceId = string.Empty;
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            ToolId = string.Empty;
            ProviderId = string.Empty;
            ExecutionTargetId = string.Empty;
            CostStatus = AiCostStatus.Unknown;
            RequestedCostPolicy = AiCostPolicy.NoRestriction;
            Identity = new AgentIdentityContext();
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Operation { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public string AgentProfileId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string ToolId { get; set; }
        public string ProviderId { get; set; }
        public string ExecutionTargetId { get; set; }
        public AiCostStatus CostStatus { get; set; }
        public AiCostPolicy RequestedCostPolicy { get; set; }
        public AgentIdentityContext Identity { get; set; }
        [JsonInclude]
        public IDictionary<string, string> Attributes { get; private set; }

        public AiPolicyEvaluationContext Clone()
        {
            var clone = new AiPolicyEvaluationContext
            {
                Operation = Operation,
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                ToolId = ToolId,
                ProviderId = ProviderId,
                ExecutionTargetId = ExecutionTargetId,
                CostStatus = CostStatus,
                RequestedCostPolicy = RequestedCostPolicy,
                Identity = Identity == null ? new AgentIdentityContext() : Identity.Clone()
            };
            foreach (var pair in Attributes ?? new Dictionary<string, string>())
                clone.Attributes[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            Require(Operation, nameof(Operation), 256);
            Require(ResourceType, nameof(ResourceType), 256, false);
            Require(ResourceId, nameof(ResourceId), 512, false);
            Require(AgentProfileId, nameof(AgentProfileId), 512, false);
            Require(RuntimeInstanceId, nameof(RuntimeInstanceId), 512, false);
            Require(ExecutionId, nameof(ExecutionId), 512, false);
            Require(ToolId, nameof(ToolId), 512, false);
            Require(ProviderId, nameof(ProviderId), 256, false);
            Require(ExecutionTargetId, nameof(ExecutionTargetId), 512, false);
            if (Identity != null) Identity.Validate();
            if (Attributes == null) throw new ArgumentNullException(nameof(Attributes));
            if (Attributes.Count > 32) throw new ArgumentOutOfRangeException(nameof(Attributes));
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiPolicyDecision
    {
        public AiPolicyDecision()
        {
            Outcome = AiPolicyOutcome.NotApplicable;
            PolicyVersion = string.Empty;
            RuleId = string.Empty;
            RuleName = string.Empty;
            Scope = AiPolicyScopeKind.System;
            Reason = string.Empty;
            IsBuiltIn = false;
            EvaluatedAt = DateTimeOffset.UtcNow;
        }

        public AiPolicyOutcome Outcome { get; set; }
        public string PolicyVersion { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public AiPolicyScopeKind Scope { get; set; }
        public int Priority { get; set; }
        public string Reason { get; set; }
        public bool IsBuiltIn { get; set; }
        public DateTimeOffset EvaluatedAt { get; private set; }

        public bool IsAllowed { get { return Outcome == AiPolicyOutcome.Allow; } }
        public bool RequiresApproval { get { return Outcome == AiPolicyOutcome.RequireApproval; } }
        public bool IsDenied { get { return Outcome == AiPolicyOutcome.Deny; } }
        public bool IsDeferred { get { return Outcome == AiPolicyOutcome.Defer; } }
    }
}
