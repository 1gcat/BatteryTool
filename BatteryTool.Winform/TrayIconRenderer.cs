using BatteryTool.Core;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BatteryTool.Winform;

/// <summary>
/// Draws a single tray icon that shows BOTH devices at once:
/// the left half is the keyboard, the right half is the mouse.
/// Each half is a mini vertical "battery" whose fill level is the battery
/// percent; fill color encodes level (green/yellow/red), gray means unknown
/// or disconnected. A small bolt marks charging. Two thin device glyphs
/// (keyboard keys / mouse outline) decorate the bottom of each half.
/// </summary>
internal static class TrayIconRenderer
{
    /// <summary>
    /// Picks the display for one tray half: prefer a fresh reading, then the
    /// lowest battery among fresh ones, then a stale cached value, else null.
    /// </summary>
    public static BatteryDisplay? SelectDisplay(IReadOnlyList<BatteryDisplay> displays, DeviceKind kind)
    {
        var candidates = displays.Where(d => d.Reading.Kind == kind).ToArray();
        return candidates.FirstOrDefault(d => !d.IsStale && d.Percent is not null)
            ?? candidates.Where(d => !d.IsStale).OrderBy(d => d.Percent ?? int.MaxValue).FirstOrDefault()
            ?? candidates.FirstOrDefault(d => d.Percent is not null);
    }

    public static Bitmap CreateBitmap(BatteryDisplay? keyboard, BatteryDisplay? mouse)
    {
        const int size = 64;
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            DrawHalf(g, new RectangleF(4, 6, 26, 52), keyboard, keyboardGlyph: true);
            DrawHalf(g, new RectangleF(34, 6, 26, 52), mouse, keyboardGlyph: false);
        }
        return bmp;
    }

    public static Icon CreateIcon(BatteryDisplay? keyboard, BatteryDisplay? mouse)
    {
        // Render large then downscale for smoother results at 16/20 DPI sizes.
        using var bmp = CreateBitmap(keyboard, mouse);
        using var scaled = new Bitmap(bmp, new Size(32, 32));
        IntPtr handle = scaled.GetHicon();
        try { return Icon.FromHandle(handle).Clone() is Icon icon ? icon : SystemIcons.Application; }
        finally { DestroyIconSafe(handle); }
    }

    private static void DrawHalf(Graphics g, RectangleF cell, BatteryDisplay? display, bool keyboardGlyph)
    {
        int? percent = display?.Percent;
        bool stale = display?.IsStale ?? true;
        bool charging = display?.Charging == true;

        // Battery body: border + fill from bottom.
        var body = new RectangleF(cell.X + 2, cell.Y + 5, cell.Width - 4, cell.Height - 5);
        float capH = 4;
        var capRect = new RectangleF(cell.X + cell.Width / 2 - 5, cell.Y, 10, capH);

        using var borderPen = new Pen(stale ? Color.DimGray : Color.Gainsboro, 3f);
        g.DrawRectangle(borderPen, body.X, body.Y, body.Width, body.Height);
        g.FillRectangle(stale ? Brushes.DimGray : Brushes.Gainsboro, capRect);

        Color fill = percent switch
        {
            null => Color.FromArgb(120, 120, 120),
            <= 20 => Color.OrangeRed,
            <= 50 => Color.Gold,
            _ => Color.LimeGreen
        };
        if (stale && percent is not null)
            fill = Color.FromArgb(150, 150, 150);
        if (percent is int p && display is not null)
        {
            float fillH = body.Height * Math.Clamp(p, 0, 100) / 100f - 4;
            if (fillH > 1)
            {
                using var b = new SolidBrush(fill);
                g.FillRectangle(b, body.X + 2.5f, body.Y + body.Height - fillH - 2.5f, body.Width - 5, fillH);
            }
        }

        // Device glyph at the bottom inside the cell, over the fill.
        using var glyphPen = new Pen(Color.FromArgb(230, 255, 255, 255), 2.2f);
        if (keyboardGlyph)
        {
            // Keyboard: rounded rect with 2 rows of "keys".
            var kb = new RectangleF(cell.X + 3, cell.Bottom - 13, cell.Width - 6, 10);
            using var path = RoundedRect(kb, 2);
            g.DrawPath(glyphPen, path);
            for (int i = 0; i < 3; i++)
            {
                float kx = kb.X + 3 + i * (kb.Width - 6) / 3;
                g.DrawLine(glyphPen, kx, kb.Y + 2.5f, kx, kb.Y + 5);
            }
            g.DrawLine(glyphPen, kb.X + 4, kb.Y + 7.5f, kb.Right - 4, kb.Y + 7.5f);
        }
        else
        {
            // Mouse: oval outline + scroll wheel line.
            var ms = new RectangleF(cell.X + 4, cell.Bottom - 15, cell.Width - 8, 13);
            g.DrawEllipse(glyphPen, ms);
            g.DrawLine(glyphPen, cell.X + cell.Width / 2, ms.Y + 2.5f, cell.X + cell.Width / 2, ms.Y + 6.5f);
        }

        // Charging bolt across the cell center.
        if (charging)
        {
            using var boltPen = new Pen(Color.DeepSkyBlue, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            float cx = cell.X + cell.Width / 2;
            g.DrawLines(boltPen, new PointF[]
            {
                new(cx + 3, cell.Y + 14), new(cx - 3, cell.Y + 26),
                new(cx + 1, cell.Y + 26), new(cx - 3, cell.Y + 38)
            });
        }

        // Unknown/disconnected marker: medium-gray fill + white bold "?"
        // stays legible at the tray's real 16 px size. Grayscale
        // antialiasing avoids ClearType color fringes on transparent pixels.
        if (display is null || percent is null)
        {
            using var unknownBrush = new SolidBrush(Color.FromArgb(110, 110, 110));
            g.FillRectangle(unknownBrush, body.X + 2.5f, body.Y + 2.5f, body.Width - 5, body.Height - 5);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var font = new Font("Segoe UI", 26f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("?", font, Brushes.White, new RectangleF(cell.X, cell.Y + 4, cell.Width, cell.Height - 8), sf);
        }
    }

    private static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);

    private static void DestroyIconSafe(IntPtr handle)
    {
        try { DestroyIcon(handle); } catch { /* best effort */ }
    }
}
