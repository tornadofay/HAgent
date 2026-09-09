using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiSkillLifecycleStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum AiSkillDependencyKind
    {
        Knowledge = 0,
        Tool = 1
    }

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

        public AiSkillProvenance Clone()
        {
            return new AiSkillProvenance
            {
                Source = Source,
                SourceId = SourceId,
                SourceUri = SourceUri,
                CreatedBy = CreatedBy,
                SourceExecutionId = SourceExecutionId,
                SourceRuntimeInstanceId = SourceRuntimeInstanceId,
                Evidence = Evidence,
                Confidence = Confidence
            };
        }

        public void Validate()
        {
            ValidateText(Source, 256, false, nameof(Source));
            ValidateText(SourceId, 512, false, nameof(SourceId));
            ValidateText(SourceUri, 2048, false, nameof(SourceUri));
            ValidateText(CreatedBy, 512, false, nameof(CreatedBy));
            ValidateText(SourceExecutionId, 256, false, nameof(SourceExecutionId));
            ValidateText(SourceRuntimeInstanceId, 256, false, nameof(SourceRuntimeInstanceId));
            ValidateText(Evidence, 4096, false, nameof(Evidence));
            if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m))
                throw new ArgumentException("Skill provenance confidence must be between 0 and 1.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiSkillParameterContract
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Schema { get; set; }

        public AiSkillParameterContract Clone()
        {
            return new AiSkillParameterContract
            {
                Name = Name,
                Type = Type,
                Description = Description,
                Required = Required,
                Schema = Schema
            };
        }

        public void Validate()
        {
            ValidateText(Name, 128, true, nameof(Name));
            ValidateText(Type, 128, true, nameof(Type));
            ValidateText(Description, 2048, false, nameof(Description));
            ValidateText(Schema, 16000, false, nameof(Schema));
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiSkillPrecondition
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }

        public AiSkillPrecondition Clone()
        {
            return new AiSkillPrecondition { Id = Id, Description = Description, Expression = Expression };
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, nameof(Id));
            ValidateText(Description, 4096, true, nameof(Description));
            ValidateText(Expression, 4096, false, nameof(Expression));
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiSkillProcedureStep
    {
        public string Id { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public string Instruction { get; set; }
        public IList<string> RequiredToolIds { get; private set; }
        public IList<string> RequiredKnowledgeIds { get; private set; }

        public AiSkillProcedureStep()
        {
            RequiredToolIds = new List<string>();
            RequiredKnowledgeIds = new List<string>();
        }

        public AiSkillProcedureStep Clone()
        {
            var clone = new AiSkillProcedureStep
            {
                Id = Id,
                Order = Order,
                Title = Title,
                Instruction = Instruction
            };
            foreach (var id in RequiredToolIds) clone.RequiredToolIds.Add(id);
            foreach (var id in RequiredKnowledgeIds) clone.RequiredKnowledgeIds.Add(id);
            return clone;
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, nameof(Id));
            if (Order < 0) throw new ArgumentException("Skill procedure step order cannot be negative.");
            ValidateText(Title, 256, true, nameof(Title));
            ValidateText(Instruction, 8000, true, nameof(Instruction));
            ValidateIds(RequiredToolIds, 32, 512, nameof(RequiredToolIds));
            ValidateIds(RequiredKnowledgeIds, 32, 512, nameof(RequiredKnowledgeIds));
        }

        private static void ValidateIds(IList<string> values, int maxCount, int maxLength, string name)
        {
            if (values == null || values.Count > maxCount) throw new ArgumentException(name + " exceeds its bounds.");
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength) throw new ArgumentException(name + " contains an invalid identifier.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength) throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiSkillDependencyReference
    {
        public AiSkillDependencyKind Kind { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public bool Required { get; set; }

        public AiSkillDependencyReference Clone()
        {
            return new AiSkillDependencyReference { Kind = Kind, ResourceId = ResourceId, Version = Version, Required = Required };
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiSkillDependencyKind), Kind)) throw new ArgumentOutOfRangeException(nameof(Kind));
            if (string.IsNullOrWhiteSpace(ResourceId) || ResourceId.Length > 512) throw new ArgumentException("Skill dependency ResourceId is required and bounded.");
            if (Version.HasValue && Version.Value <= 0) throw new ArgumentException("Skill dependency Version must be positive when specified.");
        }
    }

    public sealed class AiSkillRelationship
    {
        public string RelationshipType { get; set; }
        public string TargetResourceType { get; set; }
        public string TargetResourceId { get; set; }

        public AiSkillRelationship Clone()
        {
            return new AiSkillRelationship
            {
                RelationshipType = RelationshipType,
                TargetResourceType = TargetResourceType,
                TargetResourceId = TargetResourceId
            };
        }

        public void Validate()
        {
            ValidateText(RelationshipType, 128, true, nameof(RelationshipType));
            ValidateText(TargetResourceType, 256, true, nameof(TargetResourceType));
            ValidateText(TargetResourceId, 512, true, nameof(TargetResourceId));
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength) throw new ArgumentException(name + " exceeds its maximum length.");
        }
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

        public AiSkillDefinition()
        {
            Version = 1;
            Status = AiSkillLifecycleStatus.Draft;
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = CreatedUtc;
            Inputs = new List<AiSkillParameterContract>();
            Outputs = new List<AiSkillParameterContract>();
            Preconditions = new List<AiSkillPrecondition>();
            Steps = new List<AiSkillProcedureStep>();
            Dependencies = new List<AiSkillDependencyReference>();
            Constraints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Relationships = new List<AiSkillRelationship>();
            Provenance = new AiSkillProvenance();
        }

        public bool IsAuthoritative { get { return Status == AiSkillLifecycleStatus.Published; } }

        public AiSkillDefinition Clone()
        {
            var clone = new AiSkillDefinition
            {
                Id = Id,
                Version = Version,
                Scope = Scope,
                OwnerId = OwnerId,
                Name = Name,
                Description = Description,
                Status = Status,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                CreatedUtc = CreatedUtc,
                UpdatedUtc = UpdatedUtc
            };
            foreach (var item in Inputs) clone.Inputs.Add(item == null ? null : item.Clone());
            foreach (var item in Outputs) clone.Outputs.Add(item == null ? null : item.Clone());
            foreach (var item in Preconditions) clone.Preconditions.Add(item == null ? null : item.Clone());
            foreach (var item in Steps) clone.Steps.Add(item == null ? null : item.Clone());
            foreach (var item in Dependencies) clone.Dependencies.Add(item == null ? null : item.Clone());
            foreach (var pair in Constraints) clone.Constraints[pair.Key] = pair.Value;
            foreach (var pair in Metadata) clone.Metadata[pair.Key] = pair.Value;
            foreach (var item in Relationships) clone.Relationships.Add(item == null ? null : item.Clone());
            return clone;
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, nameof(Id));
            if (Version <= 0) throw new ArgumentException("Skill Version must be positive.");
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope)) throw new ArgumentException("Invalid skill scope.");
            ValidateText(OwnerId, 2048, Scope != AgentResourceScope.Global, nameof(OwnerId));
            ValidateText(Name, 500, true, nameof(Name));
            ValidateText(Description, 8000, true, nameof(Description));
            if (!Enum.IsDefined(typeof(AiSkillLifecycleStatus), Status)) throw new ArgumentException("Invalid skill lifecycle status.");
            if (Provenance == null) throw new ArgumentException("Skill provenance is required.");
            Provenance.Validate();
            if (CreatedUtc == default(DateTime) || UpdatedUtc == default(DateTime) || UpdatedUtc < CreatedUtc) throw new ArgumentException("Skill timestamps are invalid.");
            ValidateContracts(Inputs, 64, nameof(Inputs));
            ValidateContracts(Outputs, 64, nameof(Outputs));
            if (Preconditions == null || Preconditions.Count > 64) throw new ArgumentException("Skill preconditions exceed their bounds.");
            foreach (var item in Preconditions) { if (item == null) throw new ArgumentException("Skill preconditions cannot contain null entries."); item.Validate(); }
            if (Steps == null || Steps.Count > 128) throw new ArgumentException("Skill procedure steps exceed their bounds.");
            var orderValues = new HashSet<int>();
            foreach (var item in Steps) { if (item == null) throw new ArgumentException("Skill procedure steps cannot contain null entries."); item.Validate(); if (!orderValues.Add(item.Order)) throw new ArgumentException("Skill procedure step orders must be unique."); }
            if (Dependencies == null || Dependencies.Count > 128) throw new ArgumentException("Skill dependencies exceed their bounds.");
            foreach (var item in Dependencies) { if (item == null) throw new ArgumentException("Skill dependencies cannot contain null entries."); item.Validate(); }
            ValidateMap(Constraints, 64, 128, 4096, nameof(Constraints));
            ValidateMap(Metadata, 64, 128, 4096, nameof(Metadata));
            if (Relationships == null || Relationships.Count > 64) throw new ArgumentException("Skill relationships exceed their bounds.");
            foreach (var item in Relationships) { if (item == null) throw new ArgumentException("Skill relationships cannot contain null entries."); item.Validate(); }
        }

        private static void ValidateContracts(IList<AiSkillParameterContract> values, int maxCount, string name)
        {
            if (values == null || values.Count > maxCount) throw new ArgumentException(name + " exceed their bounds.");
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in values) { if (item == null) throw new ArgumentException(name + " cannot contain null entries."); item.Validate(); if (!names.Add(item.Name.Trim())) throw new ArgumentException(name + " parameter names must be unique."); }
        }

        private static void ValidateMap(IDictionary<string, string> values, int maxEntries, int maxKeyLength, int maxValueLength, string name)
        {
            if (values == null || values.Count > maxEntries) throw new ArgumentException(name + " exceeds its bounds.");
            foreach (var pair in values)
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > maxKeyLength || pair.Value == null || pair.Value.Length > maxValueLength) throw new ArgumentException(name + " contains an invalid entry.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength) throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public sealed class AiSkillReference
    {
        public string SkillId { get; set; }
        public long? Version { get; set; }
        public bool Required { get; set; }

        public AiSkillReference Clone()
        {
            return new AiSkillReference { SkillId = SkillId, Version = Version, Required = Required };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SkillId) || SkillId.Length > 128) throw new ArgumentException("Skill reference SkillId is required and bounded.");
            if (Version.HasValue && Version.Value <= 0) throw new ArgumentException("Skill reference Version must be positive when specified.");
        }
    }

    public sealed class AiSkillSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<AiSkillReference> References { get; private set; }

        public AiSkillSet()
        {
            References = new List<AiSkillReference>();
        }

        public AiSkillSet Clone()
        {
            var clone = new AiSkillSet { Id = Id, Name = Name };
            foreach (var reference in References) clone.References.Add(reference == null ? null : reference.Clone());
            return clone;
        }

        public void Validate()
        {
            if (Id != null && Id.Length > 128) throw new ArgumentException("Skill set Id is too long.");
            if (string.IsNullOrWhiteSpace(Name) || Name.Length > 500) throw new ArgumentException("Skill set Name is required and bounded.");
            if (References == null || References.Count > 256) throw new ArgumentException("Skill set references exceed their bounds.");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var reference in References) { if (reference == null) throw new ArgumentException("Skill set references cannot contain null entries."); reference.Validate(); var key = reference.SkillId + "\n" + (reference.Version.HasValue ? reference.Version.Value.ToString() : "latest"); if (!ids.Add(key)) throw new ArgumentException("Skill set references must be unique."); }
        }
    }

    public sealed class AiSkillBinding
    {
        public AiSkillReference Reference { get; set; }
        public AiSkillDefinition Definition { get; set; }

        public AiSkillBinding Clone()
        {
            return new AiSkillBinding
            {
                Reference = Reference == null ? null : Reference.Clone(),
                Definition = Definition == null ? null : Definition.Clone()
            };
        }

        public void Validate()
        {
            if (Reference == null) throw new ArgumentException("Skill binding reference is required.");
            if (Definition == null) throw new ArgumentException("Skill binding definition is required.");
            Reference.Validate();
            Definition.Validate();
            if (!string.Equals(Reference.SkillId, Definition.Id, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Skill binding reference and definition IDs must match.");
            if (Reference.Version.HasValue && Reference.Version.Value != Definition.Version) throw new ArgumentException("Skill binding reference version does not match definition version.");
            if (!Definition.IsAuthoritative) throw new ArgumentException("Only published skill definitions may be bound for execution.");
        }
    }

    public sealed class AiSkillExecutionSnapshot
    {
        public IReadOnlyList<AiSkillBinding> Bindings { get; private set; }

        public AiSkillExecutionSnapshot(IEnumerable<AiSkillBinding> bindings)
        {
            var clones = new List<AiSkillBinding>();
            foreach (var binding in bindings ?? Enumerable.Empty<AiSkillBinding>())
            {
                var clone = binding == null ? null : binding.Clone();
                if (clone == null) throw new ArgumentException("Skill execution snapshots cannot contain null bindings.", nameof(bindings));
                clone.Validate();
                clones.Add(clone);
            }
            Bindings = clones.AsReadOnly();
        }

        public AiSkillExecutionSnapshot Clone()
        {
            return new AiSkillExecutionSnapshot(Bindings);
        }
    }

    public interface IAiSkillDefinitionSource
    {
        Task<AiSkillDefinition> GetAsync(AiSkillReference reference, CancellationToken cancellationToken);
    }

    public sealed class AiGovernedSkillResolver
    {
        private readonly IAiSkillDefinitionSource _source;
        private readonly AiResourceGovernanceEvaluator _governance;

        public AiGovernedSkillResolver(IAiSkillDefinitionSource source, AiResourceGovernanceEvaluator governance)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _governance = governance ?? throw new ArgumentNullException(nameof(governance));
        }

        public async Task<AiSkillExecutionSnapshot> ResolveAsync(
            AiSkillSet skillSet,
            AgentIdentityContext identity,
            string agentProfileId,
            string runtimeInstanceId,
            string executionId,
            CancellationToken cancellationToken)
        {
            if (skillSet == null) throw new ArgumentNullException(nameof(skillSet));
            skillSet.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            var bindings = new List<AiSkillBinding>();
            foreach (var reference in skillSet.References)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var requested = new AiResourceGovernanceRequest
                {
                    Operation = "skill.invoke",
                    ResourceType = "skill",
                    ResourceId = reference.SkillId,
                    Scope = AgentResourceScope.Global,
                    AgentProfileId = agentProfileId ?? string.Empty,
                    RuntimeInstanceId = runtimeInstanceId ?? string.Empty,
                    ExecutionId = executionId ?? string.Empty,
                    Identity = identity == null ? new AgentIdentityContext() : identity.Clone()
                };
                var decision = _governance.Evaluate(requested);
                if (!decision.Allowed)
                {
                    if (reference.Required) throw new InvalidOperationException("Required skill was not admitted: " + reference.SkillId + ". " + decision.Reason);
                    continue;
                }

                var definition = await _source.GetAsync(reference.Clone(), cancellationToken).ConfigureAwait(false);
                if (definition == null)
                {
                    if (reference.Required) throw new InvalidOperationException("Required skill definition was not found: " + reference.SkillId + ".");
                    continue;
                }
                definition.Validate();
                if (!definition.IsAuthoritative)
                {
                    if (reference.Required) throw new InvalidOperationException("Required skill definition is not published: " + reference.SkillId + ".");
                    continue;
                }
                if (!string.Equals(definition.Id, reference.SkillId, StringComparison.OrdinalIgnoreCase) || (reference.Version.HasValue && definition.Version != reference.Version.Value))
                {
                    if (reference.Required) throw new InvalidOperationException("Resolved skill definition does not satisfy the requested reference: " + reference.SkillId + ".");
                    continue;
                }
                var binding = new AiSkillBinding { Reference = reference.Clone(), Definition = definition.Clone() };
                binding.Validate();
                bindings.Add(binding);
            }
            return new AiSkillExecutionSnapshot(bindings);
        }
    }
}
