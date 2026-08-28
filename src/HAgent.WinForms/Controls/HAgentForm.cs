using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HAgent.WinForms.Controls
{
    public abstract class HAgentForm : Form
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private readonly Color _header1 = Color.MidnightBlue;
        private readonly Color _header2 = Color.Black;
        private readonly Color _surface = Color.FromArgb(248, 250, 252);
        private Panel _header;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Button _closeButton;

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

        protected virtual int HeaderHeight { get { return 74; } }
        protected virtual int CornerRadius { get { return 12; } }

        private void BuildChrome(string title, string subtitle)
        {
            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = _header1,
                Cursor = Cursors.Default
            };
            _header.Paint += Header_Paint;
            _header.MouseDown += Chrome_MouseDown;

            _titleLabel = new Label
            {
                AutoSize = true,
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Left = 18,
                Top = 12,
                BackColor = Color.Transparent
            };
            _titleLabel.MouseDown += Chrome_MouseDown;

            _subtitleLabel = new Label
            {
                AutoSize = true,
                Text = subtitle,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.5f),
                Left = 19,
                Top = 40,
                BackColor = Color.Transparent
            };
            _subtitleLabel.MouseDown += Chrome_MouseDown;

            _closeButton = new Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(70, 255, 255, 255) },
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Symbol", 15f),
                Size = new Size(38, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            _closeButton.Click += delegate { Close(); };
            _header.Controls.Add(_titleLabel);
            _header.Controls.Add(_subtitleLabel);
            _header.Controls.Add(_closeButton);
            _header.Resize += delegate { _closeButton.Location = new Point(_header.Width - _closeButton.Width - 8, 8); };
            _closeButton.Location = new Point(_header.Width - _closeButton.Width - 8, 8);

            BodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _surface,
                Padding = new Padding(24)
            };

            Controls.Add(BodyPanel);
            Controls.Add(_header);
            Resize += delegate { UpdateRoundedRegion(); };
            Shown += delegate { UpdateRoundedRegion(); };
        }

        protected void SetHeaderText(string title, string subtitle)
        {
            _titleLabel.Text = title;
            _subtitleLabel.Text = subtitle;
        }

        private void Header_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(_header.ClientRectangle, _header1, _header2, 90f))
                e.Graphics.FillRectangle(brush, _header.ClientRectangle);

            using (var pen = new Pen(Color.FromArgb(55, Color.White), 1f))
                e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        }

        private void UpdateRoundedRegion()
        {
            if (Width <= 0 || Height <= 0) return;
            using (var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius))
            {
                Region = new Region(path);
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void Chrome_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
