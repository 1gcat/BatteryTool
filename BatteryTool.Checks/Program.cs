using BatteryTool.Core;

// Manual smoke checks for the battery protocol layer. Run from a terminal:
//   dotnet run --project BatteryTool.Checks
// Exit code 0 = all checks passed.

var systemLanguage = I18n.DetectFromSystem();
var failures = new List<string>();
void Check(string name, bool ok, string detail = "")
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}{(detail.Length > 0 ? $"  ({detail})" : "")}");
    if (!ok) failures.Add(name);
}

// --- Cherry protocol -------------------------------------------------------
var cherryQuery = BatteryProtocol.CherryQuery();
Check("Cherry 查询包为 64 字节", cherryQuery.Length == 64);
Check("Cherry 查询命令头", cherryQuery[0] == 4 && cherryQuery[1] == 0x20 && cherryQuery[3] == 0x1A && cherryQuery[4] == 6);

Check("Cherry 解析：正常电量", BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20, 0, 0x1A, 6, 0, 0, 0, 87, 0 }, out var p, out var c) && p == 87 && !c);
Check("Cherry 解析：充电中", BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20, 0, 0x1A, 6, 0, 0, 0, 100, 1 }, out p, out c) && p == 100 && c);
Check("Cherry 解析：0 视为无效", !BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20, 0, 0x1A, 6, 0, 0, 0, 0, 0 }, out _, out _));
Check("Cherry 解析：>100 视为无效", !BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20, 0, 0x1A, 6, 0, 0, 0, 101, 0 }, out _, out _));
Check("Cherry 解析：响应过短", !BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20 }, out _, out _));
Check("Cherry 解析：命令头不匹配", !BatteryProtocol.TryParseCherry(new byte[] { 5, 0x20, 0, 0x1A, 6, 0, 0, 0, 50, 0 }, out _, out _));
Check("Cherry 解析：状态字节 FF 视为错误", !BatteryProtocol.TryParseCherry(new byte[] { 4, 0x20, 0, 0x1A, 6, 0, 0, 0xFF, 77, 0 }, out _, out _));
Check("Cherry 解析：MX3 抓包样本 77%", BatteryProtocol.TryParseCherry(
    Convert.FromHexString("0420001A060000004D00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"), out p, out c) && p == 77 && !c);
Check("Cherry 通道：00EA col04 支持", BatteryProtocol.IsSupportedCherryChannel(0x00EA, "hid#vid_046a&pid_00ea&col04#..."));
Check("Cherry 通道：00EA col01 不支持", !BatteryProtocol.IsSupportedCherryChannel(0x00EA, "hid#vid_046a&pid_00ea&col01#..."));
Check("Cherry 通道：01AC col04 支持", BatteryProtocol.IsSupportedCherryChannel(0x01AC, "hid#vid_046a&pid_01ac&col04#..."));
Check("Cherry 通道：其他 PID 不支持", !BatteryProtocol.IsSupportedCherryChannel(0x1234, "hid#vid_046a&pid_1234&col04#..."));

// --- ROG mouse protocol ----------------------------------------------------
var omni = BatteryProtocol.Mice.First(m => m.ProductId == 0x1ACE);
Check("OMNI 接收器接口/报告ID/包长", omni.Interface == "mi_02&col03" && omni.ReportId == 3 && omni.PacketLength == 64);
var omniQuery = BatteryProtocol.MouseQuery(omni);
Check("OMNI 查询包内容", omniQuery.Length == 64 && omniQuery[0] == 3 && omniQuery[1] == 0x12 && omniQuery[2] == 7);

