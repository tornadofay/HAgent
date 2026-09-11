using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Prepares authoritative learned-resource inputs for an execution by reusing the canonical
    /// context admission/assembly pipeline and canonical instruction-source contracts.
    /// This class does not authorize through prompt text and does not mutate authoritative resources.
    /// </summary>
    public sealed class AiLearningExecutionPreparation
    {
        private readonly IContextAssembler _contextAssembler;

        public AiLearningExecutionPreparation(IContextAssembler contextAssembler)
        {
            _contextAssembler = contextAssembler ?? throw new ArgumentNullException(nameof(contextAssembler));
        }

        public async Task<AiLearningExecutionPreparationResult> PrepareAsync(
            IReadOnlyList<AiInstructionSource> learnedInstructionSources,
            IReadOnlyList<ContextRetrievalSource> learnedContextSources,
            ContextBudget contextBudget,
            ContextAdmissionContext admissionContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (learnedInstructionSources == null) throw new ArgumentNullException(nameof(learnedInstructionSources));
            if (learnedContextSources == null) throw new ArgumentNullException(nameof(learnedContextSources));
            if (contextBudget == null) throw new ArgumentNullException(nameof(contextBudget));
            if (admissionContext == null) throw new ArgumentNullException(nameof(admissionContext));

            contextBudget.Validate();
            admissionContext.Validate();

            var instructionCopies = new List<AiInstructionSource>(learnedInstructionSources.Count);
            foreach (var source in learnedInstructionSources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (source == null)
                    throw new ArgumentException("Learned instruction sources cannot contain null values.", nameof(learnedInstructionSources));
                source.Validate();
                instructionCopies.Add(source.Clone());
            }

            ContextAssemblyResult contextResult = null;
            if (learnedContextSources.Count > 0)
            {
                contextResult = await _contextAssembler.AssembleAsync(
                    learnedContextSources,
                    contextBudget.Clone(),
                    admissionContext.Clone(),
                    cancellationToken).ConfigureAwait(false);
            }

            return new AiLearningExecutionPreparationResult(
                instructionCopies,
                contextResult == null ? null : contextResult.Snapshot.Clone(),
                contextResult == null ? null : contextResult.AdmissionDecisions,
                contextResult == null ? null : contextResult.Compaction);
        }
    }

    public sealed class AiLearningExecutionPreparationResult
    {
        public AiLearningExecutionPreparationResult(
            IReadOnlyList<AiInstructionSource> instructionSources,
            ContextSnapshot context,
            IReadOnlyList<ContextAdmissionDecision> admissionDecisions,
            ContextCompactionResult compaction)
        {
            InstructionSources = instructionSources ?? throw new ArgumentNullException(nameof(instructionSources));
            Context = context;
            AdmissionDecisions = admissionDecisions;
            Compaction = compaction;
        }

        public IReadOnlyList<AiInstructionSource> InstructionSources { get; private set; }
        public ContextSnapshot Context { get; private set; }
        public IReadOnlyList<ContextAdmissionDecision> AdmissionDecisions { get; private set; }
        public ContextCompactionResult Compaction { get; private set; }
    }
}
