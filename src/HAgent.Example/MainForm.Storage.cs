using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Storage.File;
using HAgent.Storage.MySql;
using HAgent.Storage.SqlServer;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private string StorageConfigurationPath
        {
            get { return Path.Combine(_basePath, "configuration", "storage.json"); }
        }

        private async Task<HAgentStorageOptions> LoadStorageOptionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var store = new FileHAgentStorageConfigurationStore(StorageConfigurationPath);
            var options = await store.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (options == null)
            {
                options = new HAgentStorageOptions
                {
                    StorageType = HAgentStorageType.File,
                    ApplicationName = ProcessApplicationName,
                    RootPath = AppContext.BaseDirectory
                };
            }

            options.Validate();
            return options;
        }

        private string ProcessApplicationName
        {
            get
            {
                var name = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                return string.IsNullOrWhiteSpace(name) ? "HAgent" : name;
            }
        }

        private async Task<string> LoadDatabasePasswordAsync(HAgentStorageOptions options, CancellationToken cancellationToken)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.PasswordSecretId)) return string.Empty;
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            return await secrets.GetAsync(options.PasswordSecretId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IAiStore> CreateConfiguredAiStoreAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = await LoadStorageOptionsAsync(cancellationToken).ConfigureAwait(false);
            switch (options.StorageType)
            {
                case HAgentStorageType.File:
                    return new FileAiStore(Path.Combine(options.GetEffectiveRootPath(), "configuration", "settings.json"));

                case HAgentStorageType.SqlServer:
                {
                    var password = await LoadDatabasePasswordAsync(options, cancellationToken).ConfigureAwait(false);
                    var bootstrapper = new SqlServerHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password, cancellationToken).ConfigureAwait(false);
                    var connectionString = SqlServerHAgentStorageBootstrapper.BuildConnectionString(
                        options.ServerName,
                        options.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    return new SqlServerAiStore(connectionString);
                }

                case HAgentStorageType.MySql:
                {
                    var password = await LoadDatabasePasswordAsync(options, cancellationToken).ConfigureAwait(false);
                    var bootstrapper = new MySqlHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password, cancellationToken).ConfigureAwait(false);
                    var connectionString = MySqlHAgentStorageBootstrapper.BuildConnectionString(
                        options.ServerName,
                        options.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    return new MySqlAiStore(connectionString);
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(options.StorageType), "Unsupported HAgent storage type.");
            }
        }

        private async Task<IToolStore> CreateConfiguredToolStoreAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = await LoadStorageOptionsAsync(cancellationToken).ConfigureAwait(false);
            switch (options.StorageType)
            {
                case HAgentStorageType.File:
                    return new FileToolStore(Path.Combine(options.GetEffectiveRootPath(), "configuration", "tools", "tools.json"));

                case HAgentStorageType.SqlServer:
                {
                    var password = await LoadDatabasePasswordAsync(options, cancellationToken).ConfigureAwait(false);
                    var bootstrapper = new SqlServerHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password, cancellationToken).ConfigureAwait(false);
                    var connectionString = SqlServerHAgentStorageBootstrapper.BuildConnectionString(
                        options.ServerName,
                        options.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    await SqlServerToolStore.EnsureSchemaAsync(connectionString, cancellationToken).ConfigureAwait(false);
                    return new SqlServerToolStore(connectionString);
                }

                case HAgentStorageType.MySql:
                {
                    var password = await LoadDatabasePasswordAsync(options, cancellationToken).ConfigureAwait(false);
                    var bootstrapper = new MySqlHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password, cancellationToken).ConfigureAwait(false);
                    var connectionString = MySqlHAgentStorageBootstrapper.BuildConnectionString(
                        options.ServerName,
                        options.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    await MySqlToolStore.EnsureSchemaAsync(connectionString, cancellationToken).ConfigureAwait(false);
                    return new MySqlToolStore(connectionString);
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(options.StorageType), "Unsupported HAgent storage type.");
            }
        }
    }
}
