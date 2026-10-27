using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public class ModernProgress : Control
{
    private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
    private int _offset;

    public ModernProgress()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Surface;
        Height = 8;
        _timer.Interval = 16;
        _timer.Tick += (s, e) =>
        {
            _offset += 8;
            if (_offset > Width + 160) _offset = -160;
            Invalidate();
        };
    }

    public void Start()
    {
        _offset = -160;
        _timer.Start();
        Invalidate();
    }

    public void Stop()
    {
        _timer.Stop();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        var track = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = Theme.Rounded(track, Height / 2))
        using (var b = new SolidBrush(Color.FromArgb(226, 232, 240)))
            g.FillPath(b, path);

        if (!_timer.Enabled) return;

        int segW = Math.Max(70, Width / 4);
        var seg = new Rectangle(_offset, 0, segW, Height - 1);
        seg.Intersect(new Rectangle(0, 0, Width, Height));
        if (seg.Width <= 1) return;

        using (var path = Theme.Rounded(seg, Height / 2))
        using (var b = new SolidBrush(Theme.Accent))
            g.FillPath(b, path);
    }
}
