using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public class ModernTextBox : Control
{
    private readonly TextBox _box = new TextBox();
    private bool _focused;

    public int Radius { get; set; } = 8;

    public ModernTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Surface;
        Size = new Size(200, 34);

        _box.BorderStyle = BorderStyle.None;
        _box.Font = Theme.Body;
        _box.BackColor = Theme.Surface;
        _box.ForeColor = Theme.Text;
        _box.GotFocus += (s, e) => { _focused = true; Invalidate(); };
        _box.LostFocus += (s, e) => { _focused = false; Invalidate(); };
        _box.TextChanged += (s, e) => OnTextChanged(EventArgs.Empty);

        Controls.Add(_box);
        LayoutBox();
    }

    public override string Text
    {
        get => _box.Text;
        set => _box.Text = value;
    }

    public char PasswordChar
    {
        get => _box.PasswordChar;
        set => _box.PasswordChar = value;
    }

    public bool ReadOnly
    {
        get => _box.ReadOnly;
        set => _box.ReadOnly = value;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutBox();
    }

    private void LayoutBox()
    {
        _box.Location = new Point(12, (Height - _box.Height) / 2);
        _box.Width = Width - 24;
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        _box.Enabled = Enabled;
        _box.BackColor = Enabled ? Theme.Surface : Theme.HeaderBg;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.Rounded(rect, Radius);

        using (var b = new SolidBrush(Enabled ? Theme.Surface : Theme.HeaderBg))
            g.FillPath(b, path);

        Color border = !Enabled ? Theme.Border : (_focused ? Theme.Accent : Theme.Border);
        float width = _focused ? 1.6f : 1f;
        using (var pen = new Pen(border, width))
            g.DrawPath(pen, path);
    }
}
