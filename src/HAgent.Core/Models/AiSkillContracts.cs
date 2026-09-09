using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiSkillLifecycleStatus { Draft = 0, Published = 1, Archived = 2 }
    public enum AiSkillDependencyKind { Knowledge = 0, Tool = 1 }

    public sealed class AiSkillProvenance
    {
        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUri { get; set; }
        public string CreatedBy { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string Evidence { get; set; }
        public decimal? Confidence { get; set; }
        public AiSkillProvenance Clone() => new AiSkillProvenance { Source = Source, SourceId = SourceId, SourceUri = SourceUri, CreatedBy = CreatedBy, SourceExecutionId = SourceExecutionId, SourceRuntimeInstanceId = SourceRuntimeInstanceId, Evidence = Evidence, Confidence = Confidence };
        public void Validate() { Text(Source, 256); Text(SourceId, 512); Text(SourceUri, 2048); Text(CreatedBy, 512); Text(SourceExecutionId, 256); Text(SourceRuntimeInstanceId, 256); Text(Evidence, 4096); if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m)) throw new ArgumentException("Skill provenance confidence must be between 0 and 1."); }
        private static void Text(string value, int max) { if (value != null && value.Length > max) throw new ArgumentException("Skill provenance field exceeds its maximum length."); }
    }

    public sealed class AiSkillParameterContract
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Schema { get; set; }
        public AiSkillParameterContract Clone() => new AiSkillParameterContract { Name = Name, Type = Type, Description = Description, Required = Required, Schema = Schema };
        public void Validate() { Req(Name, 128, nameof(Name)); Req(Type, 128, nameof(Type)); Opt(Description, 2048, nameof(Description)); Opt(Schema, 16000, nameof(Schema)); }
        private static void Req(string value, int max, string name) { if (string.IsNullOrWhiteSpace(value) || value.Length > max) throw new ArgumentException(name + " is required and bounded."); }
        private static void Opt(string value, int max, string name) { if (value != null && value.Length > max) throw new ArgumentException(name + " exceeds its maximum length."); }
    }

    public sealed class AiSkillPrecondition
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
        public AiSkillPrecondition Clone() => new AiSkillPrecondition { Id = Id, Description = Description, Expression = Expression };
        public void Validate() { Req(Id, 128, nameof(Id)); Req(Description, 4096, nameof(Description)); Opt(Expression, 4096, nameof(Expression)); }
        private static void Req(string v, int m, string n) { if (string.IsNullOrWhiteSpace(v) || v.Length > m) throw new ArgumentException(n + " is required and bounded."); }
        private static void Opt(string v, int m, string n) { if (v != null && v.Length > m) throw new ArgumentException(n + " exceeds its maximum length."); }
    }

    public sealed class AiSkillProcedureStep
    {
        public string Id { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public string Instruction { get; set; }
        public IList<string> RequiredToolIds { get; private set; }
        public IList<string> RequiredKnowledgeIds { get; private set; }
        public AiSkillProcedureStep() { RequiredToolIds = new List<string>(); RequiredKnowledgeIds = new List<string>(); }
        public AiSkillProcedureStep Clone() { var x = new AiSkillProcedureStep { Id = Id, Order = Order, Title = Title, Instruction = Instruction }; foreach (var v in RequiredToolIds) x.RequiredToolIds.Add(v); foreach (var v in RequiredKnowledgeIds) x.RequiredKnowledgeIds.Add(v); return x; }
        public void Validate() { Req(Id, 128, nameof(Id)); if (Order < 0) throw new ArgumentException("Skill procedure step order cannot be negative."); Req(Title, 256, nameof(Title)); Req(Instruction, 8000, nameof(Instruction)); Ids(RequiredToolIds, 32, 512, nameof(RequiredToolIds)); Ids(RequiredKnowledgeIds, 32, 512, nameof(RequiredKnowledgeIds)); }
        private static void Ids(IList<string> v, int count, int length, string name) { if (v == null || v.Count > count) throw new ArgumentException(name + " exceeds its bounds."); foreach (var x in v) Req(x, length, name); }
        private static void Req(string v, int max, string name) { if (string.IsNullOrWhiteSpace(v) || v.Length > max) throw new ArgumentException(name + " is required and bounded."); }
    }

    public sealed class AiSkillDependencyReference
    {
        public AiSkillDependencyKind Kind { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public bool Required { get; set; }
        public AiSkillDependencyReference Clone() => new AiSkillDependencyReference { Kind = Kind, ResourceId = ResourceId, Version = Version, Required = Required };
        public void Validate() { if (!Enum.IsDefined(typeof(AiSkillDependencyKind), Kind)) throw new ArgumentOutOfRangeException(nameof(Kind)); if (string.IsNullOrWhiteSpace(ResourceId) || ResourceId.Length > 512) throw new ArgumentException("Skill dependency ResourceId is required and bounded."); if (Version.HasValue && Version.Value <= 0) throw new ArgumentException("Skill dependency Version must be positive when specified."); }
    }

    public sealed class AiSkillRelationship
    {
        public string RelationshipType { get; set; }
        public string TargetResourceType { get; set; }
        public string TargetResourceId { get; set; }
        public AiSkillRelationship Clone() => new AiSkillRelationship { RelationshipType = RelationshipType, TargetResourceType = TargetResourceType, TargetResourceId = TargetResourceId };
        public void Validate() { Req(RelationshipType, 128, nameof(RelationshipType)); Req(TargetResourceType, 256, nameof(TargetResourceType)); Req(TargetResourceId, 512, nameof(TargetResourceId)); }
        private static void Req(string v, int m, string n) { if (string.IsNullOrWhiteSpace(v) || v.Length > m) throw new ArgumentException(n + " is required and bounded."); }
    }

    public sealed class AiSkillDefinition
    {
        public string Id { get; set; }
        public long Version { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AiSkillLifecycleStatus Status { get; set; }
        public IList<AiSkillParameterContract> Inputs { get; private set; }
        public IList<AiSkillParameterContract> Outputs { get; private set; }
        public IList<AiSkillPrecondition> Preconditions { get; private set; }
        public IList<AiSkillProcedureStep> Steps { get; private set; }
        public IList<AiSkillDependencyReference> Dependencies { get; private set; }
        public IDictionary<string, string> Constraints { get; private set; }
        public IDictionary<string, string> Metadata { get; private set; }
        public IList<AiSkillRelationship> Relationships { get; private set; }
        public AiSkillProvenance Provenance { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public AiSkillDefinition() { Version = 1; Status = AiSkillLifecycleStatus.Draft; CreatedUtc = DateTime.UtcNow; UpdatedUtc = CreatedUtc; Inputs = new List<AiSkillParameterContract>(); Outputs = new List<AiSkillParameterContract>(); Preconditions = new List<AiSkillPrecondition>(); Steps = new List<AiSkillProcedureStep>(); Dependencies = new List<AiSkillDependencyReference>(); Constraints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); Relationships = new List<AiSkillRelationship>(); Provenance = new AiSkillProvenance(); }
        public bool IsAuthoritative => Status == AiSkillLifecycleStatus.Published;
        public AiSkillDefinition Clone() { var x = new AiSkillDefinition { Id = Id, Version = Version, Scope = Scope, OwnerId = OwnerId, Name = Name, Description = Description, Status = Status, Provenance = Provenance?.Clone(), CreatedUtc = CreatedUtc, UpdatedUtc = UpdatedUtc }; foreach (var v in Inputs) x.Inputs.Add(v?.Clone()); foreach (var v in Outputs) x.Outputs.Add(v?.Clone()); foreach (var v in Preconditions) x.Preconditions.Add(v?.Clone()); foreach (var v in Steps) x.Steps.Add(v?.Clone()); foreach (var v in Dependencies) x.Dependencies.Add(v?.Clone()); foreach (var v in Constraints) x.Constraints[v.Key] = v.Value; foreach (var v in Metadata) x.Metadata[v.Key] = v.Value; foreach (var v in Relationships) x.Relationships.Add(v?.Clone()); return x; }
        public void Validate() { Req(Id, 128, nameof(Id)); if (Version <= 0) throw new ArgumentException("Skill Version must be positive."); if (!Enum.IsDefined(typeof(AgentResourceScope), Scope)) throw new ArgumentException("Invalid skill scope."); if (Scope != AgentResourceScope.Global && string.IsNullOrWhiteSpace(OwnerId)) throw new ArgumentException("OwnerId is required outside global scope."); Opt(OwnerId, 2048, nameof(OwnerId)); Req(Name, 500, nameof(Name)); Req(Description, 8000, nameof(Description)); if (!Enum.IsDefined(typeof(AiSkillLifecycleStatus), Status)) throw new ArgumentException("Invalid skill lifecycle status."); if (Provenance == null) throw new ArgumentException("Skill provenance is required."); Provenance.Validate(); if (CreatedUtc == default(DateTime) || UpdatedUtc == default(DateTime) || UpdatedUtc < CreatedUtc) throw new ArgumentException("Skill timestamps are invalid."); Contracts(Inputs, 64, nameof(Inputs)); Contracts(Outputs, 64, nameof(Outputs)); ValidatePreconditions(); ValidateSteps(); ValidateDependencies(); Maps(Constraints, 64, 128, 4096, nameof(Constraints)); Maps(Metadata, 64, 128, 4096, nameof(Metadata)); if (Relationships == null || Relationships.Count > 64) throw new ArgumentException("Skill relationships exceed their bounds."); foreach (var v in Relationships) { if (v == null) throw new ArgumentException("Skill relationships cannot contain null entries."); v.Validate(); } }
        private void ValidatePreconditions() { if (Preconditions == null || Preconditions.Count > 64) throw new ArgumentException("Skill preconditions exceed their bounds."); foreach (var v in Preconditions) { if (v == null) throw new ArgumentException("Skill preconditions cannot contain null entries."); v.Validate(); } }
        private void ValidateSteps() { if (Steps == null || Steps.Count > 128) throw new ArgumentException("Skill procedure steps exceed their bounds."); var orders = new HashSet<int>(); foreach (var v in Steps) { if (v == null) throw new ArgumentException("Skill procedure steps cannot contain null entries."); v.Validate(); if (!orders.Add(v.Order)) throw new ArgumentException("Skill procedure step orders must be unique."); } }
        private void ValidateDependencies() { if (Dependencies == null || Dependencies.Count > 128) throw new ArgumentException("Skill dependencies exceed their bounds."); foreach (var v in Dependencies) { if (v == null) throw new ArgumentException("Skill dependencies cannot contain null entries."); v.Validate(); } }
        private static void Contracts(IList<AiSkillParameterContract> v, int max, string name) { if (v == null || v.Count > max) throw new ArgumentException(name + " exceed their bounds."); var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase); foreach (var x in v) { if (x == null) throw new ArgumentException(name + " cannot contain null entries."); x.Validate(); if (!names.Add(x.Name.Trim())) throw new ArgumentException(name + " parameter names must be unique."); } }
        private static void Maps(IDictionary<string, string> v, int max, int key, int value, string name) { if (v == null || v.Count > max) throw new ArgumentException(name + " exceed their bounds."); foreach (var x in v) if (string.IsNullOrWhiteSpace(x.Key) || x.Key.Length > key || x.Value == null || x.Value.Length > value) throw new ArgumentException(name + " contains an invalid entry."); }
        private static void Req(string v, int max, string name) { if (string.IsNullOrWhiteSpace(v) || v.Length > max) throw new ArgumentException(name + " is required and bounded."); }
        private static void Opt(string v, int max, string name) { if (v != null && v.Length > max) throw new ArgumentException(name + " exceeds its maximum length."); }
    }

    public sealed class AiSkillReference
    {
        public string SkillId { get; set; }
        public long? Version { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string OwnerId { get; set; }
        public bool Required { get; set; }
        public AiSkillReference Clone() => new AiSkillReference { SkillId = SkillId, Version = Version, Scope = Scope, OwnerId = OwnerId, Required = Required };
        public void Validate() { if (string.IsNullOrWhiteSpace(SkillId) || SkillId.Length > 128) throw new ArgumentException("Skill reference SkillId is required and bounded."); if (!Enum.IsDefined(typeof(AgentResourceScope), Scope)) throw new ArgumentException("Invalid skill reference scope."); if (Scope != AgentResourceScope.Global && string.IsNullOrWhiteSpace(OwnerId)) throw new ArgumentException("Skill reference OwnerId is required outside global scope."); if (OwnerId != null && OwnerId.Length > 2048) throw new ArgumentException("Skill reference OwnerId is too long."); if (Version.HasValue && Version.Value <= 0) throw new ArgumentException("Skill reference Version must be positive when specified."); }
    }

    public sealed class AiSkillSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<AiSkillReference> References { get; private set; }
        public AiSkillSet() { References = new List<AiSkillReference>(); }
        public AiSkillSet Clone() { var x = new AiSkillSet { Id = Id, Name = Name }; foreach (var v in References) x.References.Add(v?.Clone()); return x; }
        public void Validate() { if (Id != null && Id.Length > 128) throw new ArgumentException("Skill set Id is too long."); if (string.IsNullOrWhiteSpace(Name) || Name.Length > 500) throw new ArgumentException("Skill set Name is required and bounded."); if (References == null || References.Count > 256) throw new ArgumentException("Skill set references exceed their bounds."); var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase); foreach (var v in References) { if (v == null) throw new ArgumentException("Skill set references cannot contain null entries."); v.Validate(); var key = v.SkillId + "\n" + v.Scope + "\n" + v.OwnerId + "\n" + (v.Version.HasValue ? v.Version.Value.ToString() : "latest"); if (!keys.Add(key)) throw new ArgumentException("Skill set references must be unique."); } }
    }

    public sealed class AiSkillBinding
    {
        public AiSkillReference Reference { get; set; }
        public AiSkillDefinition Definition { get; set; }
        public AiSkillBinding Clone() => new AiSkillBinding { Reference = Reference?.Clone(), Definition = Definition?.Clone() };
        public void Validate() { if (Reference == null || Definition == null) throw new ArgumentException("Skill binding reference and definition are required."); Reference.Validate(); Definition.Validate(); if (!string.Equals(Reference.SkillId, Definition.Id, StringComparison.OrdinalIgnoreCase) || Reference.Scope != Definition.Scope || !string.Equals(Reference.OwnerId ?? string.Empty, Definition.OwnerId ?? string.Empty, StringComparison.Ordinal)) throw new ArgumentException("Skill binding identity does not match the definition."); if (Reference.Version.HasValue && Reference.Version.Value != Definition.Version) throw new ArgumentException("Skill binding version does not match the definition."); if (!Definition.IsAuthoritative) throw new ArgumentException("Only published skill definitions may be bound for execution."); }
    }

    public sealed class AiSkillExecutionSnapshot
    {
        public IReadOnlyList<AiSkillBinding> Bindings { get; private set; }
        public AiSkillExecutionSnapshot(IEnumerable<AiSkillBinding> bindings) { var x = new List<AiSkillBinding>(); foreach (var v in bindings ?? Enumerable.Empty<AiSkillBinding>()) { if (v == null) throw new ArgumentException("Skill execution snapshots cannot contain null bindings."); var clone = v.Clone(); clone.Validate(); x.Add(clone); } Bindings = x.AsReadOnly(); }
        public AiSkillExecutionSnapshot Clone() => new AiSkillExecutionSnapshot(Bindings);
    }

    public interface IAiSkillDefinitionSource
    {
        Task<AiSkillDefinition> GetAsync(AiSkillReference reference, CancellationToken cancellationToken);
    }

    public sealed class AiGovernedSkillResolver
    {
        private readonly IAiSkillDefinitionSource _source;
        private readonly AiResourceGovernanceEvaluator _governance;
        public AiGovernedSkillResolver(IAiSkillDefinitionSource source, AiResourceGovernanceEvaluator governance) { _source = source ?? throw new ArgumentNullException(nameof(source)); _governance = governance ?? throw new ArgumentNullException(nameof(governance)); }
        public async Task<AiSkillExecutionSnapshot> ResolveAsync(AiSkillSet skillSet, AgentIdentityContext identity, string agentProfileId, string runtimeInstanceId, string executionId, CancellationToken cancellationToken)
        {
            if (skillSet == null) throw new ArgumentNullException(nameof(skillSet));
            skillSet.Validate();
            var bindings = new List<AiSkillBinding>();
            foreach (var reference in skillSet.References)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var decision = _governance.Evaluate(new AiResourceGovernanceRequest { Operation = "skill.invoke", ResourceType = "skill", ResourceId = reference.SkillId, Scope = reference.Scope, ResourceOwnerId = reference.OwnerId, AgentProfileId = agentProfileId ?? string.Empty, RuntimeInstanceId = runtimeInstanceId ?? string.Empty, ExecutionId = executionId ?? string.Empty, Identity = identity == null ? new AgentIdentityContext() : identity.Clone() });
                if (!decision.Allowed) { if (reference.Required) throw new InvalidOperationException("Required skill was not admitted: " + reference.SkillId + ". " + decision.Reason); continue; }
                var definition = await _source.GetAsync(reference.Clone(), cancellationToken).ConfigureAwait(false);
                if (definition == null || !definition.IsAuthoritative || !string.Equals(definition.Id, reference.SkillId, StringComparison.OrdinalIgnoreCase) || definition.Scope != reference.Scope || !string.Equals(definition.OwnerId ?? string.Empty, reference.OwnerId ?? string.Empty, StringComparison.Ordinal) || (reference.Version.HasValue && definition.Version != reference.Version.Value)) { if (reference.Required) throw new InvalidOperationException("Required skill definition does not satisfy the reference: " + reference.SkillId + "."); continue; }
                var binding = new AiSkillBinding { Reference = reference.Clone(), Definition = definition.Clone() }; binding.Validate(); bindings.Add(binding);
            }
            return new AiSkillExecutionSnapshot(bindings);
        }
    }
}
