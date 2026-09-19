namespace BatteryTool.Core;

public sealed record BatteryDisplay(BatteryReading Reading, int? Percent, bool? Charging,
    DateTimeOffset? LastSuccess, bool IsStale);

/// <summary>Keeps successful values in memory only; never represents a failed read as fresh data.</summary>
public sealed class BatteryHistory
{
    private readonly Dictionary<string, BatteryReading> _lastGood = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<BatteryDisplay> Update(IReadOnlyList<BatteryReading> readings)
    {
        var output = new List<BatteryDisplay>();
        foreach (var reading in readings)
        {
            if (reading.State == BatteryState.Ready && reading.Percent is >= 1 and <= 100)
                _lastGood[reading.Id] = reading;
            _lastGood.TryGetValue(reading.Id, out var previous);
            bool fresh = reading.State == BatteryState.Ready;
            output.Add(new(reading, fresh ? reading.Percent : previous?.Percent,
                fresh ? reading.Charging : null, previous?.CheckedAt, !fresh && previous is not null));
        }
        var present = readings.Select(r => r.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        // Removed devices remain visible as disconnected, with an explicitly stale last reading.
        foreach (var old in _lastGood.Values.Where(r => !present.Contains(r.Id)))
            output.Add(new(old with { State = BatteryState.Disconnected, Percent = null, Charging = null,
                Detail = I18n.DeviceDisconnected, CheckedAt = DateTimeOffset.Now },
                old.Percent, null, old.CheckedAt, true));
        return output;
    }
}
