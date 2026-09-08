using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static string GetExampleSubGroup(string title)
        {
            var key = (title ?? string.Empty).Trim().ToUpperInvariant();

            if (key == "CONTEXT CONTRACTS" || key == "CONTEXT ACQUISITION" || key == "CONTEXT BUDGET" || key == "CONTEXT RANKING" || key == "CONTEXT COMPACTION" || key == "CONTEXT CACHE" || key == "CONTEXT EXECUTION INTEGRATION" || key == "CONTEXT MULTI-RESOURCE RETRIEVAL" || key == "CONTEXT POLICY ASSEMBLY" || key == "CONTEXT ASSEMBLY" || key == "CONTEXT HOST AUTHORIZATION")
                return "Context Core";
            if (key.StartsWith("UI ", StringComparison.Ordinal) || key == "APPLICATION OBJECT CONTEXT")
                return "UI Context";
            if (key == "DATA QUERY CONTRACT")
                return "Data Access Context";

            if (key == "RUNTIME INSTANCES" || key == "RUNTIME OVERRIDES" || key == "RUNTIME SHUTDOWN" || key == "RUNTIME SCHEDULING" || key == "RUNTIME CONCURRENCY")
                return "Runtime Instances";
            if (key == "EXECUTION INTERVENTION" || key == "INTERVENTION HARDENING")
                return "Intervention";
            if (key == "RUNTIME TERMINAL STATE" || key == "RESOURCE CAPABILITY" || key == "RUNTIME EXECUTION")
                return "Execution";
            if (key == "EXECUTION TARGET PLANNING" || key == "EXECUTION TARGET CATALOG" || key == "QUOTA ADMISSION")
                return "Planning & Capacity";
            if (key == "EXECUTION AUDIT" || key == "INTERNAL INVENTORY")
                return "Diagnostics";

            return "Execution";
        }

        private static string GetExampleFeatureGroup(string title)
        {
            var key = (title ?? string.Empty).Trim().ToUpperInvariant();

            if (key == "MESSAGING" || key == "SESSION" || key == "PERSISTENT SESSION")
                return "Core";

            if (key.Contains("MEMORY") || key == "AUTOMATIC MEMORY" || key == "TASK / EVENT MEMORY" || key == "EPISODIC MEMORY")
                return "Memory";

            if (key == "CONTEXT CONTRACTS" || key == "CONTEXT ACQUISITION" || key == "CONTEXT RANKING" || key == "CONTEXT COMPACTION" || key == "CONTEXT CACHE" || key == "CONTEXT EXECUTION INTEGRATION" || key == "CONTEXT MULTI-RESOURCE RETRIEVAL" || key == "CONTEXT POLICY ASSEMBLY" || key == "CONTEXT ASSEMBLY" || key == "CONTEXT HOST AUTHORIZATION" || key == "CONTEXT BUDGET" ||
                key.StartsWith("UI ", StringComparison.Ordinal) || key == "APPLICATION OBJECT CONTEXT" || key == "DATA QUERY CONTRACT")
                return "Context";

            if (key.Contains("TOOL"))
                return "Tools";

            if (key.Contains("PROVIDER") || key == "CAPABILITIES" || key == "RESPONSE NORMALIZATION" ||
                key == "STREAMING" || key == "LIVE STREAMING")
                return "Providers";

            if (key.Contains("POLICY") || key == "APPROVAL WORKFLOW")
                return "Policy";

            if (key.Contains("EVENT"))
                return "Events";

            if (key.Contains("IDENTITY"))
                return "Identity";

            if (key.Contains("RUNTIME") || key.Contains("EXECUTION") || key == "RESOURCE CAPABILITY" || key == "QUOTA ADMISSION")
                return "Runtime";

            if (key.Contains("WORKSPACE"))
                return "Workspace";

            if (key.Contains("LEARNING") || key.Contains("COGNITION"))
                return "Cognition";

            if (key.Contains("CONFIGURATION"))
                return "Configuration";

            return "Diagnostics";
        }
    }
}