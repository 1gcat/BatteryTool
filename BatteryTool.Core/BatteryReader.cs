using System.Diagnostics;
using HidSharp;

namespace BatteryTool.Core;

/// <summary>Only battery queries are sent. No lighting, pairing, DPI or power settings are written.</summary>
public sealed class BatteryReader(Action<string>? log = null)
{
    public IReadOnlyList<BatteryReading> ReadAll(CancellationToken cancellationToken = default,
        DeviceKind? onlyKind = null)
    {
        var results = new List<BatteryReading>();
        var devices = DeviceList.Local.GetHidDevices().ToArray();
        foreach (var device in devices.Where(d =>
            (d.VendorID == 0x046A && (onlyKind is null or DeviceKind.CherryKeyboard)) ||
            (d.VendorID == 0x0B05 && (onlyKind is null or DeviceKind.RogMouse))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = device.DevicePath;
            if (device.VendorID == 0x046A && BatteryProtocol.IsSupportedCherryChannel(device.ProductID, path))
                results.Add(ReadDevice(device, device.ProductID == 0x00EA ? I18n.CherryMx3 : I18n.CherryMx2,
                    DeviceKind.CherryKeyboard, null, cancellationToken));
            else if (device.VendorID == 0x0B05)
            {
                var profile = BatteryProtocol.Mice.FirstOrDefault(p => p.ProductId == device.ProductID &&
                    path.Contains(p.Interface, StringComparison.OrdinalIgnoreCase));
                if (profile is not null)
                    results.Add(ReadDevice(device, I18n.DisplayDeviceName(profile.Name), DeviceKind.RogMouse, profile, cancellationToken));
            }
        }

        if ((onlyKind is null or DeviceKind.CherryKeyboard) && !results.Any(r => r.Kind == DeviceKind.CherryKeyboard))
        {
            var cherry = devices.FirstOrDefault(d => d.VendorID == 0x046A);
            results.Add(new("cherry-unavailable", I18n.CherryKeyboard, DeviceKind.CherryKeyboard,
                cherry is null ? BatteryState.Disconnected : BatteryState.Unsupported, null, null,
                cherry is null ? I18n.CherryNotFound : I18n.CherryUnsupportedChannel(cherry.ProductID),
                DateTimeOffset.Now));
        }
        if ((onlyKind is null or DeviceKind.RogMouse) && !results.Any(r => r.Kind == DeviceKind.RogMouse))
            results.Add(new("rog-unavailable", I18n.RogMouse, DeviceKind.RogMouse, BatteryState.Disconnected,
                null, null, I18n.RogNotFound, DateTimeOffset.Now));
        return results;
    }

    private BatteryReading ReadDevice(HidDevice device, string name, DeviceKind kind,
        MouseProfile? profile, CancellationToken token)
    {
        string id = device.DevicePath;
        string descriptor = $"VID {device.VendorID:X4} / PID {device.ProductID:X4}";
        try
        {
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Interruptible, true);
            config.SetOption(OpenOption.Exclusive, false);
            using var stream = device.Open(config);
            stream.ReadTimeout = 20;
            stream.WriteTimeout = 500;
            byte[] buffer = new byte[Math.Max(256, device.GetMaxInputReportLength())];
            // Bound drain time even when unsolicited input keeps arriving.
            var drainTime = Stopwatch.StartNew();
            for (int i = 0; i < 16 && drainTime.ElapsedMilliseconds < 100; i++)
            {
                token.ThrowIfCancellationRequested();
                try { stream.Read(buffer); }
                catch (TimeoutException) { break; }
            }
            stream.ReadTimeout = 300;
            byte[] query = profile is null ? BatteryProtocol.CherryQuery() : BatteryProtocol.MouseQuery(profile);
            int attempts = profile is null ? 1 : 3;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                token.ThrowIfCancellationRequested();
                stream.Write(query);
                stream.Flush();
                log?.Invoke($"{name} {descriptor} TX {Convert.ToHexString(query)}");
                var deadline = Stopwatch.StartNew();
                while (deadline.ElapsedMilliseconds < (profile is null ? 3000 : 900))
                {
                    token.ThrowIfCancellationRequested();
                    int count;
                    try { count = stream.Read(buffer); }
                    catch (TimeoutException) { continue; }
                    if (count == 0) continue;
                    log?.Invoke($"{name} RX {Convert.ToHexString(buffer.AsSpan(0, count))}");
                    if (profile is not null && count >= 3 && buffer[0] == profile.ReportId && buffer[1] == 0xFF && buffer[2] == 0xAA)
                        // G-Helper treats FF AA on a battery query as "device not ready" (asleep/off),
                        // not a firmware error; the dongle answers but the mouse does not.
                        return Result(BatteryState.NoResponse, I18n.MouseAsleep);
                    int percent;
                    bool charging;
                    bool valid = profile is null
                        ? BatteryProtocol.TryParseCherry(buffer.AsSpan(0, count), out percent, out charging)
                        : BatteryProtocol.TryParseMouse(buffer.AsSpan(0, count), profile, out percent, out charging);
                    if (valid)
                        return new(id, name, kind, BatteryState.Ready, percent, charging, descriptor, DateTimeOffset.Now);
                }
            }
            return Result(BatteryState.NoResponse, I18n.NoValidBattery);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            log?.Invoke($"{name} {descriptor} ERROR {ex.Message}");
            return Result(BatteryState.Error, I18n.ReadFailedDetail(ex.Message));
        }

        BatteryReading Result(BatteryState state, string detail) =>
            new(id, name, kind, state, null, null, I18n.JoinDetail(descriptor, detail), DateTimeOffset.Now);
    }
}
