using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public class ModernButton : Control
{
    private bool _hover;
    private bool _pressed;

    public bool Primary { get; set; } = true;
    public int Radius { get; set; } = 8;

    public ModernButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Surface;
        Font = Theme.ButtonFont;
        Size = new Size(140, 34);
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.Rounded(rect, Radius);

        Color fill;
        Color border;
        Color fore;

        if (!Enabled)
        {
            fill = Theme.Disabled;
            border = Theme.Disabled;
            fore = Theme.DisabledText;
        }
        else if (Primary)
        {
            fill = _pressed ? Theme.AccentPressed : (_hover ? Theme.AccentHover : Theme.Accent);
            border = fill;
            fore = Color.White;
        }
        else
        {
            fill = _pressed ? Theme.Disabled : (_hover ? Theme.HeaderBg : Theme.Surface);
            border = Theme.Border;
            fore = Theme.Text;
        }

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        if (!Primary || !Enabled)
        {
            using var pen = new Pen(border);
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(g, Text, Font, rect, fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
