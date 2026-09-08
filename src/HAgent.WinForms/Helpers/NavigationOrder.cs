using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.WinForms.Controls;
using HAgent.WinForms.Forms;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.Helpers
{
    internal static class NavigationOrder
    {
        private static readonly string[] Order =
        {
            "Overview",
            "Providers",
            "Agents",
            "Tools",
            "Policy",
            "About"
        };

        public static void Apply(Control root)
        {
            if (root == null) return;

            foreach (Control child in root.Controls)
            {
                if (child is FlowLayoutPanel panel && ContainsNavigationButtons(panel))
                {
                    EnsurePolicyButton(root, panel);
                    Reorder(panel);
                    return;
                }

                Apply(child);
            }
        }

        private static bool ContainsNavigationButtons(FlowLayoutPanel panel)
        {
            var texts = panel.Controls.Cast<Control>()
                .Select(x => x.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return new[] { "Overview", "Providers", "Agents", "Tools", "About" }.All(texts.Contains);
        }

        private static void EnsurePolicyButton(Control root, FlowLayoutPanel nav)
        {
            if (nav.Controls.Cast<Control>().Any(x => string.Equals(x.Text, "Policy", StringComparison.OrdinalIgnoreCase)))
                return;

            var settings = root as Forms.AISettingsForm;
            if (settings == null)
            {
                var parent = nav.Parent;
                settings = parent as Forms.AISettingsForm;
            }

            if (settings == null)
                return;

            var button = CreatePolicyButton();
            button.Click += delegate
            {
                try
                {
                    var storeField = typeof(Forms.AISettingsForm).GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic);
                    var agentsField = typeof(Forms.AISettingsForm).GetField("_agents", BindingFlags.Instance | BindingFlags.NonPublic);
                    var store = storeField == null ? null : storeField.GetValue(settings) as IAiStore;
                    var agents = agentsField == null ? null : agentsField.GetValue(settings) as IReadOnlyList<AiAgent>;
                    if (store == null)
                        throw new InvalidOperationException("The AI configuration store is unavailable.");

                    using (var form = new PolicyEditorForm(store, agents ?? new List<AiAgent>()))
                    {
                        form.ShowDialog(settings);
                    }
                }
                catch (Exception ex)
                {
                    HMessage.ShowException(settings, "The policy configuration could not be opened.", "Policy", ex);
                }
            };
            nav.Controls.Add(button);
        }

        private static HButton CreatePolicyButton()
        {
            return new HButton
            {
                Text = "Policy",
                Width = 166,
                Height = 42,
                RoundButton = true,
                Edge = 10,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                TextMargin = 16,
                Margin = new Padding(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                ButtonLeaveBackGroundColor1 = System.Drawing.Color.FromArgb(31, 24, 69),
                ButtonLeaveBackGroundColor2 = System.Drawing.Color.FromArgb(25, 20, 54),
                ButtonLeaveForeColor = System.Drawing.Color.FromArgb(239, 234, 250),
                ButtonLeaveBorderColor = System.Drawing.Color.FromArgb(55, 45, 94),
                ButtonEnterBackGroundColor1 = System.Drawing.Color.FromArgb(76, 54, 132),
                ButtonEnterBackGroundColor2 = System.Drawing.Color.FromArgb(55, 39, 100),
                ButtonEnterForeColor = System.Drawing.Color.White,
                ButtonEnterBorderColor = System.Drawing.Color.FromArgb(116, 76, 210),
                ButtonDownBackGroundColor1 = System.Drawing.Color.FromArgb(61, 43, 110),
                ButtonDownBackGroundColor2 = System.Drawing.Color.FromArgb(42, 29, 78),
                ButtonDownForeColor = System.Drawing.Color.White,
                ButtonDownBorderColor = System.Drawing.Color.FromArgb(104, 76, 170),
                Font = new System.Drawing.Font("Segoe UI", 9.5f)
            };
        }

        private static void Reorder(FlowLayoutPanel panel)
        {
            var buttons = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);

            foreach (Control control in panel.Controls)
            {
                if (!string.IsNullOrWhiteSpace(control.Text))
                    buttons[control.Text] = control;
            }

            for (var i = Order.Length - 1; i >= 0; i--)
            {
                Control control;
                if (buttons.TryGetValue(Order[i], out control))
                    panel.Controls.SetChildIndex(control, 0);
            }
        }
    }
}
