using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public class RoundPanel : Panel
{
    public string Title { get; set; } = "";
    public int Radius { get; set; } = 12;
    public Color Fill { get; set; } = Theme.Surface;
    public Color BorderColor { get; set; } = Theme.Border;
    public bool DrawBorder { get; set; } = true;

    public RoundPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = Theme.Background;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.Rounded(rect, Radius);

        using (var b = new SolidBrush(Fill))
            g.FillPath(b, path);

        if (DrawBorder)
        {
            using var pen = new Pen(BorderColor);
            g.DrawPath(pen, path);
        }

        if (!string.IsNullOrEmpty(Title))
            TextRenderer.DrawText(g, Title, Theme.CardTitle, new Point(20, 14), Theme.Text);

        base.OnPaint(e);
    }
}
