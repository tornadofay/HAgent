using HLibraries;
using HLibraries.Dialogs;
using HLibraries.Themes;
using HLibraries.Themes.Style;
using HLibraries.Themes.ThemeProvider;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HAgent.WinForms.Helpers
{
    public enum HeaderButton { None, Close, Minimize, Help }
    // public enum CloseType { ExitForm, ExitApplication, CancelDialogResult, Hide }

    [DefaultEvent("PerformOnClose")]
    public partial class Header : UserControl
    {
        #region Win32 Universal Drag
        // This is the secret to professional dragging. It works on Forms, Panels, and GroupBoxes.
        private bool _isDragging;
        private Point _lastMousePos;
        private HeaderButton _hoveredButton = HeaderButton.None;
        private HeaderButton _pressedButton = HeaderButton.None;
        #endregion

        #region Fields & State
       // private IHyperTheme _currentTheme = DefaultThemeProvider.Instance.CurrentTheme;
        private bool _allowClose = true;
        private bool _allowHelp = true;
        private bool _allowMinimize = true;
        private bool _allowMove = true;
        private string _arCaption = "عنوان عربي";
        private string _enCaption = "English Caption";
      //  private CloseType _closeMode = CloseType.ExitForm;
        private Form _helpForm;
     //   private LanguageMode _languageType = LanguageMode.English;

        private Image _headerIcon;
        private Image _cachedIcon;
        private int _imageWidth = 20, _imageHeight = 20;
        private int _imageMargin = 8;
        private int _controlHeight = 49;
        private Color _backColor1 = Color.FromArgb(30, 30, 30);
        private Color _backColor2 = Color.FromArgb(45, 45, 45);
        private Color _foreColor = Color.White;
        private Color _buttonHoverColor = Color.FromArgb(60, 60, 60);
        private Color _closeHoverColor = Color.FromArgb(232, 17, 35); // Windows 10 Close Red

        public event EventHandler PerformOnClose;
        public event EventHandler PerformOnHelp;
        public event EventHandler PerformOnMinimize;
        public event EventHandler PerformIfExitCancel;
        #endregion

        #region Constructor
        public Header()
        {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            // THIS IS THE KEY: Prevent the layout engine from collapsing us to 0
            MinimumSize = new Size(0, _controlHeight);

            // TITLE FONT: Larger and semi-bold, like a real window title
            this.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);

            if (!IsDesignTime())
            {
                DefaultThemeProvider.Instance.ThemeChanged += OnThemeChanged;
            }
            UpdateCachedIcon();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cachedIcon?.Dispose();
                if (!IsDesignTime()) DefaultThemeProvider.Instance.ThemeChanged -= OnThemeChanged;
                _headerButtonFont?.Dispose();
                _helpButtonFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        // This tells WinForms: "When docking, I need THIS much space"
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                return new Size(proposedSize.Width, _controlHeight);
            if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                return new Size(_controlHeight, proposedSize.Height);
            return base.GetPreferredSize(proposedSize);
        }

        // Prevent the layout engine from collapsing us to 0
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // When docked Top/Bottom, enforce our height
            if ((Dock == DockStyle.Top || Dock == DockStyle.Bottom))
            {
                if (height < 10) height = _controlHeight;
            }
            // When docked Left/Right, enforce our width
            else if ((Dock == DockStyle.Left || Dock == DockStyle.Right))
            {
                if (width < 10) width = _controlHeight;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        // Ensure we resize properly when docking changes
        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) Invalidate(true);
        }

        // Expose ControlHeight for backward compatibility
        [Category("HHeader"), Description("Header height when docked Top/Bottom, or width when docked Left/Right.")]
        public int ControlHeight
        {
            get => _controlHeight;
            set
            {
                if (_controlHeight != value)
                {
                    _controlHeight = value;
                    MinimumSize = new Size(0, _controlHeight); // Prevent collapse
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        private static bool IsDesignTime() => LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        private void OnThemeChanged(object s, EventArgs e) { _currentTheme = DefaultThemeProvider.Instance.CurrentTheme; Invalidate(); }
        #endregion

        #region Universal Drag Logic (Works on Forms, Panels, GroupBoxes)
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                var btn = HitTest(e.Location);
                if (btn == HeaderButton.None && _allowMove)
                {
                    _isDragging = true;
                    _lastMousePos = MousePosition; // Screen coordinates
                    Capture = true;
                }
                else
                {
                    _pressedButton = btn;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Update Hover State
            var btn = HitTest(e.Location);
            if (btn != _hoveredButton) { _hoveredButton = btn; Invalidate(); }

            // Handle Dragging
            if (_isDragging)
            {
                // Find the target to move. Defaults to Form, but can be a Panel/GroupBox if specified.
                Control target = DragTarget ?? FindForm();
                if (target != null)
                {
                    int dx = MousePosition.X - _lastMousePos.X;
                    int dy = MousePosition.Y - _lastMousePos.Y;
                    target.Location = new Point(target.Location.X + dx, target.Location.Y + dy);
                    _lastMousePos = MousePosition;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    Capture = false;
                }
                else
                {
                    var btn = HitTest(e.Location);
                    if (btn == _pressedButton) ExecuteButtonAction(btn);
                    _pressedButton = HeaderButton.None;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoveredButton != HeaderButton.None) { _hoveredButton = HeaderButton.None; Invalidate(); }
        }
        #endregion

        #region Hit Testing & Layout
        private const int BUTTON_WIDTH = 45;

        private HeaderButton HitTest(Point clientPos)
        {
            var rects = CalculateLayout();
            if (_allowClose && rects.close.Contains(clientPos)) return HeaderButton.Close;
            if (_allowMinimize && rects.min.Contains(clientPos)) return HeaderButton.Minimize;
            if (_allowHelp && rects.help.Contains(clientPos)) return HeaderButton.Help;
            return HeaderButton.None;
        }

        private (Rectangle icon, Rectangle text, Rectangle help, Rectangle min, Rectangle close) CalculateLayout()
        {
            int w = Width, h = Height;
            if (w <= 0 || h <= 0) return (Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty, Rectangle.Empty);

            bool isRtl = RightToLeft == RightToLeft.Yes;

            // Buttons: Close is always the outermost, then Min, then Help
            Rectangle closeRect = new Rectangle(isRtl ? 0 : w - BUTTON_WIDTH, 0, BUTTON_WIDTH, h);
            Rectangle minRect = new Rectangle(isRtl ? BUTTON_WIDTH : w - BUTTON_WIDTH * 2, 0, BUTTON_WIDTH, h);
            Rectangle helpRect = new Rectangle(isRtl ? BUTTON_WIDTH * 2 : w - BUTTON_WIDTH * 3, 0, BUTTON_WIDTH, h);

            int buttonsWidth = (_allowClose ? BUTTON_WIDTH : 0)
                             + (_allowMinimize ? BUTTON_WIDTH : 0)
                             + (_allowHelp ? BUTTON_WIDTH : 0);

            // Icon: LEFT in LTR, RIGHT in RTL
            int iconX = isRtl ? w - _imageWidth - _imageMargin : _imageMargin;
            int iconY = (h - _imageHeight) / 2;
            Rectangle iconRect = new Rectangle(iconX, iconY, _imageWidth, _imageHeight);

            // FIXED: Text area calculation for RTL
            int textStart, textEnd;

            if (isRtl)
            {
                // RTL: [Buttons] [Text ........] [Icon]
                textStart = buttonsWidth + _textMargin;
                textEnd = w - _imageWidth - _imageMargin - _textMargin;
            }
            else
            {
                // LTR: [Icon] [........ Text] [Buttons]
                textStart = _imageMargin + _imageWidth + _textMargin;
                textEnd = w - buttonsWidth - _textMargin;
            }

            int textW = textEnd - textStart;
            if (textW < 0) textW = 0;
            Rectangle textRect = new Rectangle(textStart, 0, textW, h);

            return (iconRect, textRect, helpRect, minRect, closeRect);
        }
        #endregion

        #region Rendering Pipeline
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            //  g.FillRectangle(Brushes.Magenta, ClientRectangle);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var layout = CalculateLayout();

            // 1. Background
            using (var brush = new LinearGradientBrush(ClientRectangle, _backColor1, _backColor2, 90F))
                g.FillRectangle(brush, ClientRectangle);

            // 2. Icon
            if (_cachedIcon != null)
                g.DrawImage(_cachedIcon, layout.icon);

            // 3. Text
            var textFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

            if (RightToLeft == RightToLeft.Yes)
            {
                // RTL: Right-align text so it sits NEXT TO the icon (which is on the right)
                textFlags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }
            else
            {
                // LTR: Left-align text so it sits NEXT TO the icon (which is on the left)
                textFlags |= TextFormatFlags.Left;
            }

            TextRenderer.DrawText(g, Text, Font, layout.text, _foreColor, textFlags);

            // 4. Caption Buttons
            DrawButton(g, layout.help, HeaderButton.Help, _allowHelp, "?");
            DrawButton(g, layout.min, HeaderButton.Minimize, _allowMinimize, "—");
            DrawButton(g, layout.close, HeaderButton.Close, _allowClose, "X");
        }

        private readonly Font _headerButtonFont = new Font("Segoe UI", 10F);
        private readonly Font _helpButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private void DrawButton(Graphics g, Rectangle rect, HeaderButton btnType, bool visible, string glyph)
        {
            if (!visible) return;

            bool isHovered = _hoveredButton == btnType;
            bool isPressed = _pressedButton == btnType && isHovered;

            Color bgColor = Color.Transparent;
            Color fgColor = _foreColor;

            if (isPressed)
            {
                bgColor = btnType == HeaderButton.Close ? Color.FromArgb(200, 15, 30) : Color.FromArgb(80, 80, 80);
            }
            else if (isHovered)
            {
                bgColor = btnType == HeaderButton.Close ? _closeHoverColor : _buttonHoverColor;
            }

            if (bgColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(bgColor))
                    g.FillRectangle(brush, rect);
            }

            // Draw crisp glyphs using TextRenderer
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            Font btnFont = btnType == HeaderButton.Help ? _helpButtonFont : _headerButtonFont;
            TextRenderer.DrawText(g, glyph, btnFont, rect, fgColor, flags);
            btnFont.Dispose();
        }

        private void UpdateCachedIcon()
        {
            _cachedIcon?.Dispose();
            _cachedIcon = null;
            if (_headerIcon != null)
            {
                // Use the shared cache if available, or just resize once
                _cachedIcon = new Bitmap(_headerIcon, _imageWidth, _imageHeight);
            }
        }
        #endregion

        #region Button Actions
        private void ExecuteButtonAction(HeaderButton btn)
        {
            switch (btn)
            {
                case HeaderButton.Close:
                    PerformOnClose?.Invoke(this, EventArgs.Empty);
                    HandleClose();
                    break;
                case HeaderButton.Minimize:
                    PerformOnMinimize?.Invoke(this, EventArgs.Empty);
                    var frmMin = FindForm();
                    if (frmMin != null) frmMin.WindowState = FormWindowState.Minimized;
                    break;
                case HeaderButton.Help:
                    PerformOnHelp?.Invoke(this, EventArgs.Empty);
                    ShowHelp();
                    break;
            }
        }

        private void HandleClose()
        {
            var frm = FindForm();
            switch (_closeMode)
            {
                case CloseType.ExitForm:
                    frm?.Close();
                    break;
                case CloseType.ExitApplication:
                    // Replaced GC.Collect with standard exit
                    switch (Hc.Instance.CurrentLanguage)
                    {
                        case LanguageMode.Arabic:
                            DialogResult result = HMessageDialog.ShowExit(ParentForm, "هل تريد الخروج من البرنامج ؟", "تحذير خروج");
                            //Hc.Instance.ShowMessage("هل تريد الخروج من البرنامج ؟", "تحذير خروج", MessageIcon.Question, MessageButton.YesNo, ParentForm)
                            if (result == DialogResult.Yes)
                            {
                                Application.Exit();
                                // Environment.Exit(Environment.ExitCode);
                            }
                            break;

                        case LanguageMode.English:
                            DialogResult resulten = HMessageDialog.ShowExit(ParentForm, "Do you want to close the program?", "Exit Warning");
                            //Hc.Instance.ShowMessage("    Do you want to close the program?", "Exit Warning", MessageIcon.Question, MessageButton.YesNo, ParentForm)
                            if (resulten == DialogResult.Yes)
                            {
                                Application.Exit();
                                //  Environment.Exit(Environment.ExitCode);
                            }
                            break;

                            //default:
                            //    PerformIfExitCancel?.Invoke(this, EventArgs.Empty);
                            //    break;
                    }


                    break;
                case CloseType.CancelDialogResult:
                    if (frm != null) frm.DialogResult = DialogResult.Cancel;
                    break;
                case CloseType.Hide:
                    frm?.Hide();
                    break;
            }
        }

        private void ShowHelp()
        {
            var frm = FindForm();
            if (_helpForm != null && frm != null)
            {
                frm.AddOwnedForm(_helpForm);
                _helpForm.ShowDialog();
                frm.RemoveOwnedForm(_helpForm);
            }
        }
        #endregion

        #region Properties
        [Category("HHeader"), Description("The target control to drag. Defaults to the parent Form. Set this to a Panel or GroupBox to drag them instead.")]
        public Control DragTarget { get; set; }

        [Category("HHeader Buttons")] public bool AllowClose { get => _allowClose; set { _allowClose = value; Invalidate(); } }
        [Category("HHeader Buttons")] public bool AllowMinimize { get => _allowMinimize; set { _allowMinimize = value; Invalidate(); } }
        [Category("HHeader Buttons")] public bool AllowHelp { get => _allowHelp; set { _allowHelp = value; Invalidate(); } }
        [Category("HHeader")] public bool AllowMove { get => _allowMove; set => _allowMove = value; }
        [Category("HHeader")] public CloseType CloseMode { get => _closeMode; set => _closeMode = value; }
        [Category("HHeader"), Browsable(false)] public Form HelpForm { get => _helpForm; set => _helpForm = value; }

        [Category("HHeader Caption")]
        public string CaptionEn { get => _enCaption; set { _enCaption = value; if (_languageType == LanguageMode.English) UpdateText(); } }

        [Category("HHeader Caption")]
        public string CaptionAr { get => _arCaption; set { _arCaption = value; if (_languageType == LanguageMode.Arabic) UpdateText(); } }

        [Category("HHeader")]
        public LanguageMode LanguageType
        {
            get => _languageType;
            set
            {
                if (_languageType != value)
                {
                    _languageType = value;
                    RightToLeft = value == LanguageMode.Arabic ? RightToLeft.Yes : RightToLeft.No;
                    UpdateText();
                    Invalidate();
                }
            }
        }

        private void UpdateText()
        {
            Text = _languageType == LanguageMode.Arabic ? _arCaption : _enCaption;
            var frm = FindForm();
            if (frm != null) frm.Text = Text; // Sync with Form title bar if it exists
        }

        [Category("HHeader Image")]
        public Image HeaderIcon
        {
            get => _headerIcon;
            set { if (_headerIcon != value) { _headerIcon = value; UpdateCachedIcon(); Invalidate(); } }
        }

        [Category("HHeader Color")] public Color BackGroundColor1 { get => _backColor1; set { _backColor1 = value; Invalidate(); } }
        [Category("HHeader Color")] public Color BackGroundColor2 { get => _backColor2; set { _backColor2 = value; Invalidate(); } }
        [Category("HHeader Color")] public Color ForeColor1 { get => _foreColor; set { _foreColor = value; Invalidate(); } }
        #endregion

        // --- Add these fields near your other fields ---
        //private int _imageWidth = 32;
        //private int _imageHeight = 32;
        //private int _imageMargin = 5;
        private int _textMargin = 5;
        private int _groupIndex;
        private int _orderIndex;
        private Color _foreColor2 = Color.Cyan; // Kept for backward compatibility

        // --- Add these properties ---

        [Category("HHeader Image")]
        public int ImageWidth { get => _imageWidth; set { _imageWidth = value; UpdateCachedIcon(); Invalidate(); } }

        [Category("HHeader Image")]
        public int ImageHeight { get => _imageHeight; set { _imageHeight = value; UpdateCachedIcon(); Invalidate(); } }

        [Category("HHeader Image")]
        public int ImageMargin { get => _imageMargin; set { _imageMargin = value; Invalidate(); } }

        [Category("HHeader Caption")]
        public int TextMargin { get => _textMargin; set { _textMargin = value; Invalidate(); } }

        [Category("HHeader Menu")]
        public int GroupIndex { get => _groupIndex; set => _groupIndex = value; }

        [Category("HHeader Menu")]
        public int OrderIndex { get => _orderIndex; set => _orderIndex = value; }

        [Category("HHeader Color")]
        public Color ForeColor2 { get => _foreColor2; set { _foreColor2 = value; Invalidate(); } }

        // Maps the old ControlHeight to the standard Height property
        //[Category("HHeader"), Browsable(false)]
        //public int ControlHeight
        //{
        //    get => Height;
        //    set { Height = value; }
        //}
    }
}
