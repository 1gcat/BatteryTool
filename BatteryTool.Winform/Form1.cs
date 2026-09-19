using BatteryTool.Core;

namespace BatteryTool.Winform;

public partial class Form1 : Form
{
    private readonly AppSettings _settings;
    private readonly BatteryReaderLike _reader;
    private readonly BatteryHistory _history = new();
    private readonly System.Windows.Forms.Timer _timer;
    private readonly CancellationTokenSource _shutdown = new();
    private bool _initializing = true;
    private bool _busy;
    private bool _exiting;
    private Icon? _trayIcon;
    private IReadOnlyList<BatteryDisplay> _lastDisplays = Array.Empty<BatteryDisplay>();
    private ToolStripMenuItem _menuShow = null!;
    private ToolStripMenuItem _menuRefresh = null!;
    private ToolStripMenuItem _menuExit = null!;

    internal Form1(BatteryReaderLike reader)
    {
        _reader = reader;
        _settings = AppSettings.Load();
        I18n.Set(_settings.ResolveLanguage());
        InitializeComponent();
        _timer = new System.Windows.Forms.Timer(components!);
        _timer.Tick += Timer_Tick;
        comboBoxInterval.SelectedIndex = IntervalToIndex(_settings.IntervalSeconds);
        checkBoxMinimize.Checked = _settings.StartMinimized;
        checkBoxAutoStart.Checked = AutoStart.IsEnabled();
        var menu = new ContextMenuStrip(components!);
        _menuShow = new ToolStripMenuItem { Name = "menuShow" };
        _menuShow.Click += NotifyIcon_DoubleClick;
        _menuRefresh = new ToolStripMenuItem { Name = "menuRefresh" };
        _menuRefresh.Click += ButtonRefresh_Click;
        _menuExit = new ToolStripMenuItem { Name = "menuExit" };
        _menuExit.Click += (_, _) => Close();
        menu.Items.AddRange([_menuShow, _menuRefresh, new ToolStripSeparator(), _menuExit]);
        notifyIcon.ContextMenuStrip = menu;
        ApplyLanguage();
        notifyIcon.Visible = true;
        _initializing = false;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Run after the form's handle and WindowsForms synchronization context exist.
        BeginInvoke((Action)(() =>
        {
            if (_exiting) return;
            if (_settings.StartMinimized) MinimizeToTray();
            RefreshAsync();
        }));
    }

    private void ButtonRefresh_Click(object? sender, EventArgs e) => RefreshAsync();

