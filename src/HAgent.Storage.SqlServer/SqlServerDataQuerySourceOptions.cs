using System;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.SqlServer
{
    /// <summary>
    /// Runtime-only configuration for a restricted SQL Server structured-query source.
    /// The connection string is a runtime secret/connection concern and must not be persisted as agent or tool configuration.
    /// </summary>
    public sealed class SqlServerDataQuerySourceOptions
    {
        public string ConnectionString { get; set; }
        public string SourceId { get; set; }
        public string RuntimeIdentity { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public DataQuerySchema Schema { get; set; }
        public DataAccessPermissions Permissions { get; set; }
        public IDataAccessAuthorizer Authorizer { get; set; }
        public DataQueryExecutionPolicy ExecutionPolicy { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentException("Connection string is required.", nameof(ConnectionString));
            if (string.IsNullOrWhiteSpace(SourceId))
                throw new ArgumentException("Source ID is required.", nameof(SourceId));
            if (string.IsNullOrWhiteSpace(RuntimeIdentity))
                throw new ArgumentException("Runtime identity is required.", nameof(RuntimeIdentity));
            ValidateIdentifier(SchemaName, nameof(SchemaName));
            ValidateIdentifier(TableName, nameof(TableName));
            if (Schema == null) throw new ArgumentNullException(nameof(Schema));
            if (Permissions == null) throw new ArgumentNullException(nameof(Permissions));
            if (Authorizer == null) throw new ArgumentNullException(nameof(Authorizer));
            if (ExecutionPolicy == null) throw new ArgumentNullException(nameof(ExecutionPolicy));
        }

        internal static string QuoteIdentifier(string identifier)
        {
            ValidateIdentifier(identifier, nameof(identifier));
            return "[" + identifier + "]";
        }

        private static void ValidateIdentifier(string identifier, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier is required.", parameterName);

            if (!(char.IsLetter(identifier[0]) || identifier[0] == '_'))
                throw new ArgumentException("Identifier must start with a letter or underscore.", parameterName);

            for (var i = 1; i < identifier.Length; i++)
            {
                var character = identifier[i];
                if (!(char.IsLetterOrDigit(character) || character == '_'))
                    throw new ArgumentException("Identifier may contain only letters, digits, and underscores.", parameterName);
            }
        }
    }
}
