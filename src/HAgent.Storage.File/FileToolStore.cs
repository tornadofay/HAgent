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
    public sealed class FileToolStore : IToolStore, IDisposable
    {
        private sealed class Data
        {
            public List<AiTool> Tools { get; set; } = new List<AiTool>();
        }

        private readonly string _path;
        private readonly object _sync = new object();
        private Data _data;
        private bool _disposed;
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public FileToolStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));
            _path = path;
            _data = Load();
        }

        public Task<IReadOnlyList<AiTool>> GetToolsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            lock (_sync)
            {
                return Task.FromResult<IReadOnlyList<AiTool>>(_data.Tools.Select(Clone).ToList().AsReadOnly());
            }
        }

        public Task SaveToolAsync(AiTool tool, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (string.IsNullOrWhiteSpace(tool.Id)) throw new ArgumentException("Tool ID is required.", nameof(tool));
            lock (_sync)
            {
                var index = _data.Tools.FindIndex(x => string.Equals(x.Id, tool.Id, StringComparison.OrdinalIgnoreCase));
                var copy = Clone(tool);
                if (index >= 0) _data.Tools[index] = copy;
                else _data.Tools.Add(copy);
                Persist();
            }
            return Task.CompletedTask;
        }

        public Task DeleteToolAsync(string toolId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(toolId)) return Task.CompletedTask;
            lock (_sync)
            {
                _data.Tools.RemoveAll(x => string.Equals(x.Id, toolId, StringComparison.OrdinalIgnoreCase));
                Persist();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
                _data = new Data();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileToolStore));
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
                throw new InvalidDataException("HAgent tool store could not be read. The file was not overwritten.", ex);
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

        private static AiTool Clone(AiTool source)
        {
            return new AiTool
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                InputSchemaJson = source.InputSchemaJson,
                Category = source.Category,
                Type = source.Type,
                IsBuiltIn = source.IsBuiltIn,
                Enabled = source.Enabled
            };
        }
    }
}
