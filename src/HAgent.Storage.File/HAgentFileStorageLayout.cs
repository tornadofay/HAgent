using System;
using System.IO;
using HAgent.Models;

namespace HAgent.Storage.File
{
    /// <summary>
    /// Creates the application-specific on-disk layout used by HAgent's File storage backend.
    /// </summary>
    public sealed class HAgentFileStorageLayout
    {
        public HAgentFileStorageLayout(HAgentStorageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.StorageType != HAgentStorageType.File)
                throw new ArgumentException("The storage options must use the File backend.", nameof(options));
            options.Validate();
            RootPath = options.GetEffectiveRootPath();
        }

        public string RootPath { get; private set; }
        public string ConfigurationPath { get { return Path.Combine(RootPath, "configuration"); } }
        public string ProvidersPath { get { return Path.Combine(ConfigurationPath, "providers"); } }
        public string AgentsPath { get { return Path.Combine(ConfigurationPath, "agents"); } }
        public string ToolsPath { get { return Path.Combine(ConfigurationPath, "tools"); } }
        public string SkillsPath { get { return Path.Combine(ConfigurationPath, "skills"); } }
        public string MemoryPath { get { return Path.Combine(RootPath, "memory"); } }
        public string ConversationsPath { get { return Path.Combine(RootPath, "conversations"); } }
        public string WikiPath { get { return Path.Combine(RootPath, "wiki"); } }
        public string RuntimePath { get { return Path.Combine(RootPath, "runtime"); } }
        public string CachePath { get { return Path.Combine(RootPath, "cache"); } }
        public string LogsPath { get { return Path.Combine(RootPath, "logs"); } }

        public void EnsureCreated()
        {
            Directory.CreateDirectory(RootPath);
            Directory.CreateDirectory(ConfigurationPath);
            Directory.CreateDirectory(ProvidersPath);
            Directory.CreateDirectory(AgentsPath);
            Directory.CreateDirectory(ToolsPath);
            Directory.CreateDirectory(SkillsPath);
            Directory.CreateDirectory(MemoryPath);
            Directory.CreateDirectory(ConversationsPath);
            Directory.CreateDirectory(WikiPath);
            Directory.CreateDirectory(RuntimePath);
            Directory.CreateDirectory(CachePath);
            Directory.CreateDirectory(LogsPath);
        }
    }
}
