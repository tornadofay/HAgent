using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum WorkspaceParticipantKind
    {
        User,
        Agent
    }

    public enum WorkspaceParticipantState
    {
        Active,
        Suspended,
        Retired
    }

    public sealed class WorkspaceParticipant
    {
        public WorkspaceParticipant()
        {
            ParticipantId = string.Empty;
            DisplayName = string.Empty;
            State = WorkspaceParticipantState.Active;
        }

        public string ParticipantId { get; set; }
        public WorkspaceParticipantKind Kind { get; set; }
        public string DisplayName { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ProfileId { get; set; }
        public WorkspaceParticipantState State { get; set; }
    }

    public sealed class AgentWorkspace
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, WorkspaceParticipant> _participants =
            new Dictionary<string, WorkspaceParticipant>(StringComparer.OrdinalIgnoreCase);

        public AgentWorkspace(string workspaceId, string name)
        {
            if (string.IsNullOrWhiteSpace(workspaceId))
                throw new ArgumentException("Workspace ID is required.", nameof(workspaceId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Workspace name is required.", nameof(name));

            WorkspaceId = workspaceId.Trim();
            Name = name.Trim();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public string WorkspaceId { get; private set; }
        public string Name { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public string DefaultRecipientId { get; private set; }

        public IReadOnlyList<WorkspaceParticipant> GetParticipants()
        {
            lock (_sync)
            {
                return new List<WorkspaceParticipant>(_participants.Values).AsReadOnly();
            }
        }

        public void AddParticipant(WorkspaceParticipant participant, bool makeDefault = false)
        {
            if (participant == null) throw new ArgumentNullException(nameof(participant));
            if (string.IsNullOrWhiteSpace(participant.ParticipantId))
                throw new ArgumentException("Participant ID is required.", nameof(participant));

            lock (_sync)
            {
                _participants[participant.ParticipantId] = participant;
                if (makeDefault || string.IsNullOrWhiteSpace(DefaultRecipientId))
                    DefaultRecipientId = participant.ParticipantId;
            }
        }

        public bool RemoveParticipant(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return false;

            lock (_sync)
            {
                if (!_participants.Remove(participantId)) return false;
                if (string.Equals(DefaultRecipientId, participantId, StringComparison.OrdinalIgnoreCase))
                    DefaultRecipientId = FindNextActiveParticipantId();
                return true;
            }
        }

        public bool SetDefaultRecipient(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return false;

            lock (_sync)
            {
                WorkspaceParticipant participant;
                if (!_participants.TryGetValue(participantId, out participant)) return false;
                if (participant.State != WorkspaceParticipantState.Active) return false;
                DefaultRecipientId = participant.ParticipantId;
                return true;
            }
        }

        public bool TryGetParticipant(string participantId, out WorkspaceParticipant participant)
        {
            if (string.IsNullOrWhiteSpace(participantId))
            {
                participant = null;
                return false;
            }

            lock (_sync)
            {
                return _participants.TryGetValue(participantId, out participant);
            }
        }

        private string FindNextActiveParticipantId()
        {
            foreach (var participant in _participants.Values)
            {
                if (participant.State == WorkspaceParticipantState.Active)
                    return participant.ParticipantId;
            }

            return string.Empty;
        }
    }
}
