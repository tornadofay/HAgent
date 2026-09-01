# HAgent.ExternalConsumer

Standalone host-consumer smoke sample for Phase 0.95.

The sample references only `HAgent.Core`. It does not reference WinForms, HAgent storage providers, HWorld, or any application-specific project.

It demonstrates:

- the canonical `AgentExecutionRequest` host boundary;
- bounded host context and host correlation;
- public runtime-instance creation and execution;
- concurrent independent runtime executions;
- provider-neutral host consumption through a host-owned test adapter.

Build the sample from Visual Studio or the command line for either `net481` or `net9.0`.

The sample uses only a local in-memory provider/store and does not contact an external provider.
