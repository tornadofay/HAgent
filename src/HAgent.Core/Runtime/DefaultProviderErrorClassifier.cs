using System;
using System.Threading;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultProviderErrorClassifier : IProviderErrorClassifier
    {
        public ProviderErrorKind Classify(Exception exception)
        {
            if (exception == null) return ProviderErrorKind.Unknown;
            if (exception is OperationCanceledException) return ProviderErrorKind.Cancelled;
            if (exception is TimeoutException) return ProviderErrorKind.Transient;

            var typeName = exception.GetType().FullName ?? string.Empty;
            var message = exception.Message ?? string.Empty;

            if (ContainsAny(typeName, message, "HttpRequestException", "temporar", "timeout", "timed out", "connection reset", "connection refused"))
                return ProviderErrorKind.Transient;
            if (ContainsAny(typeName, message, "429", "rate limit", "too many requests"))
                return ProviderErrorKind.RateLimited;
            if (ContainsAny(typeName, message, "401", "403", "unauthorized", "forbidden", "authentication", "api key"))
                return ProviderErrorKind.Authentication;
            if (ContainsAny(typeName, message, "400", "404", "422", "invalid request", "bad request"))
                return ProviderErrorKind.InvalidRequest;
            if (ContainsAny(typeName, message, "502", "503", "504", "service unavailable", "gateway"))
                return ProviderErrorKind.Unavailable;

            return ProviderErrorKind.Unknown;
        }

        private static bool ContainsAny(string typeName, string message, params string[] values)
        {
            foreach (var value in values)
            {
                if (typeName.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    message.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
