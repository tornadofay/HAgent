using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal static class ProviderErrorAdvisor
    {
        public static ProviderErrorKind InferKind(Exception exception)
        {
            if (exception == null) return ProviderErrorKind.Unknown;

            var message = exception.Message ?? string.Empty;
            if (Contains(message, "model_terms_required") || Contains(message, "requires terms acceptance"))
                return ProviderErrorKind.ModelTermsRequired;
            if (Contains(message, "permission_denied") || Contains(message, "permission denied") || Contains(message, "access denied"))
                return ProviderErrorKind.PermissionDenied;
            if (Contains(message, "model_not_found") || Contains(message, "model not found") || Contains(message, "unknown model"))
                return ProviderErrorKind.ModelNotFound;
            if (Contains(message, "429") || Contains(message, "rate limit") || Contains(message, "too many requests"))
                return ProviderErrorKind.RateLimited;
            if (Contains(message, "401") || Contains(message, "unauthorized") || Contains(message, "authentication") || Contains(message, "api key"))
                return ProviderErrorKind.Authentication;

            return ProviderErrorKind.Unknown;
        }

        public static string GetActionableMessage(ProviderErrorKind kind, string providerName, string model, string originalMessage)
        {
            var provider = string.IsNullOrWhiteSpace(providerName) ? "The provider" : providerName;
            var selectedModel = string.IsNullOrWhiteSpace(model) ? "the selected model" : model;

            switch (kind)
            {
                case ProviderErrorKind.ModelTermsRequired:
                    return provider + " requires terms acceptance before " + selectedModel + " can be used. Accept the model terms in the provider console, then test again.\r\n\r\nProvider detail:\r\n" + (originalMessage ?? string.Empty);
                case ProviderErrorKind.PermissionDenied:
                    return provider + " denied access to " + selectedModel + ". Check account, organization, project, or model permissions.\r\n\r\nProvider detail:\r\n" + (originalMessage ?? string.Empty);
                case ProviderErrorKind.ModelNotFound:
                    return selectedModel + " was not found by " + provider + ". Refresh the model list or verify the model identifier.\r\n\r\nProvider detail:\r\n" + (originalMessage ?? string.Empty);
                case ProviderErrorKind.Authentication:
                    return provider + " rejected authentication. Check the API key, secret, or credential configuration.\r\n\r\nProvider detail:\r\n" + (originalMessage ?? string.Empty);
                case ProviderErrorKind.RateLimited:
                    return provider + " rate-limited the request. HAgent will use its configured retry policy where applicable.\r\n\r\nProvider detail:\r\n" + (originalMessage ?? string.Empty);
                default:
                    return originalMessage ?? string.Empty;
            }
        }

        private static bool Contains(string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