Check("ROG 解析：正常电量", BatteryProtocol.TryParseMouse(new byte[] { 3, 0x12, 7, 0, 0, 66, 0, 0, 0, 0, 0 }, omni, out p, out c) && p == 66 && !c);
Check("ROG 解析：充电中", BatteryProtocol.TryParseMouse(new byte[] { 3, 0x12, 7, 0, 0, 66, 0, 0, 0, 0, 1 }, omni, out p, out c) && p == 66 && c);
Check("ROG 解析：0 视为休眠", !BatteryProtocol.TryParseMouse(new byte[] { 3, 0x12, 7, 0, 0, 0, 0, 0, 0, 0, 0 }, omni, out _, out _));
Check("ROG 解析：>100 视为无效", !BatteryProtocol.TryParseMouse(new byte[] { 3, 0x12, 7, 0, 0, 101, 0, 0, 0, 0, 0 }, omni, out _, out _));
Check("ROG 解析：报告ID不匹配", !BatteryProtocol.TryParseMouse(new byte[] { 4, 0x12, 7, 0, 0, 66, 0, 0, 0, 0, 0 }, omni, out _, out _));
Check("ROG 解析：FF AA 错误帧不解析", !BatteryProtocol.TryParseMouse(new byte[] { 3, 0xFF, 0xAA, 0, 0, 66, 0, 0, 0, 0, 0 }, omni, out _, out _));

var scaled = BatteryProtocol.Mice.First(m => m.ProductId == 0x18E5);
Check("ROG 解析：Scale=25 型号", BatteryProtocol.TryParseMouse(new byte[] { 0, 0x12, 7, 0, 0, 3, 0, 0, 0, 0, 0 }, scaled, out p, out c) && p == 75);
var carry = BatteryProtocol.Mice.First(m => m.ProductId == 0x18B4);
Check("ROG 解析：Strix Carry 偏移7", BatteryProtocol.TryParseMouse(new byte[] { 0, 0x12, 7, 0, 0, 0, 0, 4, 0, 0, 0 }, carry, out p, out c) && p == 100);

// --- History ---------------------------------------------------------------
var history = new BatteryHistory();
var good = new BatteryReading("dev1", "测试鼠标", DeviceKind.RogMouse, BatteryState.Ready, 80, false, "ok", DateTimeOffset.Now);
var first = history.Update(new[] { good });
Check("历史：首次成功读数", first.Count == 1 && first[0].Percent == 80 && !first[0].IsStale);
var fail = good with { State = BatteryState.NoResponse, Percent = null, Charging = null };
var second = history.Update(new[] { fail });
Check("历史：失败时保留旧值并标记", second[0].Percent == 80 && second[0].IsStale);
var third = history.Update(Array.Empty<BatteryReading>());
Check("历史：设备消失后仍显示旧电量", third.Count == 1 && third[0].Percent == 80 && third[0].Reading.State == BatteryState.Disconnected);

// --- I18n ------------------------------------------------------------------
I18n.Set("en");
Check("I18n English title", I18n.FormTitle.Contains("Peripheral"));
Check("I18n English OMNI name", I18n.DisplayDeviceName("ROG Mouse (OMNI Receiver)") == "ROG Mouse (OMNI Receiver)");
I18n.Set("zh");
Check("I18n Chinese title", I18n.FormTitle.Contains("外设"));
Check("I18n Chinese OMNI name", I18n.DisplayDeviceName("ROG Mouse (OMNI Receiver)") == "ROG 鼠标（OMNI 接收器）");
I18n.Set(systemLanguage);

// --- Live HID probe (informational, never fails the run) -------------------
Console.WriteLine();
Console.WriteLine("== 本机 HID 探测（仅信息展示） ==");
try
{
    var reader = new BatteryReader(msg => Console.WriteLine($"  log: {msg}"));
    foreach (var reading in reader.ReadAll(onlyKind: DeviceKind.CherryKeyboard))
        Console.WriteLine($"  [{reading.State}] {reading.Name}: {reading.Percent?.ToString() ?? "-"}% " +
            $"charging={reading.Charging?.ToString() ?? "-"} | {reading.Detail}");
}
catch (Exception ex)
{
    Console.WriteLine($"  探测异常：{ex.Message}");
}

Console.WriteLine();
if (failures.Count > 0)
{
    Console.WriteLine($"共 {failures.Count} 项失败：{string.Join(", ", failures)}");
    return 1;
}
Console.WriteLine("全部检查通过。");
return 0;
