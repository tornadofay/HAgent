using System;

namespace HAgent.Example
{
    internal static class ExampleCodeSnippets
    {
        public static string Get(string title)
        {
            switch (title)
            {
                case "Messaging": return @"var response = await ai.SendAsync(
    agentId: ""assistant"",
    message: ""Reply with exactly MESSAGE-OK and nothing else."" );

Console.WriteLine(response.Text);";
                case "Session": return @"var session = ai.CreateSession(""assistant"", ""conversation-42"");
await session.SendAsync(""Store this temporary test value in our conversation: HAgent-session-42."");

var response = await session.SendAsync(
    ""What temporary test value did I just give you? Reply with only the value."");";
                case "Persistent Session": return @"var session = ai.CreateSession(""assistant"", ""conversation-42"");
await session.SendAsync(""Store this persistent test value in this conversation: HAgent-persist-42."");

var reopened = await ai.OpenSessionAsync(""assistant"", ""conversation-42"");
var history = await reopened.ReadAsync();";
                case "Runtime Execution": return @"var execution = await ai.ExecuteAsync(
    ""assistant"",
    ""Reply with the word RUNTIME-OK and nothing else."",
    new AgentExecutionOptions
    {
        Timeout = TimeSpan.FromSeconds(60),
        MaxProviderAttempts = 3,
        MaxRetriesPerProvider = 1
    });";
                case "Configuration": return @"var providers = await store.GetProvidersAsync();
var agents = await store.GetAgentsAsync();

foreach (var provider in providers)
    Console.WriteLine(provider.Name);";
                case "Memory": return @"await ai.RememberAsync(
    ""assistant"",
    ""HAgent-memory-42"");

var memories = await ai.RecallAsync(""assistant"", ""HAgent-memory-42"");
await ai.ForgetAsync(memories[0].Id);";
                case "Automatic Memory": return @"var policy = new ExplicitConversationMemoryPolicy(memoryStore);
await policy.ProcessAsync(
    agentId: ""assistant"",
    sessionId: ""session-42"",
    userMessage: ""Remember this: HAgent-auto-memory-42."",
    assistantMessage: ""Confirmed."" );";
                case "Context Budget": return @"var builder = new ContextBuilder(new ContextBudgetOptions
{
    MaxMessages = 10,
    MaxCharacters = 7000
});

var bounded = builder.Build(messages);";
                case "Task / Event Memory": return @"var taskId = ""task-42"";
await memoryStore.SaveAsync(new MemoryRecord
{
    Id = Guid.NewGuid().ToString(),
    Scope = MemoryScope.Task,
    Kind = MemoryKind.Task,
    OwnerId = taskId,
    Content = ""Import customers from the selected form.""
});";
                case "Episodic Memory": return @"await memoryStore.SaveAsync(new EpisodicMemory
{
    Id = Guid.NewGuid().ToString(),
    TaskId = ""task-42"",
    SessionId = ""session-42"",
    Title = ""Customer import"",
    Summary = ""1,284 customers imported successfully; 3 records were rejected."",
    Outcome = ""Completed successfully""
});";
                case "Capabilities": return @"var capabilities = await providerAdapter.GetModelCapabilitiesAsync(
    provider, agent.Model, CancellationToken.None);

Console.WriteLine(capabilities.ToolCalling);
Console.WriteLine(capabilities.Streaming);";
                case "Response Normalization": return @"var response = AIResponse.FromProvider(
    text: ""Customer data is ready."",
    reasoning: ""Provider-side reasoning metadata."",
    structuredOutputJson: ""{\""customerId\"":42}"",
    toolCalls: calls,
    requestId: ""request-42"");";
                case "Streaming": return @"await ai.StreamAsync(
    agentId: ""assistant"",
    message: ""Explain why streaming is useful."",
    onDelta: delta => Console.Write(delta.Text));";
                case "Live Streaming": return @"await ai.StreamAsync(
    agentId: ""assistant"",
    message: ""Explain why streaming responses are useful in a desktop AI assistant."",
    progress: new Progress<AIResponseDelta>(delta =>
    {
        Console.Write(delta.Text);
    }));";
                case "Tool Registry": return @"var definition = new AiTool
{
    Id = ""example.echo"",
    Name = ""Example Echo"",
    Description = ""Returns the supplied value."",
    InputSchemaJson = ""{...}"",
    Type = AiToolType.Application,
    Enabled = true
};

client.RegisterTool(new DelegateAgentTool(
    definition,
    context => Task.FromResult(
        ToolExecutionResult.Success(Convert.ToString(context.Arguments[""value""])) )));";
                case "Provider Tool Transport": return @"var response = await adapter.SendWithToolsAsync(
    provider,
    agent,
    providerSystemPrompt,
    agentSystemPrompt,
    messages,
    new[] { tool },
    CancellationToken.None);

Console.WriteLine(response.ToolCalls[0].Name);";
                case "Tool Loop": return @"client.RegisterTool(exampleAddTool);

var result = await client.RunToolLoopAsync(
    agentId: ""assistant"",
    message: ""Use the add tool."",
    maxTurns: 4,
    maxToolCalls: 4,
    cancellationToken: CancellationToken.None);

Console.WriteLine(result.Response.Text);";
                case "Live Tool Loop": return @"client.RegisterTool(new DelegateAgentTool(
    exampleAddDefinition,
    context => Task.FromResult(
        ToolExecutionResult.Success(
            (Convert.ToInt32(context.Arguments[""a""]) +
             Convert.ToInt32(context.Arguments[""b""])).ToString()))));

var result = await client.RunToolLoopAsync(
    selectedAgent.Id,
    ""Use example_add with a=3 and b=4, then report the result."",
    4, 4, CancellationToken.None);";
                case "Tool Persistence": return @"var store = new FileToolStore(""tool-definitions\tools.json"");
