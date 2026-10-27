using System.Drawing.Drawing2D;

namespace CellToSQL.UI;

public static class Theme
{
    public static readonly Color Background = Color.FromArgb(244, 246, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color Text = Color.FromArgb(30, 41, 59);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    public static readonly Color AccentPressed = Color.FromArgb(30, 64, 175);
    public static readonly Color HeaderBg = Color.FromArgb(248, 250, 252);
    public static readonly Color Zebra = Color.FromArgb(249, 250, 252);
    public static readonly Color SelectedRow = Color.FromArgb(219, 234, 254);
    public static readonly Color Disabled = Color.FromArgb(226, 232, 240);
    public static readonly Color DisabledText = Color.FromArgb(148, 163, 184);
    public static readonly Color LogBg = Color.FromArgb(15, 23, 42);
    public static readonly Color LogText = Color.FromArgb(134, 239, 172);

    public static readonly Font Title = new Font("Segoe UI", 16f, FontStyle.Bold);
    public static readonly Font Subtitle = new Font("Segoe UI", 9.5f);
    public static readonly Font CardTitle = new Font("Segoe UI", 11f, FontStyle.Bold);
    public static readonly Font Body = new Font("Segoe UI", 9.5f);
    public static readonly Font Caption = new Font("Segoe UI", 8.25f);
    public static readonly Font CaptionBold = new Font("Segoe UI", 8.25f, FontStyle.Bold);
    public static readonly Font ButtonFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font Log = new Font("Consolas", 9f);

    public static GraphicsPath Rounded(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        if (d > r.Width) d = r.Width;
        if (d > r.Height) d = r.Height;
        if (d <= 0) d = 1;

        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
