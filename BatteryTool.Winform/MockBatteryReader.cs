using BatteryTool.Core;

namespace BatteryTool.Winform;

/// <summary>
/// Demo data source that simulates one keyboard and one mouse without any
/// real HID hardware, so the UI and tray icon can be previewed on any PC.
/// Run with: BatteryTool.Winform --mock
/// The battery levels drift down slowly (charging cycles up), and the mouse
/// occasionally "sleeps" to exercise the stale/unknown rendering paths.
/// </summary>
internal sealed class MockBatteryReader : BatteryReaderLike
{
    private int _tick;

    public IReadOnlyList<BatteryReading> ReadAll(CancellationToken cancellationToken = default)
    {
        _tick++;
        // Keyboard: drains 100% -> 20% over ~20 refreshes, then "recharges".
        int cycle = _tick % 40;
        int kbPercent = cycle < 20 ? 100 - cycle * 4 : 20 + (cycle - 20) * 4;
        bool kbCharging = cycle >= 20;

        // Mouse: stable range with an occasional sleep period (no response).
        int mousePercent = 55 - (_tick % 8);
        bool mouseAsleep = _tick % 11 == 0;

        return new[]
        {
            new BatteryReading("mock-keyboard", I18n.MockKeyboard, DeviceKind.CherryKeyboard,
                BatteryState.Ready, kbPercent, kbCharging, I18n.MockKeyboardDetail,
                DateTimeOffset.Now),
            mouseAsleep
                ? new BatteryReading("mock-mouse", I18n.MockMouse, DeviceKind.RogMouse,
                    BatteryState.NoResponse, null, null,
                    I18n.MockMouseSleep,
                    DateTimeOffset.Now)
                : new BatteryReading("mock-mouse", I18n.MockMouse, DeviceKind.RogMouse,
                    BatteryState.Ready, mousePercent, false, I18n.MockMouseDetail,
                    DateTimeOffset.Now),
        };
    }
}

/// <summary>Minimal contract shared by the real reader and the mock reader.</summary>
internal interface BatteryReaderLike
{
    IReadOnlyList<BatteryReading> ReadAll(CancellationToken cancellationToken = default);
}

internal sealed class RealBatteryReaderAdapter : BatteryReaderLike
{
    private readonly BatteryReader _reader;
    public RealBatteryReaderAdapter(BatteryReader reader) => _reader = reader;
    public IReadOnlyList<BatteryReading> ReadAll(CancellationToken cancellationToken = default) =>
        _reader.ReadAll(cancellationToken);
}
