#nullable enable
namespace IPTester;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    // ── Top bar ──
    private Panel pnlTop;
    private Label lblTitle;
    private Label lblSubtitle;

    // ── Input row ──
    private TextBox txtTarget;
    private Label lblTarget;
    private Button btnDetectLAN;
    private Button btnScan;
    private Button btnStop;

    // ── Options row ──
    private CheckBox chkScanPorts;
    private ComboBox cmbPortPreset;
    private Label lblPortPreset;
    private TextBox txtCustomPorts;
    private Label lblCustomPorts;
    private Label lblTimeout;
    private NumericUpDown numTimeout;
    private CheckBox chkHostname;
    private CheckBox chkProfiling;
    private CheckBox chkMacVendor;

    // ── Progress ──
    private ProgressBar progressBar;
    private Label lblProgress;

    // ── Results ──
    private DataGridView gridResults;
    private Panel pnlBottom;
    private Label lblSummary;
    private Button btnSnapshot;
    private Button btnExport;
    private Button btnCompare;

    // ── Snapshot dialog components ──
    private string? _lastSnapshotPath;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlTop = new Panel();
        lblTitle = new Label();
        lblSubtitle = new Label();
        txtTarget = new TextBox();
        lblTarget = new Label();
        btnDetectLAN = new Button();
        btnScan = new Button();
        btnStop = new Button();
        chkScanPorts = new CheckBox();
        cmbPortPreset = new ComboBox();
        lblPortPreset = new Label();
        txtCustomPorts = new TextBox();
        lblCustomPorts = new Label();
        lblTimeout = new Label();
        numTimeout = new NumericUpDown();
        chkHostname = new CheckBox();
        chkProfiling = new CheckBox();
        chkMacVendor = new CheckBox();
        progressBar = new ProgressBar();
        lblProgress = new Label();
        gridResults = new DataGridView();
        pnlBottom = new Panel();
        lblSummary = new Label();
        btnSnapshot = new Button();
        btnExport = new Button();
        btnCompare = new Button();

        pnlTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numTimeout).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridResults).BeginInit();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // ═══════════════════════════════════════════
        //  FORM
        // ═══════════════════════════════════════════
        Text = "IP Test Tool · 007";
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 780);
        MinimumSize = new Size(1000, 650);
        Font = new Font("Microsoft YaHei UI", 10F);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 246, 250);

        // ═══════════════════════════════════════════
        //  TOP BAR
        // ═══════════════════════════════════════════
        pnlTop.BackColor = Color.FromArgb(30, 33, 40);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 56;
        pnlTop.Padding = new Padding(20, 0, 20, 0);

        lblTitle.Text = "🔍 IP Test Tool · 007";
        lblTitle.ForeColor = Color.White;
        lblTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 8);
        lblTitle.Size = new Size(300, 28);
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;

        lblSubtitle.Text = "网络诊断 · 端口扫描 · IP冲突检测 · 设备识别";
        lblSubtitle.ForeColor = Color.FromArgb(160, 165, 175);
        lblSubtitle.Font = new Font("Microsoft YaHei UI", 8.5F);
        lblSubtitle.Location = new Point(20, 34);
        lblSubtitle.Size = new Size(400, 18);
        lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;

        pnlTop.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

        // ═══════════════════════════════════════════
        //  INPUT ROW
        // ═══════════════════════════════════════════
        lblTarget.Text = "目标:";
        lblTarget.Location = new Point(20, 72);
        lblTarget.Size = new Size(48, 36);
        lblTarget.TextAlign = ContentAlignment.MiddleRight;
        lblTarget.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);

        txtTarget.Location = new Point(72, 76);
        txtTarget.Size = new Size(670, 30);
        txtTarget.Font = new Font("Consolas", 10F);
        txtTarget.PlaceholderText = "192.168.1.1  或  192.168.1.0/24  或  192.168.1.1-192.168.1.100  逗号分隔多个";
        txtTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        btnDetectLAN.Text = "🔍 局域网";
        btnDetectLAN.Location = new Point(750, 76);
        btnDetectLAN.Size = new Size(100, 32);
        btnDetectLAN.BackColor = Color.FromArgb(255, 140, 0);
        btnDetectLAN.ForeColor = Color.White;
        btnDetectLAN.FlatStyle = FlatStyle.Flat;
        btnDetectLAN.FlatAppearance.BorderSize = 0;
        btnDetectLAN.Cursor = Cursors.Hand;
        btnDetectLAN.Font = new Font("Microsoft YaHei UI", 9F);
        btnDetectLAN.Click += btnDetectLAN_Click;
        btnDetectLAN.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnScan.Text = "🚀 扫描";
        btnScan.Location = new Point(856, 76);
        btnScan.Size = new Size(100, 32);
        btnScan.BackColor = Color.FromArgb(0, 120, 212);
        btnScan.ForeColor = Color.White;
        btnScan.FlatStyle = FlatStyle.Flat;
        btnScan.FlatAppearance.BorderSize = 0;
        btnScan.Cursor = Cursors.Hand;
        btnScan.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        btnScan.Click += btnScan_Click;
        btnScan.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnStop.Text = "⏹ 停止";
        btnStop.Location = new Point(962, 76);
        btnStop.Size = new Size(100, 32);
        btnStop.BackColor = Color.FromArgb(232, 17, 35);
        btnStop.ForeColor = Color.White;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.FlatAppearance.BorderSize = 0;
        btnStop.Cursor = Cursors.Hand;
        btnStop.Font = new Font("Microsoft YaHei UI", 9F);
        btnStop.Visible = false;
        btnStop.Click += btnStop_Click;
        btnStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // ═══════════════════════════════════════════
        //  OPTIONS ROW
        // ═══════════════════════════════════════════
        chkScanPorts.Text = "端口扫描";
        chkScanPorts.Location = new Point(20, 120);
        chkScanPorts.Size = new Size(90, 24);
        chkScanPorts.Checked = true;
        chkScanPorts.Font = new Font("Microsoft YaHei UI", 9F);
        chkScanPorts.CheckedChanged += chkScanPorts_CheckedChanged;

        lblPortPreset.Text = "预设:";
        lblPortPreset.Location = new Point(110, 120);
        lblPortPreset.Size = new Size(38, 24);
        lblPortPreset.TextAlign = ContentAlignment.MiddleRight;
        lblPortPreset.Font = new Font("Microsoft YaHei UI", 9F);

        cmbPortPreset.Location = new Point(150, 120);
        cmbPortPreset.Size = new Size(120, 26);
        cmbPortPreset.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPortPreset.Font = new Font("Microsoft YaHei UI", 9F);

        lblCustomPorts.Text = "自定义:";
        lblCustomPorts.Location = new Point(278, 120);
        lblCustomPorts.Size = new Size(55, 24);
        lblCustomPorts.TextAlign = ContentAlignment.MiddleRight;
        lblCustomPorts.Font = new Font("Microsoft YaHei UI", 9F);

        txtCustomPorts.Location = new Point(336, 120);
        txtCustomPorts.Size = new Size(160, 26);
        txtCustomPorts.Font = new Font("Consolas", 9F);
        txtCustomPorts.PlaceholderText = "80,443,502";

        lblTimeout.Text = "超时:";
        lblTimeout.Location = new Point(504, 120);
        lblTimeout.Size = new Size(38, 24);
        lblTimeout.TextAlign = ContentAlignment.MiddleRight;
        lblTimeout.Font = new Font("Microsoft YaHei UI", 9F);

        numTimeout.Location = new Point(544, 120);
        numTimeout.Size = new Size(72, 26);
        numTimeout.Minimum = 100;
        numTimeout.Maximum = 10000;
        numTimeout.Value = 2000;
        numTimeout.Increment = 500;
        numTimeout.Font = new Font("Microsoft YaHei UI", 9F);

        var lblMs = new Label { Text = "ms", Location = new Point(620, 120), Size = new Size(28, 24) };

        chkHostname.Text = "主机名";
        chkHostname.Location = new Point(656, 120);
        chkHostname.Size = new Size(72, 24);
        chkHostname.Checked = true;
        chkHostname.Font = new Font("Microsoft YaHei UI", 9F);

        chkProfiling.Text = "设备推测";
        chkProfiling.Location = new Point(730, 120);
        chkProfiling.Size = new Size(85, 24);
        chkProfiling.Checked = true;
        chkProfiling.Font = new Font("Microsoft YaHei UI", 9F);

        chkMacVendor.Text = "MAC厂商";
        chkMacVendor.Location = new Point(818, 120);
        chkMacVendor.Size = new Size(88, 24);
        chkMacVendor.Checked = true;
        chkMacVendor.Font = new Font("Microsoft YaHei UI", 9F);

        // ═══════════════════════════════════════════
        //  PROGRESS
        // ═══════════════════════════════════════════
        progressBar.Location = new Point(20, 158);
        progressBar.Size = new Size(960, 8);
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblProgress.Text = "就绪";
        lblProgress.Location = new Point(990, 153);
        lblProgress.Size = new Size(190, 20);
        lblProgress.TextAlign = ContentAlignment.MiddleRight;
        lblProgress.ForeColor = Color.FromArgb(120, 120, 130);
        lblProgress.Font = new Font("Microsoft YaHei UI", 9F);
        lblProgress.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // ═══════════════════════════════════════════
        //  RESULTS GRID
        // ═══════════════════════════════════════════
        gridResults.Location = new Point(20, 176);
        gridResults.Size = new Size(1160, 500);
        gridResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        gridResults.AllowUserToAddRows = false;
        gridResults.AllowUserToDeleteRows = false;
        gridResults.AllowUserToResizeRows = false;
        gridResults.ReadOnly = true;
        gridResults.RowHeadersVisible = false;
        gridResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridResults.BackgroundColor = Color.White;
        gridResults.BorderStyle = BorderStyle.None;
        gridResults.ColumnHeadersHeight = 36;
        gridResults.RowTemplate.Height = 26;
        gridResults.GridColor = Color.FromArgb(230, 232, 238);

        gridResults.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { Name = "colIP",       HeaderText = "IP 地址",   FillWeight = 16, MinimumWidth = 110 },
            new DataGridViewTextBoxColumn { Name = "colStatus",   HeaderText = "状态",      FillWeight = 9,  MinimumWidth = 70 },
            new DataGridViewTextBoxColumn { Name = "colLatency",  HeaderText = "延迟",      FillWeight = 7,  MinimumWidth = 55 },
            new DataGridViewTextBoxColumn { Name = "colMAC",      HeaderText = "MAC 地址",  FillWeight = 16, MinimumWidth = 120 },
            new DataGridViewTextBoxColumn { Name = "colVendor",   HeaderText = "厂商",      FillWeight = 13, MinimumWidth = 100 },
            new DataGridViewTextBoxColumn { Name = "colHostname", HeaderText = "主机名",    FillWeight = 13, MinimumWidth = 100 },
            new DataGridViewTextBoxColumn { Name = "colDevice",   HeaderText = "设备类型",  FillWeight = 14, MinimumWidth = 110 },
            new DataGridViewTextBoxColumn { Name = "colPorts",    HeaderText = "开放端口",  FillWeight = 12, MinimumWidth = 90 },
        });

        gridResults.EnableHeadersVisualStyles = false;
        gridResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 241, 245);
        gridResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 70);
        gridResults.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        gridResults.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 0);
        gridResults.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F);
        gridResults.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 50);

        // ═══════════════════════════════════════════
        //  BOTTOM BAR
        // ═══════════════════════════════════════════
        pnlBottom.BackColor = Color.FromArgb(245, 246, 250);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 44;
        pnlBottom.Padding = new Padding(20, 6, 20, 6);

        lblSummary.Text = "就绪 — 输入目标IP后点击扫描";
        lblSummary.Location = new Point(20, 6);
        lblSummary.Size = new Size(700, 30);
        lblSummary.TextAlign = ContentAlignment.MiddleLeft;
        lblSummary.ForeColor = Color.FromArgb(100, 100, 110);
        lblSummary.Font = new Font("Microsoft YaHei UI", 9F);

        btnSnapshot.Text = "📸 快照";
        btnSnapshot.Size = new Size(90, 30);
        btnSnapshot.BackColor = Color.FromArgb(100, 100, 110);
        btnSnapshot.ForeColor = Color.White;
        btnSnapshot.FlatStyle = FlatStyle.Flat;
        btnSnapshot.FlatAppearance.BorderSize = 0;
        btnSnapshot.Cursor = Cursors.Hand;
        btnSnapshot.Font = new Font("Microsoft YaHei UI", 9F);
        btnSnapshot.Click += btnSnapshot_Click;
        btnSnapshot.Anchor = AnchorStyles.Right;

        btnCompare.Text = "🔄 对比";
        btnCompare.Size = new Size(90, 30);
        btnCompare.BackColor = Color.FromArgb(100, 100, 110);
        btnCompare.ForeColor = Color.White;
        btnCompare.FlatStyle = FlatStyle.Flat;
        btnCompare.FlatAppearance.BorderSize = 0;
        btnCompare.Cursor = Cursors.Hand;
        btnCompare.Font = new Font("Microsoft YaHei UI", 9F);
        btnCompare.Click += btnCompare_Click;
        btnCompare.Anchor = AnchorStyles.Right;

        btnExport.Text = "📋 导出";
        btnExport.Size = new Size(90, 30);
        btnExport.BackColor = Color.FromArgb(0, 120, 212);
        btnExport.ForeColor = Color.White;
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Cursor = Cursors.Hand;
        btnExport.Font = new Font("Microsoft YaHei UI", 9F);
        btnExport.Click += btnExport_Click;
        btnExport.Anchor = AnchorStyles.Right;

        // 底部按钮靠右排列
        var flow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Location = new Point(820, 4),
            Size = new Size(360, 34),
            Anchor = AnchorStyles.Right,
        };
        flow.Controls.AddRange(new Control[] { btnExport, btnCompare, btnSnapshot });
        pnlBottom.Controls.AddRange(new Control[] { lblSummary, flow });

        // ═══════════════════════════════════════════
        //  ADD TO FORM
        // ═══════════════════════════════════════════
        Controls.AddRange(new Control[] {
            pnlTop, lblTarget, txtTarget, btnDetectLAN, btnScan, btnStop,
            chkScanPorts, lblPortPreset, cmbPortPreset, lblCustomPorts, txtCustomPorts,
            lblTimeout, numTimeout, lblMs,
            chkHostname, chkProfiling, chkMacVendor,
            progressBar, lblProgress,
            gridResults, pnlBottom
        });

        pnlTop.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numTimeout).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridResults).EndInit();
        pnlBottom.ResumeLayout(false);
        ResumeLayout(false);
    }
}
