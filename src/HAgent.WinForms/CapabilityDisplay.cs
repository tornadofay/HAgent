using System;
using System.Text;
using System.Windows.Forms;
using HAgent.Models;

namespace HAgent.WinForms
{
    internal static class CapabilityDisplay
    {
        public static string BuildSummary(AiModelCapabilities capabilities)
        {
            if (capabilities == null) return "Capabilities: unavailable";

            var chat = capabilities.Get(AiCapability.Chat);
            var summary = "Capabilities: Chat " + Symbol(chat) +
                          "  Tools " + Symbol(capabilities.Get(AiCapability.ToolCalling)) +
                          "  Vision " + Symbol(capabilities.Get(AiCapability.Vision)) +
                          "  Reasoning " + Symbol(capabilities.Get(AiCapability.Reasoning));

            if (chat == CapabilitySupport.Unsupported)
                return "Not suitable for chat  •  " + summary;

            return summary;
        }

        public static void AttachToolTip(Control control, AiModelCapabilities capabilities)
        {
            if (control == null || capabilities == null) return;

            var tip = new ToolTip();
            var text = new StringBuilder();
            text.AppendLine("Model: " + capabilities.Model);
            text.AppendLine();
            foreach (AiCapability capability in Enum.GetValues(typeof(AiCapability)))
            {
                if (capability == AiCapability.None) continue;
                var evidence = capabilities.GetEvidence(capability);
                text.Append(capability);
                text.Append(": ");
                text.Append(evidence.Support);
                text.Append(" | source: ");
                text.Append(evidence.Source);
                text.Append(" | confidence: ");
                text.Append((evidence.Confidence * 100d).ToString("0") + "%");
                if (!string.IsNullOrWhiteSpace(evidence.Note))
                {
                    text.Append(" | ");
                    text.Append(evidence.Note);
                }
                text.AppendLine();
            }
            text.AppendLine();
            text.Append("Unknown means HAgent has not established support.");
            tip.SetToolTip(control, text.ToString());
        }

        private static string Symbol(CapabilitySupport support)
        {
            switch (support)
            {
                case CapabilitySupport.Supported: return "✓";
                case CapabilitySupport.Unsupported: return "×";
                default: return "?";
            }
        }
    }
}
