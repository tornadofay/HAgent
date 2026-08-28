using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HAgent.WinForms.Helpers
{
    public enum HeaderButton
    {
        None,
        Close,
        Minimize,
        Help
    }

    public enum CloseType
    {
        ExitForm,
        ExitApplication,
        CancelDialogResult,
        Hide
    }

    public enum LanguageMode
    {
        English,
        Arabic
    }

    /// <summary>
    /// Lightweight, self-contained HAgent window header.
    /// It intentionally has no dependency on the larger HLibraries framework.
    /// </summary>
    [DefaultEvent("PerformOnClose")]
    public partial class Header : UserControl
    {
        private const int ButtonWidth = 44;

        private bool _isDragging;
        private Point _lastMousePosition;
        private HeaderButton _hoveredButton = HeaderButton.None;
        private HeaderButton _pressedButton = HeaderButton.None;

        private bool _allowClose = true;
        private bool _allowHelp;
        private bool _allowMinimize;
        private bool _allowMove = true;
        private CloseType _closeMode = CloseType.ExitForm;

        private string _captionEn = "HAgent";
        private string _captionAr = "HAgent";
        private string _subtitleEn = string.Empty;
        private string _subtitleAr = string.Empty;
        private LanguageMode _languageType = LanguageMode.English;

        private Image _headerIcon;
        private Image _cachedIcon;
        private int _imageWidth = 22;
        private int _imageHeight = 22;
        private int _imageMargin = 10;
        private int _textMargin = 8;
        private int _controlHeight = 54;

        private Color _backColor1 = Color.FromArgb(31, 24, 69);
        private Color _backColor2 = Color.FromArgb(88, 39, 126);
        private Color _foreColor = Color.FromArgb(246, 244, 255);
        private Color _subtitleColor = Color.FromArgb(214, 205, 235);
        private Color _buttonHoverColor = Color.FromArgb(70, 255, 255, 255);
        private Color _buttonPressedColor = Color.FromArgb(95, 255, 255, 255);
        private Color _closeHoverColor = Color.FromArgb(218, 70, 102);
        private Form _helpForm;

        public event EventHandler PerformOnClose;
        public event EventHandler PerformOnHelp;
        public event EventHandler PerformOnMinimize;
        public event EventHandler PerformIfExitCancel;

        public Header()
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            MinimumSize = new Size(0, _controlHeight);
            Height = _controlHeight;
            Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
            BackColor = Color.Transparent;
            UpdateCachedIcon();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _cachedIcon != null)
            {
                _cachedIcon.Dispose();
                _cachedIcon = null;
            }

            base.Dispose(disposing);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                return new Size(proposedSize.Width, _controlHeight);

            if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                return new Size(_controlHeight, proposedSize.Height);

            return base.GetPreferredSize(proposedSize);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                height = Math.Max(height, _controlHeight);
            else if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                width = Math.Max(width, _controlHeight);

            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            var button = HitTest(e.Location);
            if (button == HeaderButton.None && _allowMove)
            {
                _isDragging = true;
                _lastMousePosition = Cursor.Position;
                Capture = true;
                return;
            }

            _pressedButton = button;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var button = HitTest(e.Location);
            if (button != _hoveredButton)
            {
                _hoveredButton = button;
                Invalidate();
            }

            if (!_isDragging) return;

            var target = DragTarget ?? FindForm();
            if (target == null) return;

            var current = Cursor.Position;
            var dx = current.X - _lastMousePosition.X;
            var dy = current.Y - _lastMousePosition.Y;
            if (dx != 0 || dy != 0)
                target.Location = new Point(target.Location.X + dx, target.Location.Y + dy);

            _lastMousePosition = current;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            if (_isDragging)
            {
                _isDragging = false;
                Capture = false;
                return;
            }

            var button = HitTest(e.Location);
            if (button == _pressedButton)
                ExecuteButtonAction(button);

            _pressedButton = HeaderButton.None;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredButton != HeaderButton.None)
            {
                _hoveredButton = HeaderButton.None;
                Invalidate();
            }
        }

        private HeaderButton HitTest(Point point)
        {
            var layout = CalculateLayout();
            if (_allowClose && layout.Close.Contains(point)) return HeaderButton.Close;
            if (_allowMinimize && layout.Minimize.Contains(point)) return HeaderButton.Minimize;
            if (_allowHelp && layout.Help.Contains(point)) return HeaderButton.Help;
            return HeaderButton.None;
        }

        private HeaderLayout CalculateLayout()
        {
            var width = Width;
            var height = Height;
            var isRtl = RightToLeft == RightToLeft.Yes;

            var close = new Rectangle(isRtl ? 0 : width - ButtonWidth, 0, ButtonWidth, height);
            var minimize = new Rectangle(isRtl ? ButtonWidth : width - ButtonWidth * 2, 0, ButtonWidth, height);
            var help = new Rectangle(isRtl ? ButtonWidth * 2 : width - ButtonWidth * 3, 0, ButtonWidth, height);

            var buttonsWidth =
                (_allowClose ? ButtonWidth : 0) +
                (_allowMinimize ? ButtonWidth : 0) +
                (_allowHelp ? ButtonWidth : 0);

            var iconX = isRtl ? width - _imageWidth - _imageMargin : _imageMargin;
            var iconY = Math.Max(0, (height - _imageHeight) / 2);
            var icon = new Rectangle(iconX, iconY, _imageWidth, _imageHeight);

            var left = isRtl
                ? buttonsWidth + _textMargin
                : _imageMargin + _imageWidth + _textMargin;

            var right = isRtl
                ? width - _imageWidth - _imageMargin - _textMargin
                : width - buttonsWidth - _textMargin;

            var textWidth = Math.Max(0, right - left);
            var text = new Rectangle(left, 5, textWidth, Math.Max(0, height - 10));

            return new HeaderLayout(icon, text, help, minimize, close);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var brush = new LinearGradientBrush(ClientRectangle, _backColor1, _backColor2, 90f))
                g.FillRectangle(brush, ClientRectangle);

            var layout = CalculateLayout();
            if (_cachedIcon != null)
                g.DrawImage(_cachedIcon, layout.Icon);

            var title = _languageType == LanguageMode.Arabic ? _captionAr : _captionEn;
            var subtitle = _languageType == LanguageMode.Arabic ? _subtitleAr : _subtitleEn;

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                var titleRect = new Rectangle(layout.Text.X, 4, layout.Text.Width, Math.Max(18, layout.Text.Height / 2));
                var subtitleRect = new Rectangle(layout.Text.X, Height / 2 - 1, layout.Text.Width, Math.Max(16, Height / 2 - 3));
                DrawText(g, title, Font, titleRect, _foreColor, RightToLeft == RightToLeft.Yes);
                using (var subtitleFont = new Font("Segoe UI", 7.8f))
                    DrawText(g, subtitle, subtitleFont, subtitleRect, _subtitleColor, RightToLeft == RightToLeft.Yes);
            }
            else
            {
                DrawText(g, title, Font, layout.Text, _foreColor, RightToLeft == RightToLeft.Yes);
            }

            DrawButton(g, layout.Help, HeaderButton.Help, _allowHelp, "?");
            DrawButton(g, layout.Minimize, HeaderButton.Minimize, _allowMinimize, "—");
            DrawButton(g, layout.Close, HeaderButton.Close, _allowClose, "×");
        }

        private static void DrawText(Graphics g, string text, Font font, Rectangle rect, Color color, bool rtl)
        {
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
            flags |= rtl ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : TextFormatFlags.Left;
            TextRenderer.DrawText(g, text ?? string.Empty, font, rect, color, flags);
        }

        private void DrawButton(Graphics g, Rectangle rect, HeaderButton button, bool visible, string glyph)
        {
            if (!visible || rect.Width <= 0) return;

            var hovered = _hoveredButton == button;
            var pressed = _pressedButton == button && hovered;

            if (pressed || hovered)
            {
                var color = button == HeaderButton.Close
                    ? _closeHoverColor
                    : (pressed ? _buttonPressedColor : _buttonHoverColor);
                using (var brush = new SolidBrush(color))
                    g.FillRectangle(brush, rect);
            }

            using (var font = button == HeaderButton.Help
                ? new Font("Segoe UI", 10f, FontStyle.Bold)
                : new Font("Segoe UI Symbol", 13f, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, glyph, font, rect, _foreColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        private void ExecuteButtonAction(HeaderButton button)
        {
            switch (button)
            {
                case HeaderButton.Close:
                    PerformOnClose?.Invoke(this, EventArgs.Empty);
                    HandleClose();
                    break;
                case HeaderButton.Minimize:
                    PerformOnMinimize?.Invoke(this, EventArgs.Empty);
                    var form = FindForm();
                    if (form != null) form.WindowState = FormWindowState.Minimized;
                    break;
                case HeaderButton.Help:
                    PerformOnHelp?.Invoke(this, EventArgs.Empty);
                    ShowHelp();
                    break;
            }
        }

        private void HandleClose()
        {
            var form = FindForm();
            switch (_closeMode)
            {
                case CloseType.ExitForm:
                    form?.Close();
                    break;
                case CloseType.ExitApplication:
                    if (HMessage.ShowQuestion(form, "Do you want to close the application?", "Exit") == DialogResult.Yes)
                        Application.Exit();
                    break;
                case CloseType.CancelDialogResult:
                    if (form != null) form.DialogResult = DialogResult.Cancel;
                    break;
                case CloseType.Hide:
                    form?.Hide();
                    break;
            }
        }

        private void ShowHelp()
        {
            var form = FindForm();
            if (_helpForm == null || form == null) return;

            form.AddOwnedForm(_helpForm);
            try { _helpForm.ShowDialog(form); }
            finally { form.RemoveOwnedForm(_helpForm); }
        }

        private void UpdateCachedIcon()
        {
            if (_cachedIcon != null)
            {
                _cachedIcon.Dispose();
                _cachedIcon = null;
            }

            if (_headerIcon != null && _imageWidth > 0 && _imageHeight > 0)
                _cachedIcon = new Bitmap(_headerIcon, _imageWidth, _imageHeight);
        }

        [Category("HHeader")]
        public int ControlHeight
        {
            get { return _controlHeight; }
            set
            {
                value = Math.Max(24, value);
                if (_controlHeight == value) return;
                _controlHeight = value;
                MinimumSize = new Size(0, value);
                Height = value;
                Invalidate();
            }
        }

        [Category("HHeader")]
        public Control DragTarget { get; set; }

        [Category("HHeader Buttons")]
        public bool AllowClose { get { return _allowClose; } set { _allowClose = value; Invalidate(); } }

        [Category("HHeader Buttons")]
        public bool AllowMinimize { get { return _allowMinimize; } set { _allowMinimize = value; Invalidate(); } }

        [Category("HHeader Buttons")]
        public bool AllowHelp { get { return _allowHelp; } set { _allowHelp = value; Invalidate(); } }

        [Category("HHeader")]
        public bool AllowMove { get { return _allowMove; } set { _allowMove = value; } }

        [Category("HHeader")]
        public CloseType CloseMode { get { return _closeMode; } set { _closeMode = value; } }

        [Category("HHeader")]
        public LanguageMode LanguageType
        {
            get { return _languageType; }
            set
            {
                if (_languageType == value) return;
                _languageType = value;
                RightToLeft = value == LanguageMode.Arabic ? RightToLeft.Yes : RightToLeft.No;
                Invalidate();
            }
        }

        [Category("HHeader Caption")]
        public string CaptionEn { get { return _captionEn; } set { _captionEn = value ?? string.Empty; Invalidate(); } }

        [Category("HHeader Caption")]
        public string CaptionAr { get { return _captionAr; } set { _captionAr = value ?? string.Empty; Invalidate(); } }

        [Category("HHeader Caption")]
        public string SubtitleEn { get { return _subtitleEn; } set { _subtitleEn = value ?? string.Empty; Invalidate(); } }

        [Category("HHeader Caption")]
        public string SubtitleAr { get { return _subtitleAr; } set { _subtitleAr = value ?? string.Empty; Invalidate(); } }

        [Category("HHeader Image")]
        public Image HeaderIcon { get { return _headerIcon; } set { _headerIcon = value; UpdateCachedIcon(); Invalidate(); } }

        [Category("HHeader Image")]
        public int ImageWidth { get { return _imageWidth; } set { _imageWidth = Math.Max(1, value); UpdateCachedIcon(); Invalidate(); } }

        [Category("HHeader Image")]
        public int ImageHeight { get { return _imageHeight; } set { _imageHeight = Math.Max(1, value); UpdateCachedIcon(); Invalidate(); } }

        [Category("HHeader Image")]
        public int ImageMargin { get { return _imageMargin; } set { _imageMargin = Math.Max(0, value); Invalidate(); } }

        [Category("HHeader Caption")]
        public int TextMargin { get { return _textMargin; } set { _textMargin = Math.Max(0, value); Invalidate(); } }

        [Category("HHeader Color")]
        public Color BackGroundColor1 { get { return _backColor1; } set { _backColor1 = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color BackGroundColor2 { get { return _backColor2; } set { _backColor2 = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ForeColor1 { get { return _foreColor; } set { _foreColor = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color SubtitleColor { get { return _subtitleColor; } set { _subtitleColor = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ButtonHoverColor { get { return _buttonHoverColor; } set { _buttonHoverColor = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color ButtonPressedColor { get { return _buttonPressedColor; } set { _buttonPressedColor = value; Invalidate(); } }

        [Category("HHeader Color")]
        public Color CloseHoverColor { get { return _closeHoverColor; } set { _closeHoverColor = value; Invalidate(); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Form HelpForm { get { return _helpForm; } set { _helpForm = value; } }

        private struct HeaderLayout
        {
            public HeaderLayout(Rectangle icon, Rectangle text, Rectangle help, Rectangle minimize, Rectangle close)
            {
                Icon = icon;
                Text = text;
                Help = help;
                Minimize = minimize;
                Close = close;
            }

            public Rectangle Icon { get; private set; }
            public Rectangle Text { get; private set; }
            public Rectangle Help { get; private set; }
            public Rectangle Minimize { get; private set; }
            public Rectangle Close { get; private set; }
        }
    }
}
