using System;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddWorkspaceRoleTab()
        {
            AddApiTab(
                "WORKSPACE ROLES",
                "Run workspace role policy test",
                "Verifies coordinator, specialist, and participant roles as generic workspace policy metadata over ordinary agent participants.",
                "A coordinator may receive user messages and delegate to an allowed specialist; a specialist without delegation permission must be rejected.",
                "Workspace role-policy verification.",
                TestWorkspaceRolesAsync,
                "Coordinator + specialist policy",
                "Provider-independent deterministic model test; no network or storage mutation.");
        }
    }
}
