namespace BatteryTool.Core;

public enum DeviceKind { CherryKeyboard, RogMouse }
public enum BatteryState { Ready, NoResponse, Unsupported, Disconnected, Error }

public sealed record BatteryReading(string Id, string Name, DeviceKind Kind,
    BatteryState State, int? Percent, bool? Charging, string Detail, DateTimeOffset CheckedAt);

    // Protocol facts from cherry-battery-state and G-Helper; see README.md.
public sealed record MouseProfile(int ProductId, string Name, string Interface = "mi_00",
    byte ReportId = 0, int PacketLength = 65, int BatteryOffset = 5, int Scale = 1);

public static class BatteryProtocol
{
    public static IReadOnlyList<MouseProfile> Mice { get; } = new MouseProfile[]
    {
        new(0x1ACE, "ROG Mouse (OMNI Receiver)", "mi_02&col03", 3, 64),
        new(0x1AD0, "ROG Harpe II", "mi_02&col03", 3, 64),
        new(0x1A1A, "ROG Chakram X"), new(0x1A18, "ROG Chakram X (USB)"),
        new(0x197F, "ROG Gladius III Wireless"), new(0x197D, "ROG Gladius III (USB)"),
        new(0x1A72, "ROG Gladius III Aimpoint"), new(0x1A70, "ROG Gladius III Aimpoint (USB)"),
        new(0x1B0C, "ROG Gladius III EVA-02"), new(0x1B0A, "ROG Gladius III EVA-02 (USB)"),
        new(0x1A94, "ROG Harpe Ace Aim Lab"), new(0x1A92, "ROG Harpe Ace Aim Lab (USB)"),
        new(0x1B67, "ROG Harpe Ace Extreme (USB)"),
        new(0x1A68, "ROG Keris Aimpoint"), new(0x1A66, "ROG Keris Aimpoint (USB)"),
        new(0x1B16, "ROG Keris II Ace (USB)"),
        new(0x1979, "ROG Spatha X"), new(0x1977, "ROG Spatha X (USB)"),
        new(0x18E5, "ROG Chakram", Scale: 25), new(0x18E3, "ROG Chakram (USB)", Scale: 25),
        new(0x1908, "ROG Pugio II", Scale: 25), new(0x1906, "ROG Pugio II (USB)", Scale: 25),
        new(0x1960, "ROG Keris Wireless", Scale: 25), new(0x195E, "ROG Keris (USB)", Scale: 25),
        new(0x1A59, "ROG Keris EVA", Scale: 25), new(0x1A57, "ROG Keris EVA (USB)", Scale: 25),
        new(0x1949, "ROG Strix Impact II Wireless", Scale: 25), new(0x1947, "ROG Strix Impact II (USB)", Scale: 25),
        new(0x18A0, "ROG Gladius II Wireless", "mi_02", Scale: 25),
        new(0x18B4, "ROG Strix Carry", "mi_01", BatteryOffset: 7, Scale: 25),
        new(0x1B63, "ROG Harpe Ace Mini (USB)", PacketLength: 64),
        new(0x1C69, "ROG Harpe II Ace (USB)", PacketLength: 64),
        new(0x1C0C, "ROG Keris II Origin (USB)", PacketLength: 64),
        new(0x1D4C, "ROG Keris II Origin KJP (USB)", PacketLength: 64)
    };

    public static bool IsSupportedCherryChannel(int productId, string path) =>
        productId is 0x01AC or 0x00EA && path.Contains("col04", StringComparison.OrdinalIgnoreCase);

    // MX3 00EA independently verified: cherry-address1.pcap frames 11/13,
    // SET_REPORT(Output, ID 4), 64 bytes; response payload begins at byte 8.
    public static byte[] CherryQuery()
    {
        var packet = new byte[64];
        new byte[] { 4, 0x20, 0, 0x1A, 6 }.CopyTo(packet, 0);
        return packet;
    }

    public static byte[] MouseQuery(MouseProfile profile)
    {
        var packet = new byte[profile.PacketLength];
        packet[0] = profile.ReportId;
        packet[1] = 0x12;
        packet[2] = 7;
        return packet;
    }

    public static bool TryParseCherry(ReadOnlySpan<byte> data, out int percent, out bool charging)
    {
        percent = 0; charging = false;
        // Verified response: 04 20 00 1A 06 00 00 00 <percent> <charging?> …
        // byte[3] echoes the query command; byte[7] is a status byte (FF = error).
        if (data.Length < 10 || data[0] != 4 || data[1] != 0x20 || data[3] != 0x1A ||
            data[7] == 0xFF || data[8] is 0 or > 100)
            return false;
        percent = data[8]; charging = data[9] != 0;
        return true;
    }

    public static bool TryParseMouse(ReadOnlySpan<byte> data, MouseProfile profile, out int percent, out bool charging)
    {
        percent = 0; charging = false;
        if (data.Length < 11 || data[0] != profile.ReportId || data[1] != 0x12 || data[2] != 7)
            return false;
        int value = data[profile.BatteryOffset] * profile.Scale;
        // Both references treat zero as unavailable/asleep, not a reliable 0% reading.
        if (value is <= 0 or > 100) return false;
        percent = value; charging = data[10] != 0;
        return true;
    }
}
