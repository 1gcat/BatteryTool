namespace BatteryTool.Winform
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                StopMonitoring();
                components.Dispose();
                _trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            listViewDevices = new ListView();
            columnDevice = new ColumnHeader();
            columnBattery = new ColumnHeader();
            columnStatus = new ColumnHeader();
            columnUpdated = new ColumnHeader();
            buttonRefresh = new Button();
            labelInterval = new Label();
            comboBoxInterval = new ComboBox();
            comboBoxLanguage = new ComboBox();
            labelDetail = new Label();
            checkBoxMinimize = new CheckBox();
            checkBoxAutoStart = new CheckBox();
            labelTrayLegend = new Label();
            buttonTray = new Button();
            notifyIcon = new NotifyIcon(components);
            statusStrip = new StatusStrip();
            toolStripStatus = new ToolStripStatusLabel();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // listViewDevices
            // 
            listViewDevices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewDevices.Columns.AddRange(new ColumnHeader[] { columnDevice, columnBattery, columnStatus, columnUpdated });
            listViewDevices.FullRowSelect = true;
            listViewDevices.Location = new Point(12, 12);
            listViewDevices.MultiSelect = false;
            listViewDevices.Name = "listViewDevices";
            listViewDevices.ShowItemToolTips = true;
            listViewDevices.Size = new Size(700, 220);
            listViewDevices.TabIndex = 0;
            listViewDevices.UseCompatibleStateImageBehavior = false;
            listViewDevices.View = View.Details;
            listViewDevices.SelectedIndexChanged += ListViewDevices_SelectedIndexChanged;
            // 
            // columnDevice
            // 
            columnDevice.Text = "设备";
            columnDevice.Width = 240;
            // 
            // columnBattery
            // 
            columnBattery.Text = "电量";
            columnBattery.Width = 65;
            // 
            // columnStatus
            // 
            columnStatus.Text = "状态";
            columnStatus.Width = 230;
            // 
            // columnUpdated
            // 
            columnUpdated.Text = "上次成功读取";
            columnUpdated.Width = 140;
            // 
            // buttonRefresh
            // 
            buttonRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonRefresh.Location = new Point(12, 316);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new Size(100, 29);
            buttonRefresh.TabIndex = 1;
            buttonRefresh.Text = "立即刷新";
            buttonRefresh.Click += ButtonRefresh_Click;
            // 
            // labelInterval
            // 
            labelInterval.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelInterval.AutoSize = true;
            labelInterval.Location = new Point(118, 322);
            labelInterval.Name = "labelInterval";
            labelInterval.Size = new Size(68, 17);
            labelInterval.TabIndex = 2;
            labelInterval.Text = "自动刷新：";
            // 
            // comboBoxInterval
            // 
            comboBoxInterval.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            comboBoxInterval.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxInterval.Items.AddRange(new object[] { "5 秒", "10 秒", "20 秒", "30 秒", "60 秒", "120 秒", "关闭" });
            comboBoxInterval.Location = new Point(220, 317);
            comboBoxInterval.Name = "comboBoxInterval";
            comboBoxInterval.Size = new Size(110, 25);
            comboBoxInterval.TabIndex = 2;
            comboBoxInterval.SelectedIndexChanged += ComboBoxInterval_SelectedIndexChanged;
            // 
            // comboBoxLanguage
            // 
            comboBoxLanguage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLanguage.Items.AddRange(new object[] { "中文", "English" });
            comboBoxLanguage.Location = new Point(340, 317);
            comboBoxLanguage.Name = "comboBoxLanguage";
            comboBoxLanguage.Size = new Size(100, 25);
            comboBoxLanguage.TabIndex = 3;
            comboBoxLanguage.SelectedIndexChanged += ComboBoxLanguage_SelectedIndexChanged;
            // 
            // labelDetail
            // 
            labelDetail.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelDetail.Location = new Point(12, 240);
            labelDetail.Name = "labelDetail";
            labelDetail.Size = new Size(700, 70);
            labelDetail.TabIndex = 4;
            labelDetail.Text = "选择设备可查看连接状态和故障说明。";
            // 
            // checkBoxMinimize
            // 
            checkBoxMinimize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBoxMinimize.AutoSize = true;
            checkBoxMinimize.Location = new Point(12, 361);
            checkBoxMinimize.Name = "checkBoxMinimize";
            checkBoxMinimize.Size = new Size(135, 21);
            checkBoxMinimize.TabIndex = 5;
            checkBoxMinimize.Text = "启动时最小化到托盘";
            checkBoxMinimize.CheckedChanged += CheckBoxMinimize_CheckedChanged;
            // 
            // checkBoxAutoStart
            // 
            checkBoxAutoStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBoxAutoStart.AutoSize = true;
            checkBoxAutoStart.Location = new Point(300, 361);
            checkBoxAutoStart.Name = "checkBoxAutoStart";
            checkBoxAutoStart.Size = new Size(207, 21);
            checkBoxAutoStart.TabIndex = 6;
            checkBoxAutoStart.Text = "开机自动启动（当前用户登录时）";
            checkBoxAutoStart.CheckedChanged += CheckBoxAutoStart_CheckedChanged;
            // 
            // labelTrayLegend
            // 
            labelTrayLegend.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelTrayLegend.Location = new Point(12, 392);
            labelTrayLegend.Name = "labelTrayLegend";
            labelTrayLegend.Size = new Size(700, 32);
            labelTrayLegend.TabIndex = 7;
            labelTrayLegend.Text = "托盘：左键盘 / 右鼠标 · 绿 >50% / 黄 ≤50% / 红 ≤20% · 灰：旧数据 · ?：未知 · 蓝：充电";
            // 
            // buttonTray
            // 
            buttonTray.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonTray.Location = new Point(574, 316);
            buttonTray.Name = "buttonTray";
            buttonTray.Size = new Size(138, 29);
            buttonTray.TabIndex = 4;
            buttonTray.Text = "最小化到托盘";
            buttonTray.Click += ButtonTray_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.Text = "BatteryTool";
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatus });
            statusStrip.Location = new Point(0, 424);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(724, 22);
            statusStrip.TabIndex = 8;
            // 
            // toolStripStatus
            // 
            toolStripStatus.Name = "toolStripStatus";
            toolStripStatus.Size = new Size(32, 17);
            toolStripStatus.Text = "就绪";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(724, 446);
            Controls.Add(listViewDevices);
            Controls.Add(buttonRefresh);
            Controls.Add(labelInterval);
            Controls.Add(comboBoxInterval);
            Controls.Add(comboBoxLanguage);
            Controls.Add(labelDetail);
            Controls.Add(checkBoxMinimize);
            Controls.Add(checkBoxAutoStart);
            Controls.Add(labelTrayLegend);
            Controls.Add(buttonTray);
            Controls.Add(statusStrip);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "BatteryTool — 外设电量";
            FormClosing += Form1_FormClosing;
            Resize += Form1_Resize;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listViewDevices;
        private ColumnHeader columnDevice;
        private ColumnHeader columnBattery;
        private ColumnHeader columnStatus;
        private ColumnHeader columnUpdated;
        private Button buttonRefresh;
        private Label labelInterval;
        private ComboBox comboBoxInterval;
        private ComboBox comboBoxLanguage;
        private Label labelDetail;
        private CheckBox checkBoxMinimize;
        private CheckBox checkBoxAutoStart;
        private Label labelTrayLegend;
        private Button buttonTray;
        private NotifyIcon notifyIcon;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatus;
    }
}
