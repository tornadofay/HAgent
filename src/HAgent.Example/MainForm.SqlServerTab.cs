using System;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private bool _sqlServerTabAdded;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_sqlServerTabAdded)
                return;

            _sqlServerTabAdded = true;
            AddApiTab(
                "SQL Server Data Query",
                "Run SQL Server query test",
                "Runs the restricted SQL Server IDataQuerySource against dbo.HAgentExampleCustomers using runtime-only connection fields. The adapter performs only generated structured SELECT operations.",
                "The test should return David and Alice, reject the Secret field, deny an unauthorized host request before database execution, and preserve an injection-shaped value as data.",
                "Server Name\r\nUser Name\r\nPassword\r\nDatabase",
                TestSqlServerDataQueryAsync,
                "Runtime database boundary",
                "Use a disposable/read-only database containing dbo.HAgentExampleCustomers with Id, Name, Amount, and optional Secret columns. Connection values are used only for this running Example and are not persisted or logged.");
        }
    }
}