await store.SaveToolAsync(tool, CancellationToken.None);

var reopened = new FileToolStore(""tool-definitions\tools.json"");
var tools = await reopened.GetToolsAsync(CancellationToken.None);";
                case "Agent Tool Assignment": return @"agent.ToolIds = new List<string>
{
    ""example.assignment.tool""
};

await aiStore.SaveAgentAsync(agent, CancellationToken.None);";
                case "UI Context": return @"using (var host = HAgentHost.Attach(form, registry))
{
    var snapshot = await host.Context.InspectAsync();
    var name = await host.Context.ReadControlAsync(""txtCustomerName"");
    var rows = await host.Context.ReadDataAsync(""gridCustomers"", 100);
}";
                case "UI Context UserControl": return @"using System.Windows.Forms;
using HAgent.WinForms.UI;

var panel = new UserControl { Name = ""CustomerPanel"" };
var registry = new InMemoryToolRegistry();

using (var host = HAgentHost.Attach(
    panel,
    rootId: ""CustomerPanel"",
    registry: registry,
    registerUiTools: true,
    permissions: new UiAutomationPermissions
    {
        AutomaticDiscovery = true,
        ReadControls = true,
        ReadData = true
    }))
{
    var context = host.Context;
    var snapshot = await context.InspectAsync();
    var value = await context.ReadControlAsync(""txtCustomerName"");
    var rows = await context.ReadDataAsync(""gridCustomers"", 100);

    Console.WriteLine(context.RootId);
    Console.WriteLine(snapshot.Id);
}";
                case "UI Native IList": return @"using System.Collections.Generic;
using System.Windows.Forms;
using HAgent.WinForms.UI;

var panel = new UserControl { Name = ""NativeListPanel"" };
var grid = new DataGridView { Name = ""gridNativeCustomers"" };
var customers = new List<NativeCustomer>
{
    new NativeCustomer { Id = 20, Name = ""Native Alice"" },
    new NativeCustomer { Id = 21, Name = ""Native Bob"" }
};

grid.DataSource = customers;
panel.Controls.Add(grid);

var permissions = new UiAutomationPermissions
{
    AutomaticDiscovery = true,
    ReadControls = true,
    ReadData = true
};

var sources = new WinFormsDataSourceDiscovery().Discover(panel, permissions);
var source = sources[0];

Console.WriteLine(source.SourceKind);
Console.WriteLine(source.CurrencyManagerType);
Console.WriteLine(source.CurrentItemType);
Console.WriteLine(source.ItemType);
Console.WriteLine(source.Position + "" / "" + source.Count);";
                case "UI Data Relationships": return @"using System.Collections.Generic;
using System.Windows.Forms;
using HAgent.WinForms.UI;

var panel = new UserControl { Name = ""RelationshipPanel"" };
var idBox = new TextBox { Name = ""txtCustomerId"" };
var nameBox = new TextBox { Name = ""txtCustomerName"" };
var grid = new DataGridView { Name = ""gridCustomers"" };
var bindingSource = new BindingSource();

bindingSource.DataSource = new List<Customer>
{
    new Customer { Id = 30, Name = ""Relationship Alice"" },
    new Customer { Id = 31, Name = ""Relationship Bob"" }
};

idBox.DataBindings.Add(""Text"", bindingSource, ""Id"");
nameBox.DataBindings.Add(""Text"", bindingSource, ""Name"");
grid.DataSource = bindingSource;

panel.Controls.Add(idBox);
panel.Controls.Add(nameBox);
panel.Controls.Add(grid);

var permissions = new UiAutomationPermissions
{
    AutomaticDiscovery = true,
    ReadControls = true,
    ReadData = true
};

var relationships = new WinFormsDataRelationshipDiscovery()
    .Discover(panel, permissions);

foreach (var relationship in relationships)
    Console.WriteLine(relationship.ControlId + "" -> "" +
        string.Join("", "", relationship.RelatedControlIds));";
                case "UI Custom Control Adapter": return @"using System.Windows.Forms;
using HAgent.WinForms.UI;

var control = new MyHyperTextBox
{
    Name = ""txtCustomerName"",
    DbFieldName = ""CustomerName"",
    DisplayName = ""Customer Name""
};

var adapter = new ReflectionUiControlAdapter();
var value = adapter.ReadValue(control);
adapter.WriteValue(control, ""Changed Customer"");

Console.WriteLine(control.DbFieldName);
Console.WriteLine(value);
Console.WriteLine(control.GetValue());";
                case "Application Object Context": return @"using HAgent.WinForms.UI;

var host = HAgentHost.Attach(panel, ""ApplicationContextPanel"", registry, false, permissions);
host.Application.Attach(""invoiceTable"", tableInfo, maxDepth: 2, maxCollectionItems: 20);

var descriptor = host.Application.Describe(""invoiceTable"");
foreach (var property in descriptor.Properties)
    Console.WriteLine(property.Name + "": "" + property.Kind);";
                case "Data Query Contract": return @"var source = new MyDataQuerySource();

var request = new DataQueryRequest
{
    Fields = new[] { ""Id"", ""Name"", ""Amount"" },
    Filters = new[]
    {
        new DataFilterCondition
        {
            Field = ""Amount"",
            Operator = DataQueryOperator.GreaterThanOrEqual,
            Value = 60
        }
    },
    Sorts = new[]
    {
        new DataSort { Field = ""Amount"", Descending = true }
    },
    Take = 20
};

var result = await source.QueryAsync(request, CancellationToken.None);";
                default: return @"// See the corresponding HAgent example source file.
// The Example application uses the public HAgent API shown here as the reference pattern.";
            }
        }
    }
}
