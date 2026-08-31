using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Models;
using HAgent.WinForms;
using HAgent.WinForms.Helpers;
using HAgent.WinForms.Helpers.Button;
using System.Linq;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private HButton CreateButton(string text, int width)
        {
            return new HButton
            {
                Text = text,
                Width = width,
                Height = 36,
                RoundButton = true,
                Edge = 10,
                Font = new Font("Segoe UI", 9.3f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                ButtonLeaveBackGroundColor1 = Color.FromArgb(92, 67, 168),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(57, 40, 108),
                ButtonLeaveForeColor = Color.White,
                ButtonLeaveBorderColor = Color.FromArgb(116, 76, 210),
                ButtonEnterBackGroundColor1 = Color.FromArgb(126, 94, 214),
                ButtonEnterBackGroundColor2 = Color.FromArgb(79, 54, 145),
                ButtonEnterForeColor = Color.White,
                ButtonEnterBorderColor = Color.FromArgb(146, 118, 232),
                ButtonDownBackGroundColor1 = Color.FromArgb(72, 52, 132),
                ButtonDownBackGroundColor2 = Color.FromArgb(45, 31, 88),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176),
                ButtonDownForeColor = Color.White,
                ButtonDownBorderColor = Color.FromArgb(104, 79, 176),
                Margin = new Padding(0, 0, 10, 0)
            };
        }

        private void SetBusy(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => SetBusy(text)));
                return;
            }

            SetButtonsEnabled(false);
            _globalStatus.Text = text;
            _globalStatus.ForeColor = Accent;
        }

        private void SetReady()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)SetReady);
                return;
            }

            SetButtonsEnabled(true);
            _ = UpdateSelectedAgentAsync();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => SetButtonsEnabled(enabled)));
                return;
            }

            _configurationButton.Enabled = enabled;
            _clearOutputButton.Enabled = enabled;
            _agentSelector.Enabled = enabled;
            foreach (var button in _testButtons)
                button.Enabled = enabled;
        }

        private void Write(string title, string value)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => Write(title, value)));
                return;
            }

            _output.Text = "[" + title + "]" + Environment.NewLine +
                           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                           value + Environment.NewLine;
        }

        private static string RequireInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("The example input cannot be empty.", nameof(input));
            return input;
        }

        private async Task RunExampleAsync(Func<Task> action)
        {
            SetBusy("Running example...");
            try
            {
                await action();
                SetReady();
            }
            catch (Exception ex)
            {
                Write("EXCEPTION", ex.ToString());
                SetReady();
                ShowExampleException(ex);
            }
        }

        private void ShowExampleException(Exception exception)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ShowExampleException(exception)));
                return;
            }

            HMessage.ShowException(this, "The example failed.", "HAgent Example", exception);
        }

        private async Task RefreshExampleAgentsAsync()
        {
            HAgentStorageOptions options = null;
            try
            {
                options = await LoadStorageOptionsAsync().ConfigureAwait(true);
                var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);