using BatteryTool.Core;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BatteryTool.Winform;

/// <summary>
/// Debug helper: renders the real tray icon for several device states and
/// saves PNGs for visual inspection / README docs. Run with:
///   BatteryTool.Winform --preview-tray [outputDir]
/// </summary>
internal static class TrayPreview
{
    private const int DocsSize = 128;

    public static void Run(string[] args)
    {
        // args[0] is the exe path; the flag itself is args[1], an optional
        // output directory may follow it.
        int flagIndex = Array.FindIndex(args,
            a => a.Equals("--preview-tray", StringComparison.OrdinalIgnoreCase));
        string dir = flagIndex >= 0 && flagIndex + 1 < args.Length &&
            !args[flagIndex + 1].StartsWith('-')
            ? args[flagIndex + 1]
            : Path.Combine(Path.GetTempPath(), "BatteryToolTrayPreview");
        Directory.CreateDirectory(dir);

        var scenarios = new (string File, BatteryDisplay? Keyboard, BatteryDisplay? Mouse)[]
        {
            ("normal", Make(DeviceKind.CherryKeyboard, 87, false), Make(DeviceKind.RogMouse, 66, false)),
            ("charging", Make(DeviceKind.CherryKeyboard, 100, true), Make(DeviceKind.RogMouse, 40, true)),
            ("low", Make(DeviceKind.CherryKeyboard, 15, false), Make(DeviceKind.RogMouse, 25, false)),
            ("stale", Make(DeviceKind.CherryKeyboard, 87, false, stale: true), Make(DeviceKind.RogMouse, 66, false, stale: true)),
            ("unknown", null, null),
            ("mixed", Make(DeviceKind.CherryKeyboard, 87, false), null),
        };

        foreach (var (name, keyboard, mouse) in scenarios)
        {
            using var source = TrayIconRenderer.CreateBitmap(keyboard, mouse);
            using var large = new Bitmap(DocsSize, DocsSize, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(large))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(source, 0, 0, DocsSize, DocsSize);
            }
            string path = Path.Combine(dir, $"{name}.png");
            large.Save(path, ImageFormat.Png);
            Console.WriteLine($"saved {path}");
        }
        Console.WriteLine("done");
    }

    private static BatteryDisplay Make(DeviceKind kind, int percent, bool charging, bool stale = false) =>
        new(new BatteryReading("id", kind == DeviceKind.CherryKeyboard ? I18n.CherryKeyboard : I18n.RogMouse, kind,
            stale ? BatteryState.NoResponse : BatteryState.Ready,
            stale ? null : percent, stale ? null : charging, "preview", DateTimeOffset.Now),
            percent, stale ? null : charging, DateTimeOffset.Now, stale);
}
