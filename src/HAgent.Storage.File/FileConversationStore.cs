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
    /// <summary>
    /// Lightweight persistent conversation store.
    /// Each session is stored in its own JSON file so opening one conversation does not load unrelated history.
    /// </summary>
    public sealed class FileConversationStore : IConversationStore, IDisposable
    {
        private readonly string _directory;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public FileConversationStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Conversation directory is required.", nameof(directory));

            _directory = directory;
            System.IO.Directory.CreateDirectory(_directory);
        }

        public string DirectoryPath { get { return _directory; } }

        public async Task SaveAsync(ConversationSnapshot conversation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (conversation == null) throw new ArgumentNullException(nameof(conversation));
            if (string.IsNullOrWhiteSpace(conversation.SessionId)) throw new ArgumentException("Session ID is required.", nameof(conversation));

            if (conversation.Messages == null)
                conversation.Messages = new List<AIMessage>();
            if (conversation.CreatedAt == default(DateTimeOffset))
                conversation.CreatedAt = DateTimeOffset.UtcNow;
            conversation.UpdatedAt = DateTimeOffset.UtcNow;

            var path = GetPath(conversation.SessionId);
            var tempPath = path + ".tmp";
            var json = JsonSerializer.Serialize(conversation, _jsonOptions);

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json).ConfigureAwait(false);
                }

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                System.IO.File.Move(tempPath, path);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
                _gate.Release();
            }
        }

        public async Task<ConversationSnapshot> LoadAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session ID is required.", nameof(sessionId));
            var path = GetPath(sessionId);
            if (!System.IO.File.Exists(path)) return null;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream))
                {
                    var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(json)) return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<ConversationSnapshot>(json, _jsonOptions);
                        if (result != null && result.Messages == null)
                            result.Messages = new List<AIMessage>();
                        return result;
                    }
                    catch (JsonException)
                    {
                        return null;
                    }
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            var path = GetPath(sessionId);

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            finally
            {
                _gate.Release();
            }
        }

        private string GetPath(string sessionId)
        {
            var safe = new string(sessionId.Where(c =>
                char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.').ToArray());
            if (string.IsNullOrWhiteSpace(safe))
                throw new ArgumentException("Session ID contains no valid filename characters.", nameof(sessionId));
            return System.IO.Path.Combine(_directory, safe + ".json");
        }

        public void Dispose()
        {
            _gate.Dispose();
        }
    }
}
