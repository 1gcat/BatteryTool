namespace BatteryTool.Winform
{
    using BatteryTool.Core;

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            // Debug helper: render tray icon previews instead of starting the UI.
            if (args.Any(a => a.Equals("--preview-tray", StringComparison.OrdinalIgnoreCase)))
            {
                TrayPreview.Run(args);
                return;
            }

            // A second copy would show a second tray icon with stale values.
            using var mutex = new Mutex(initiallyOwned: true, @"Local\BatteryTool-SingleInstance", out bool firstInstance);
            if (!firstInstance) return;

            // Demo mode: simulated keyboard + mouse, no real HID hardware needed.
            BatteryReaderLike reader = args.Any(a => a.Equals("--mock", StringComparison.OrdinalIgnoreCase))
                ? new MockBatteryReader()
                : new RealBatteryReaderAdapter(new BatteryReader(msg => System.Diagnostics.Trace.WriteLine(msg)));

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(reader));
        }
    }
}