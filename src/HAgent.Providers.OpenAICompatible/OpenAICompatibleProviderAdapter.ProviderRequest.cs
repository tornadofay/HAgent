using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Providers.OpenAICompatible
{
    public sealed partial class OpenAICompatibleProviderAdapter
    {
        public Task<AIResponse> SendAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            return SendWithStructuredOutputAsync(request, cancellationToken);
        }

        public Task<AIResponse> SendWithToolsAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            return SendWithToolsAsync(
                request.Provider,
                request.Agent,
                request.ApiKey,
                request.SystemPrompt,
                request.Messages,
                request.Tools,
                cancellationToken);
        }

        public Task<AIResponse> SendStreamingAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Validate();
            return SendStreamingAsync(
                request.Provider,
                request.Agent,
                request.ApiKey,
                request.SystemPrompt,
                request.Messages,
                request.Progress,
                cancellationToken);
        }
    }
}