    private void ComboBoxInterval_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_initializing) return;
        _settings.IntervalSeconds = IndexToInterval(comboBoxInterval.SelectedIndex);
        SaveSettings();
        ApplyTimer();
    }

    private void ComboBoxLanguage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_initializing) return;
        string code = comboBoxLanguage.SelectedIndex == 0 ? "zh" : "en";
        if (_settings.Language == code) return;
        _settings.Language = code;
        I18n.Set(code);
        ApplyLanguage();
        SaveSettings();
        RefreshAsync();
    }

    private void CheckBoxMinimize_CheckedChanged(object? sender, EventArgs e)
    {
        if (_initializing) return;
        _settings.StartMinimized = checkBoxMinimize.Checked;
        SaveSettings();
    }

    private void CheckBoxAutoStart_CheckedChanged(object? sender, EventArgs e)
    {
        if (_initializing) return;
        if (AutoStart.SetEnabled(checkBoxAutoStart.Checked)) return;

        _initializing = true;
        try { checkBoxAutoStart.Checked = AutoStart.IsEnabled(); }
        finally { _initializing = false; }
        MessageBox.Show(this, I18n.AutostartFailed, Text,
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void SaveSettings()
    {
        if (!_settings.Save())
            MessageBox.Show(this, I18n.SettingsSaveFailed, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ApplyLanguage()
    {
        bool previous = _initializing;
        _initializing = true;
        try
        {
            Text = I18n.FormTitle;
            columnDevice.Text = I18n.ColumnDevice;
            columnBattery.Text = I18n.ColumnBattery;
            columnStatus.Text = I18n.ColumnStatus;
            columnUpdated.Text = I18n.ColumnUpdated;
            buttonRefresh.Text = I18n.Refresh;
            labelInterval.Text = I18n.AutoRefresh;
            int intervalIndex = comboBoxInterval.SelectedIndex;
            comboBoxInterval.Items.Clear();
            comboBoxInterval.Items.AddRange(I18n.IntervalItems);
            if (intervalIndex >= 0 && intervalIndex < comboBoxInterval.Items.Count)
                comboBoxInterval.SelectedIndex = intervalIndex;
            labelDetail.Text = I18n.DetailHint;
            checkBoxMinimize.Text = I18n.StartMinimized;
            checkBoxAutoStart.Text = I18n.AutoStart;
            labelTrayLegend.Text = I18n.TrayLegend;
            buttonTray.Text = I18n.MinimizeToTray;
            if (!_busy)
                toolStripStatus.Text = I18n.StatusReady;
            _menuShow.Text = I18n.MenuShow;
            _menuRefresh.Text = I18n.MenuRefresh;
            _menuExit.Text = I18n.MenuExit;
            comboBoxLanguage.SelectedIndex = I18n.IsZh ? 0 : 1;
            UpdateList(_lastDisplays);
            UpdateTray(_lastDisplays);
        }
        finally { _initializing = previous; }
    }

    private void ApplyTimer()
    {
        _timer.Stop();
        if (!_exiting && !_busy && _settings.IntervalSeconds > 0)
        {
            _timer.Interval = _settings.IntervalSeconds * 1000;
            _timer.Start();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e) => RefreshAsync();

    private async void RefreshAsync()
    {
        if (_busy || _exiting) return;
        _busy = true;
        _timer.Stop();
        buttonRefresh.Enabled = false;
        toolStripStatus.Text = I18n.StatusReading;
        try
        {
            var token = _shutdown.Token;
            var readings = await Task.Run(() => _reader.ReadAll(token), token);
            if (_exiting || IsDisposed) return;
            var displays = _history.Update(readings);
            UpdateList(displays);
            int ready = readings.Count(r => r.State == BatteryState.Ready);
            toolStripStatus.Text = I18n.StatusDone(ready, DateTime.Now);
            UpdateTray(displays);
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!_exiting && !IsDisposed)
            {
                toolStripStatus.Text = I18n.StatusReadFailed(ex.Message);
                // Do not leave the tray showing apparently fresh values after a failed scan.
                UpdateTray(_history.Update(Array.Empty<BatteryReading>()));
            }
        }
        finally
        {
            _busy = false;
            if (!_exiting && !IsDisposed)
            {
                buttonRefresh.Enabled = true;
                ApplyTimer();
            }
        }
    }

    private void UpdateTray(IReadOnlyList<BatteryDisplay> displays)
    {
        _lastDisplays = displays;
        var keyboard = TrayIconRenderer.SelectDisplay(displays, DeviceKind.CherryKeyboard);
        var mouse = TrayIconRenderer.SelectDisplay(displays, DeviceKind.RogMouse);
        var next = TrayIconRenderer.CreateIcon(keyboard, mouse);
        var previous = _trayIcon;
        notifyIcon.Icon = next;
        _trayIcon = next;
        previous?.Dispose();
        string tip = I18n.TrayText(Describe(keyboard), Describe(mouse));
        notifyIcon.Text = tip.Length <= 63 ? tip : tip[..63];

        static string Describe(BatteryDisplay? display) => display?.Percent is int percent
            ? I18n.DescribePercent(percent, display.IsStale, display.Charging == true)
            : I18n.Unknown;
    }

    private void UpdateList(IReadOnlyList<BatteryDisplay> displays)
    {
        _lastDisplays = displays;
        string? selectedId = (listViewDevices.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.Tag as BatteryDisplay)?.Reading.Id;
        listViewDevices.BeginUpdate();
        try
        {
            listViewDevices.Items.Clear();
            foreach (var display in displays)
            {
                string state = display.Reading.State switch
                {
                    BatteryState.Ready => display.Charging == true ? I18n.StateCharging : display.Percent <= 20 ? I18n.StateLow : I18n.StateOk,
                    BatteryState.NoResponse => I18n.StateNoResponse,
                    BatteryState.Unsupported => I18n.StateUnsupported,
                    BatteryState.Disconnected => I18n.StateDisconnected,
                    _ => I18n.StateError
                };
                var item = new ListViewItem(display.Reading.Name) { Tag = display, ToolTipText = display.Reading.Detail };
                item.SubItems.Add(display.Percent is int p ? $"{p}%" : "—");
                item.SubItems.Add(state + (display.IsStale ? I18n.StaleSuffix : ""));
                item.SubItems.Add(display.LastSuccess?.LocalDateTime.ToString("MM-dd HH:mm:ss") ?? "—");
                listViewDevices.Items.Add(item);
                item.Selected = display.Reading.Id == selectedId;
            }
            if (listViewDevices.SelectedItems.Count == 0 && listViewDevices.Items.Count > 0)
                listViewDevices.Items[0].Selected = true;
        }
        finally { listViewDevices.EndUpdate(); }
        ListViewDevices_SelectedIndexChanged(this, EventArgs.Empty);
    }

    private void ListViewDevices_SelectedIndexChanged(object? sender, EventArgs e)
    {
        labelDetail.Text = listViewDevices.SelectedItems.Count > 0 && listViewDevices.SelectedItems[0].Tag is BatteryDisplay display
            ? display.Reading.Detail : I18n.DetailHint;
    }

    private void ButtonTray_Click(object? sender, EventArgs e) => MinimizeToTray();

    private void Form1_Resize(object? sender, EventArgs e)
    {
        if (!_initializing && !_exiting && WindowState == FormWindowState.Minimized) MinimizeToTray();
    }

    private void MinimizeToTray()
    {
        notifyIcon.Visible = true;
        Hide();
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        notifyIcon.Visible = true;
        Activate();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Closing exits; minimizing keeps background monitoring active.
        if (e.Cancel) return;
        StopMonitoring();
    }

    private void StopMonitoring()
    {
        if (_exiting) return;
        _exiting = true;
        _timer?.Stop();
        _shutdown.Cancel();
        notifyIcon.Visible = false;
    }

    private static int IndexToInterval(int index) => index switch
    {
        0 => 5, 1 => 10, 2 => 20, 3 => 30, 4 => 60, 5 => 120, _ => 0
    };

    private static int IntervalToIndex(int seconds) => seconds switch
    {
        5 => 0, 10 => 1, 20 => 2, 30 => 3, 60 => 4, 120 => 5, 0 => 6, _ => 3
    };
}
