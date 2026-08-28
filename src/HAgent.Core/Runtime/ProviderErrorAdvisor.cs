using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal static class ProviderErrorAdvisor
    {
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
    }
}
