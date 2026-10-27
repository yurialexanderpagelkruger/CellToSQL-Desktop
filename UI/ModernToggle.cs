using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public class ModernToggle : Control
{
    private bool _checked;
    private bool _hover;

    public event EventHandler CheckedChanged;

    public ModernToggle()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Surface;
        Font = Theme.Body;
        Size = new Size(240, 24);
        Cursor = Cursors.Hand;
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnClick(EventArgs e)
    {
        if (Enabled) Checked = !Checked;
        base.OnClick(e);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        int trackW = 38;
        int trackH = 20;
        var track = new Rectangle(0, (Height - trackH) / 2, trackW, trackH);

        Color trackColor = !Enabled
            ? Theme.Disabled
            : (_checked ? (_hover ? Theme.AccentHover : Theme.Accent) : Color.FromArgb(203, 213, 225));

        using (var path = Theme.Rounded(track, trackH / 2))
        using (var b = new SolidBrush(trackColor))
            g.FillPath(b, path);

        int knob = trackH - 4;
        int kx = _checked ? track.Right - knob - 2 : track.X + 2;
        using (var b = new SolidBrush(Color.White))
            g.FillEllipse(b, kx, track.Y + 2, knob, knob);

        Color textColor = Enabled ? Theme.Text : Theme.DisabledText;
        TextRenderer.DrawText(g, Text, Font, new Point(trackW + 10, (Height - Font.Height) / 2), textColor);
    }
}
