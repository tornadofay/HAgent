using System;

namespace HAgent.Example
{
    internal static class ExampleCodeSnippets
    {
        public static string Get(string title)
        {
            switch (title)
            {
                case "Messaging": return "var response = await ai.SendAsync(\n    agentId: \"assistant\",\n    message: \"Reply with exactly MESSAGE-OK and nothing else.\");\n\nConsole.WriteLine(response.Text);";
                case "Session": return "var session = ai.CreateSession(\"assistant\", \"conversation-42\");\nawait session.SendAsync(\"Store this temporary test value in our conversation: HAgent-session-42.\");\nvar response = await session.SendAsync(\"What temporary test value did I just give you? Reply with only the value.\");";
                case "Persistent Session": return "var session = ai.CreateSession(\"assistant\", \"conversation-42\");\nawait session.SendAsync(\"Store this persistent test value in this conversation: HAgent-persist-42.\");\n\nvar reopened = await ai.OpenSessionAsync(\"assistant\", \"conversation-42\");\nvar history = await reopened.ReadAsync();";
                case "Runtime 0.2": return "var execution = await ai.ExecuteAsync(\"assistant\",\n    \"Reply with the word RUNTIME-OK and nothing else.\",\n    new AgentExecutionOptions\n    {\n        Timeout = TimeSpan.FromSeconds(60),\n        MaxProviderAttempts = 3,\n        MaxRetriesPerProvider = 1\n    });";
                case "Configuration": return "var settings = await store.GetProvidersAsync();\nvar agents = await store.GetAgentsAsync();\n\nforeach (var provider in settings)\n    Console.WriteLine(provider.Name);";
                case "Memory": return "await ai.RememberAsync(\n    \"assistant\",\n    \"HAgent-memory-42\");\n\nvar memories = await ai.RecallAsync(\"assistant\", \"HAgent-memory-42\");\nawait ai.ForgetAsync(memories[0].Id);";
                case "Automatic Memory": return "var policy = new ExplicitConversationMemoryPolicy(memoryStore);\nawait policy.ProcessAsync(\n    agentId: \"assistant\",\n    sessionId: \"session-42\",\n    userMessage: \"Remember this: HAgent-auto-memory-42.\",\n    assistantMessage: \"Confirmed.\");";
                case "Context Budget": return "var builder = new ContextBuilder(new ContextBudgetOptions\n{\n    MaxMessages = 10,\n    MaxCharacters = 7000\n});\n\nvar bounded = builder.Build(messages);";
                case "Task / Event Memory": return "var taskId = \"task-42\";\nawait memoryStore.SaveAsync(new MemoryRecord\n{\n    Id = Guid.NewGuid().ToString(),\n    Scope = MemoryScope.Task,\n    Kind = MemoryKind.Task,\n    OwnerId = taskId,\n    Content = \"Import customers from the selected form.\"\n});";
                case "Episodic Memory": return "await memoryStore.SaveAsync(new EpisodicMemory\n{\n    Id = Guid.NewGuid().ToString(),\n    TaskId = \"task-42\",\n    SessionId = \"session-42\",\n    Title = \"Customer import\",\n    Summary = \"1,284 customers imported successfully; 3 records were rejected.\",\n    Outcome = \"Completed successfully\"\n});";
                case "Capabilities": return "var capabilities = await providerAdapter.GetModelCapabilitiesAsync(\n    provider, agent.Model, CancellationToken.None);\n\nConsole.WriteLine(capabilities.ToolCalling);\nConsole.WriteLine(capabilities.Streaming);";
                case "Response Normalization": return "var response = AIResponse.FromProvider(\n    text: \"Customer data is ready.\",\n    reasoning: \"Provider-side reasoning metadata.\",\n    structuredOutputJson: \"{\\\"customerId\\\":42}\",\n    toolCalls: calls,\n    requestId: \"request-42\");";
                case "Streaming": return "await ai.StreamAsync(\n    agentId: \"assistant\",\n    message: \"Explain why streaming is useful.\",\n    onDelta: delta => Console.Write(delta.Text));";
                case "Live Streaming": return "await ai.StreamAsync(\n    agentId: \"assistant\",\n    message: \"Explain why streaming responses are useful in a desktop AI assistant.\",\n    progress: new Progress<AIResponseDelta>(delta =>\n    {\n        Console.Write(delta.Text);\n    }));";
                case "Tool Registry": return "var definition = new AiTool\n{\n    Id = \"example.echo\",\n    Name = \"Example Echo\",\n    Description = \"Returns the supplied value.\",\n    InputSchemaJson = \"{...}\",\n    Type = AiToolType.Application,\n    Enabled = true\n};\n\nclient.RegisterTool(new DelegateAgentTool(\n    definition,\n    context => Task.FromResult(\n        ToolExecutionResult.Success(Convert.ToString(context.Arguments[\"value\"])))));";
                case "Provider Tool Transport": return "var response = await adapter.SendWithToolsAsync(\n    provider,\n    agent,\n    providerSystemPrompt,\n    agentSystemPrompt,\n    messages,\n    new[] { tool },\n    CancellationToken.None);\n\nConsole.WriteLine(response.ToolCalls[0].Name);";
                case "Tool Loop": return "client.RegisterTool(exampleAddTool);\n\nvar result = await client.RunToolLoopAsync(\n    agentId: \"assistant\",\n    message: \"Use the add tool.\",\n    maxTurns: 4,\n    maxToolCalls: 4,\n    cancellationToken: CancellationToken.None);\n\nConsole.WriteLine(result.Response.Text);";
                case "Live Tool Loop": return "client.RegisterTool(new DelegateAgentTool(\n    exampleAddDefinition,\n    context => Task.FromResult(\n        ToolExecutionResult.Success(\n            (Convert.ToInt32(context.Arguments[\"a\"]) +\n             Convert.ToInt32(context.Arguments[\"b\"])).ToString()))));\n\nvar result = await client.RunToolLoopAsync(\n    selectedAgent.Id,\n    \"Use example_add with a=3 and b=4, then report the result.\",\n    4, 4, CancellationToken.None);";
                case "Tool Persistence": return "var store = new FileToolStore(\"tool-definitions\\tools.json\");\nawait store.SaveToolAsync(tool, CancellationToken.None);\n\nvar reopened = new FileToolStore(\"tool-definitions\\tools.json\");\nvar tools = await reopened.GetToolsAsync(CancellationToken.None);";
                case "Agent Tool Assignment": return "agent.ToolIds = new List<string>\n{\n    \"example.assignment.tool\"\n};\n\nawait aiStore.SaveAgentAsync(agent, CancellationToken.None);";
                case "UI Context": return "using (var host = HAgentHost.Attach(form, registry))\n{\n    var snapshot = await host.Context.InspectAsync();\n    var name = await host.Context.ReadControlAsync(\"txtCustomerName\");\n    var rows = await host.Context.ReadDataAsync(\"gridCustomers\", 100);\n}";
                default: return "// See the corresponding HAgent example source file.\n// The Example application uses the public HAgent API shown here as the reference pattern.";
            }
        }
    }
}
