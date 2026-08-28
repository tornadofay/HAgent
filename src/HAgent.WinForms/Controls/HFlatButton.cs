using System.Drawing;
using System.Windows.Forms;

namespace HAgent.WinForms.Controls
{
    internal sealed class HFlatButton : Button
    {
        public HFlatButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(37, 99, 235);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Height = 38;
            Cursor = Cursors.Hand;
            Padding = new Padding(14, 0, 14, 0);
        }
    }
}
