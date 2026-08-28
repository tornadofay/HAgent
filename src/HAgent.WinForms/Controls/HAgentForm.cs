using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HAgent.WinForms.Helpers;

namespace HAgent.WinForms.Controls
{
    /// <summary>
    /// Base shell for HAgent WinForms windows. Uses the shared HAgent Header
    /// instead of standard Windows chrome and provides a rounded AI-themed window.
    /// </summary>
    public abstract class HAgentForm : Form
    {
        private readonly Color _surface = Color.FromArgb(248, 248, 252);
        private Header _header;

        protected HAgentForm(string title, string subtitle, Size initialSize, Size minimumSize)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = _surface;
            Font = new Font("Segoe UI", 9f);
            MinimumSize = minimumSize;
            Size = initialSize;
            Padding = new Padding(1);
            BuildChrome(title, subtitle);
        }

        protected Panel BodyPanel { get; private set; }
        protected virtual int HeaderHeight { get { return 54; } }
        protected virtual int CornerRadius { get { return 14; } }
        protected Header WindowHeader { get { return _header; } }

        private void BuildChrome(string title, string subtitle)
        {
            _header = new Header
            {
                Dock = DockStyle.Top,
                HeaderHeight = HeaderHeight,
                Title = title,
                Subtitle = subtitle,
                AllowClose = true,
                AllowMinimize = false,
                AllowHelp = false,
                AllowMove = true,
                BackGroundColor1 = Color.FromArgb(31, 24, 69),
                BackGroundColor2 = Color.FromArgb(88, 39, 126),
                ForeColor1 = Color.FromArgb(246, 244, 255),
                SubtitleColor = Color.FromArgb(214, 205, 235),
                ButtonHoverColor = Color.FromArgb(58, 210, 219, 255),
                ButtonPressedColor = Color.FromArgb(78, 225, 235, 255),
                CloseHoverColor = Color.FromArgb(218, 70, 102)
            };

            BodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _surface,
                Padding = new Padding(24)
            };

            Controls.Add(BodyPanel);
            Controls.Add(_header);
            Shown += delegate { UpdateRoundedRegion(); };
        }

        protected void SetHeaderText(string title, string subtitle)
        {
            if (_header == null) return;
            _header.Title = title ?? string.Empty;
            _header.Subtitle = subtitle ?? string.Empty;
            Text = title ?? string.Empty;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateRoundedRegion();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRoundedRegion();
        }

        private void UpdateRoundedRegion()
        {
            if (!IsHandleCreated || Width <= 0 || Height <= 0)
                return;

            var radius = Math.Max(1, Math.Min(CornerRadius, Math.Min(Width, Height) / 2));
            var bounds = new Rectangle(0, 0, Width, Height);

            using (var path = CreateRoundedPath(bounds, radius))
            {
                var newRegion = new Region(path);
                var oldRegion = Region;
                Region = newRegion;
                if (oldRegion != null)
                    oldRegion.Dispose();
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();

            return path;
        }
    }
}
