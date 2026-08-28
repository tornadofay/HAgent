# Architecture

```text
                         +----------------------+
                         |   HAgent.WinForms    |
                         |  configuration UI    |
                         +----------+-----------+
                                    |
                                    v
+----------------+       +----------------------+       +---------------------------+
| File Storage   |------>|      HAgent.Core     |<------| OpenAI-Compatible Adapter |
| SQL Server     |       | models + abstractions|       | future provider adapters  |
| MySQL          |------>| runtime + sessions   |       +---------------------------+
+----------------+       +----------+-----------+
                                    |
                                    v
                              Application code
```

## Runtime flow

1. Application asks `HAgentClient` to send a message to an agent.
2. Core resolves the agent from `IAiStore`.
3. Core resolves the provider referenced by that agent.
4. Core resolves the secret through `ISecretStore`.
5. Core selects the provider adapter by `Kind`.
6. The adapter performs the provider-specific request.
7. Core returns a provider-neutral `AIResponse`.

## Prompt model

The effective system instruction is:

```text
Provider shared instruction

Agent instruction
```

unless the agent's `UseProviderSystemPrompt` is disabled.

This intentionally avoids three or four levels of inheritance. The user can understand exactly why text was sent to the provider.

## Why not put everything under the provider?

A provider is likely to serve many agents. Model selection and behavior often vary by task. Keeping those concerns separate prevents a shared connection definition from becoming a giant configuration object.

## Why not make every agent a provider client?

Because application code should depend on stable agent identity (`assistant`, `translator`, `report-writer`) rather than provider names (`openai`, `provider-2`, `local-llm`). This makes switching providers a configuration change instead of a code change.
