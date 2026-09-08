using System.Drawing;
using System.Windows.Forms;
using HAgent.WinForms.Helpers.Button;

namespace HAgent.WinForms.UI.Configuration
{
    internal abstract class ConfigurationPageBase : UserControl
    {
        protected static readonly Color Surface = Color.FromArgb(248, 248, 252);
        protected static readonly Color Heading = Color.FromArgb(31, 24, 69);
        protected static readonly Color Muted = Color.FromArgb(100, 92, 120);
        protected static readonly Color Accent = Color.FromArgb(116, 76, 210);

        protected ConfigurationPageBase()
        {
            Dock = DockStyle.Fill;
            BackColor = Surface;
            Font = new Font("Segoe UI", 9f);
        }

        protected Panel CreateHeader(string title, string description, int height = 70)
        {
            var header = new Panel { Dock = DockStyle.Top, Height = height, BackColor = Surface };
            header.Controls.Add(new Label { Text = title, AutoSize = true, Left = 0, Top = 0, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = Heading });
            header.Controls.Add(new Label { Text = description, AutoSize = true, Left = 1, Top = 35, Font = new Font("Segoe UI", 8.8f), ForeColor = Muted });
            return header;
        }

        protected static void ConfigureList(ListView list)
        {
            list.Dock = DockStyle.Fill;
            list.View = View.Details;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.GridLines = false;
            list.BackColor = Color.White;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.Font = new Font("Segoe UI", 9f);
        }

        protected static HButton CreateActionButton(string text, int width, bool destructive = false)
        {
            var button = new HButton { Text = text, Width = width, Height = 36, RoundButton = true, Edge = 10, TextAlign = ContentAlignment.MiddleCenter, TextMargin = 8, Font = new Font("Segoe UI", 9.2f, FontStyle.Bold), Cursor = Cursors.Hand };
            button.ButtonLeaveBackGroundColor1 = destructive ? Color.FromArgb(183, 61, 89) : Color.FromArgb(92, 67, 168);
            button.ButtonLeaveBackGroundColor2 = destructive ? Color.FromArgb(119, 38, 62) : Color.FromArgb(57, 40, 108);
            button.ButtonLeaveForeColor = Color.White;
            button.ButtonLeaveBorderColor = destructive ? Color.FromArgb(207, 80, 105) : Accent;
            button.ButtonEnterBackGroundColor1 = destructive ? Color.FromArgb(214, 75, 106) : Color.FromArgb(126, 94, 214);
            button.ButtonEnterBackGroundColor2 = destructive ? Color.FromArgb(150, 43, 74) : Color.FromArgb(79, 54, 145);
            button.ButtonEnterForeColor = Color.White;
            button.ButtonEnterBorderColor = destructive ? Color.FromArgb(231, 105, 131) : Color.FromArgb(146, 118, 232);
            button.ButtonDownBackGroundColor1 = destructive ? Color.FromArgb(151, 45, 70) : Color.FromArgb(72, 52, 132);
            button.ButtonDownBackGroundColor2 = destructive ? Color.FromArgb(99, 29, 51) : Color.FromArgb(45, 31, 88);
            button.ButtonDownForeColor = Color.White;
            button.ButtonDownBorderColor = destructive ? Color.FromArgb(190, 63, 91) : Color.FromArgb(104, 79, 176);
            return button;
        }
    }
}
