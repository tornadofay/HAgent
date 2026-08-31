using System;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddInternalMemoryTab()
        {
            AddApiTab(
                "Internal Memory",
                "Run internal memory test",
                "Reads HAgent-owned memory through the built-in read-only memory tool using an explicit scope and owner filter.",
                "The requested memory should be returned, another owner's memory must remain hidden, sensitive metadata must be redacted, and maxItems above the hard limit must be rejected.",
                "HAgent-internal-memory-42",
                TestInternalMemoryAsync,
                "Internal memory boundary",
                "This is read-only at the tool boundary. The example creates temporary records only to verify the public read contract, then removes them directly through the memory store.");
        }
    }
}
