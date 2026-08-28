using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.File
{
    public sealed class FileAiStore : IAiStore
    {
        private sealed class Data
        {
            public List<AiProvider> Providers { get; set; } = new List<AiProvider>();
            public List<AiAgent> Agents { get; set; } = new List<AiAgent>();
        }

        private readonly string _path;
        private readonly object _sync = new object();
        private Data _data;
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public FileAiStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));
            _path = path;
            _data = Load();
        }

        public Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) return Task.FromResult<IReadOnlyList<AiProvider>>(_data.Providers.ToList().AsReadOnly());
        }

        public Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) return Task.FromResult<IReadOnlyList<AiAgent>>(_data.Agents.ToList().AsReadOnly());
        }

        public Task SaveProviderAsync(AiProvider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync)
            {
                var index = _data.Providers.FindIndex(x => x.Id == provider.Id);
                if (index >= 0) _data.Providers[index] = provider;
                else _data.Providers.Add(provider);
                Persist();
            }
            return Task.CompletedTask;
        }

        public Task SaveAgentAsync(AiAgent agent, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync)
            {
                var index = _data.Agents.FindIndex(x => x.Id == agent.Id);
                if (index >= 0) _data.Agents[index] = agent;
                else _data.Agents.Add(agent);
                Persist();
            }
            return Task.CompletedTask;
        }

        public Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync)
            {
                _data.Agents.RemoveAll(x => x.ProviderId == providerId);
                _data.Providers.RemoveAll(x => x.Id == providerId);
                Persist();
            }
            return Task.CompletedTask;
        }

        public Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync)
            {
                _data.Agents.RemoveAll(x => x.Id == agentId);
                Persist();
            }
            return Task.CompletedTask;
        }

        private Data Load()
        {
            try
            {
                if (!System.IO.File.Exists(_path)) return new Data();
                var json = System.IO.File.ReadAllText(_path);
                return JsonSerializer.Deserialize<Data>(json, Options) ?? new Data();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("HAgent settings file could not be read. The file was not overwritten.", ex);
            }
        }

        private void Persist()
        {
            var folder = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            var temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(_data, Options));
            if (System.IO.File.Exists(_path)) System.IO.File.Replace(temp, _path, _path + ".bak", true);
            else System.IO.File.Move(temp, _path);
        }
    }
}
