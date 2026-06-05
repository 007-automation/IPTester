using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IPTester;

public partial class MainForm : Form
{
    private CancellationTokenSource? _cts;
    private readonly Dictionary<string, int[]> _portProfiles = new()
    {
        ["常用端口"] = new[] { 80, 443, 22, 3389, 8080, 8443 },
        ["Web服务"] = new[] { 80, 443, 8080, 8443, 3000, 5000 },
        ["工控协议"] = new[] { 21, 22, 80, 443, 502, 44818, 2222, 9600, 20000, 4840, 34964 },
        ["全量扫描"] = new[] { 21, 22, 23, 25, 53, 80, 110, 135, 139, 143, 443, 445, 502, 993, 995, 1433, 1521, 3306, 3389, 5432, 5900, 6379, 8080, 8443, 9090, 27017, 44818 },
    };

    public MainForm()
    {
        InitializeComponent();
        cmbPortPreset.DataSource = new List<string>(_portProfiles.Keys);
        cmbPortPreset.SelectedIndex = 2; // 默认工控协议
    }

    // ═══════════════════════════════════════════
    //  SCAN
    // ═══════════════════════════════════════════
    private async void btnScan_Click(object sender, EventArgs e)
    {
        var (targets, dupCount) = ParseTargetsWithDupCheck(txtTarget.Text.Trim());
        if (targets.Length == 0)
        {
            ShowInfo("请输入目标IP、IP范围或CIDR子网。\n\n  192.168.1.1\n  192.168.1.0/24\n  192.168.1.1-192.168.1.254\n  逗号或空格分隔多个");
            return;
        }

        if (dupCount > 0)
            lblProgress.Text = $"⚠ 已去除 {dupCount} 个重复IP";

        var ports = chkScanPorts.Checked ? GetSelectedPorts() : Array.Empty<int>();
        var timeout = (int)numTimeout.Value;
        var resolveHost = chkHostname.Checked;
        var doProfiling = chkProfiling.Checked;
        var doMacVendor = chkMacVendor.Checked;

        btnScan.Enabled = false;
        btnStop.Visible = true;
        gridResults.Rows.Clear();
        progressBar.Value = 0;
        progressBar.Maximum = targets.Length;
        if (dupCount == 0) lblProgress.Text = $"0 / {targets.Length}";

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        var sw = Stopwatch.StartNew();

        try
        {
            int online = 0, offline = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                var ip = targets[i];

                // ══ Ping ══
                var (reachable, latency) = await PingAsync(ip, timeout);

                // ══ Port Scan ══
                var openPorts = new List<int>();
                if (reachable && ports.Length > 0)
                    openPorts = await ScanPortsAsync(ip, ports, timeout, token);

                if (reachable) online++; else offline++;

                var status = reachable ? "在线" : "离线";
                var latencyStr = reachable ? $"{latency}ms" : "-";
                var mac = reachable ? "-" : "-";
                var vendor = "-";
                var hostname = "-";
                var devType = "-";
                var portsStr = reachable && ports.Length > 0
                    ? (openPorts.Count > 0 ? string.Join(", ", openPorts) : "全部关闭")
                    : "-";

                gridResults.Rows.Add(ip, status, latencyStr, mac, vendor, hostname, devType, portsStr);

                var row = gridResults.Rows[gridResults.Rows.Count - 1];
                if (reachable)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 150, 80);
                    row.DefaultCellStyle.BackColor = Color.FromArgb(240, 255, 245);
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 180, 190);
                }

                progressBar.Value = i + 1;
                lblProgress.Text = $"{i + 1} / {targets.Length}";
            }

            sw.Stop();

            // ══ 后处理: ARP, 主机名, 设备推测 ══
            await PostProcessAsync(resolveHost, doProfiling, doMacVendor, token);

            // ══ ARP 冲突检测 ══
            var arpTable = ParseARPTable();
            var conflicts = new List<string>();
            foreach (DataGridViewRow row in gridResults.Rows)
            {
                if (row.IsNewRow) continue;
                var ip = row.Cells["colIP"].Value?.ToString() ?? "";
                if (arpTable.TryGetValue(ip, out var macs))
                {
                    if (row.Cells["colMAC"].Value?.ToString() == "-")
                        row.Cells["colMAC"].Value = string.Join(" | ", macs);

                    if (macs.Length > 1)
                    {
                        row.DefaultCellStyle.ForeColor = Color.OrangeRed;
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 235);
                        row.DefaultCellStyle.Font = new Font(gridResults.Font, FontStyle.Bold);
                        row.Cells["colStatus"].Value = "⚠ IP冲突";
                        conflicts.Add($"{ip} → {string.Join(", ", macs)}");
                    }
                }
            }

            lblSummary.Text = $"共 {targets.Length} 个  |  🟢 在线 {online}  |  ⚫ 离线 {offline}  |  ⏱ {sw.Elapsed.TotalSeconds:F1}s";
            if (conflicts.Count > 0)
            {
                lblSummary.Text += $"  |  ⚠ 冲突 {conflicts.Count} 个";
                lblSummary.ForeColor = Color.OrangeRed;
                var detail = string.Join("\n", conflicts.Take(5));
                if (conflicts.Count > 5) detail += $"\n... 共 {conflicts.Count} 个";
                MessageBox.Show($"⚠ 检测到 IP 地址冲突！\n\n{detail}\n\n同一IP对应多个MAC，局域网存在IP冲突。",
                    "⚠ IP冲突警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                lblSummary.ForeColor = Color.FromArgb(100, 100, 110);
            }
        }
        catch (OperationCanceledException)
        {
            lblProgress.Text = "已停止";
            lblSummary.Text = "扫描已停止";
        }
        finally
        {
            btnScan.Enabled = true;
            btnStop.Visible = false;
        }
    }

    private void btnStop_Click(object sender, EventArgs e) => _cts?.Cancel();

    private async Task PostProcessAsync(bool resolveHost, bool doProfiling, bool doMacVendor, CancellationToken token)
    {
        if (!resolveHost && !doProfiling && !doMacVendor) return;
        if (gridResults.Rows.Count == 0) return;

        lblProgress.Text = "后处理中...";

        // 收集在线IP
        var onlineIps = new List<(DataGridViewRow row, string ip, List<int> ports)>();
        foreach (DataGridViewRow row in gridResults.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["colStatus"].Value?.ToString() != "在线") continue;
            var ip = row.Cells["colIP"].Value?.ToString() ?? "";
            var portsStr = row.Cells["colPorts"].Value?.ToString() ?? "";
            var ports = portsStr == "-" || portsStr == "全部关闭" ? new List<int>()
                : portsStr.Split(',').Select(s => int.TryParse(s.Trim(), out var p) ? p : -1).Where(p => p > 0).ToList();
            onlineIps.Add((row, ip, ports));
        }

        // 并行处理
        var tasks = onlineIps.Select(async item =>
        {
            token.ThrowIfCancellationRequested();
            var (row, ip, ports) = item;

            if (doMacVendor)
            {
                var mac = row.Cells["colMAC"].Value?.ToString();
                if (!string.IsNullOrEmpty(mac) && mac != "-")
                {
                    var vendor = MacOuiDB.Lookup(mac);
                    if (vendor != null)
                        row.Cells["colVendor"].Value = vendor;
                }
            }

            if (resolveHost)
            {
                var host = await ResolveHostnameAsync(ip);
                if (host != null)
                    row.Cells["colHostname"].Value = host;
            }

            if (doProfiling && ports.Count > 0)
            {
                var dev = DeviceProfiler.Identify(ports.ToArray());
                if (dev != null)
                    row.Cells["colDevice"].Value = dev;
            }
        });

        await Task.WhenAll(tasks);
    }

    // ═══════════════════════════════════════════
    //  NETWORK OPS
    // ═══════════════════════════════════════════
    private async Task<(bool reachable, long latency)> PingAsync(string ip, int timeout)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, timeout);
            return (reply.Status == IPStatus.Success, reply.RoundtripTime);
        }
        catch { return (false, 0); }
    }

    private async Task<List<int>> ScanPortsAsync(string ip, int[] ports, int timeout, CancellationToken token)
    {
        var open = new List<int>();
        using var semaphore = new SemaphoreSlim(30);
        var tasks = new List<Task>();

        foreach (var port in ports)
        {
            token.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(token);
            var t = Task.Run(async () =>
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(ip, port).WaitAsync(TimeSpan.FromMilliseconds(timeout), token);
                    lock (open) open.Add(port);
                }
                catch { }
                finally { semaphore.Release(); }
            }, token);
            tasks.Add(t);
        }

        await Task.WhenAll(tasks);
        open.Sort();
        return open;
    }

    private async Task<string?> ResolveHostnameAsync(string ip)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(ip);
            if (!string.IsNullOrEmpty(entry.HostName) && entry.HostName != ip)
                return entry.HostName;
        }
        catch { }
        return null;
    }

    // ═══════════════════════════════════════════
    //  ARP TABLE
    // ═══════════════════════════════════════════
    private Dictionary<string, string[]> ParseARPTable()
    {
        var result = new Dictionary<string, string[]>();
        try
        {
            var output = RunCommand("arp", "-a");
            var macsByIp = new Dictionary<string, List<string>>();

            foreach (var line in output.Split('\n'))
            {
                var m = Regex.Match(line.Trim(),
                    @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\s+([0-9a-fA-F]{2}[-:][0-9a-fA-F]{2}[-:][0-9a-fA-F]{2}[-:][0-9a-fA-F]{2}[-:][0-9a-fA-F]{2}[-:][0-9a-fA-F]{2})");
                if (!m.Success) continue;

                var ip = m.Groups[1].Value;
                var mac = m.Groups[2].Value.ToUpper().Replace(':', '-');

                if (!macsByIp.ContainsKey(ip)) macsByIp[ip] = new List<string>();
                if (!macsByIp[ip].Contains(mac)) macsByIp[ip].Add(mac);
            }

            foreach (var kv in macsByIp) result[kv.Key] = kv.Value.ToArray();
        }
        catch { }
        return result;
    }

    private string RunCommand(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            return p?.StandardOutput.ReadToEnd() ?? "";
        }
        catch { return ""; }
    }

    // ═══════════════════════════════════════════
    //  LAN DETECTION
    // ═══════════════════════════════════════════
    private void btnDetectLAN_Click(object? sender, EventArgs e)
    {
        var subnets = DetectLocalSubnets();
        if (subnets.Length == 0)
        {
            ShowInfo("未检测到活动网络连接。请确认网卡已启用。");
            return;
        }
        if (subnets.Length == 1)
        {
            txtTarget.Text = subnets[0];
            return;
        }
        var choice = MessageBox.Show(
            $"检测到 {subnets.Length} 个活动子网:\n\n{string.Join("\n", subnets.Select((s, i) => $"  [{i + 1}] {s}"))}\n\n点「是」使用第1个",
            "选择子网", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (choice == DialogResult.Yes) txtTarget.Text = subnets[0];
    }

    private string[] DetectLocalSubnets()
    {
        var subnets = new List<string>();
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;

            foreach (var ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                var mask = ip.IPv4Mask?.GetAddressBytes();
                if (mask == null) continue;

                var addr = ip.Address.GetAddressBytes();
                int prefix = 0;
                foreach (var b in mask)
                    for (int j = 7; j >= 0; j--)
                        if ((b & (1 << j)) != 0) prefix++; else goto done;
                done:

                var network = new byte[4];
                for (int i = 0; i < 4; i++) network[i] = (byte)(addr[i] & mask[i]);
                var cidr = $"{network[0]}.{network[1]}.{network[2]}.{network[3]}/{prefix}";
                if (!subnets.Contains(cidr)) subnets.Add(cidr);
            }
        }
        return subnets.ToArray();
    }

    // ═══════════════════════════════════════════
    //  TARGET PARSING
    // ═══════════════════════════════════════════
    private (string[] targets, int dupCount) ParseTargetsWithDupCheck(string input)
    {
        var raw = input.Split(new[] { ',', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

        var expanded = new List<string>();
        foreach (var item in raw)
        {
            var cidr = Regex.Match(item, @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/(\d{1,2})$");
            if (cidr.Success) { expanded.AddRange(ExpandCIDR(cidr.Groups[1].Value, int.Parse(cidr.Groups[2].Value))); continue; }

            var range = Regex.Match(item, @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})-(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})$");
            if (range.Success) { expanded.AddRange(ExpandRange(range.Groups[1].Value, range.Groups[2].Value)); continue; }

            expanded.Add(item);
        }
        var dupCount = expanded.Count - expanded.Distinct().Count();
        return (expanded.Distinct().ToArray(), dupCount);
    }

    private List<string> ExpandCIDR(string network, int prefix)
    {
        var hosts = new List<string>();
        var baseNum = IpToNum(network);
        var count = 1L << (32 - prefix);
        if (count > 65536) { ShowWarn($"子网包含 {count} 个地址，数量过大。"); return hosts; }
        var start = prefix < 31 ? baseNum + 1 : baseNum;
        var end = prefix < 31 ? baseNum + count - 2 : baseNum + count - 1;
        for (var i = start; i <= end; i++) hosts.Add(NumToIp((uint)i));
        return hosts;
    }

    private List<string> ExpandRange(string start, string end)
    {
        var hosts = new List<string>();
        var s = IpToNum(start);
        var e = IpToNum(end);
        if (s > e || e - s > 65536) { if (e - s > 65536) ShowWarn("范围过大。"); return hosts; }
        for (var i = s; i <= e; i++) hosts.Add(NumToIp((uint)(i & 0xFFFFFFFF)));
        return hosts;
    }

    private static uint IpToNum(string ip) { var p = ip.Split('.'); return ((uint)byte.Parse(p[0]) << 24) | ((uint)byte.Parse(p[1]) << 16) | ((uint)byte.Parse(p[2]) << 8) | uint.Parse(p[3]); }
    private static string NumToIp(uint num) => $"{(num >> 24) & 255}.{(num >> 16) & 255}.{(num >> 8) & 255}.{num & 255}";

    // ═══════════════════════════════════════════
    //  PORT SELECTION
    // ═══════════════════════════════════════════
    private int[] GetSelectedPorts()
    {
        var custom = txtCustomPorts.Text.Trim();
        if (!string.IsNullOrEmpty(custom))
            return custom.Split(',').Select(s => int.TryParse(s.Trim(), out var p) ? p : -1)
                .Where(p => p > 0 && p < 65536).Distinct().OrderBy(p => p).ToArray();
        var profile = cmbPortPreset.SelectedItem?.ToString() ?? "工控协议";
        return _portProfiles.GetValueOrDefault(profile, _portProfiles["工控协议"]);
    }

    private void chkScanPorts_CheckedChanged(object? sender, EventArgs e)
    {
        var en = chkScanPorts.Checked;
        cmbPortPreset.Enabled = en;
        txtCustomPorts.Enabled = en;
        lblPortPreset.Enabled = en;
        lblCustomPorts.Enabled = en;
    }

    // ═══════════════════════════════════════════
    //  SNAPSHOT / COMPARE
    // ═══════════════════════════════════════════
    private record ScanEntry(string IP, string Status, string Latency, string MAC, string Vendor, string Hostname, string Device, string Ports);
    private record ScanSnapshot(string Created, List<ScanEntry> Entries);

    private void btnSnapshot_Click(object? sender, EventArgs e)
    {
        if (gridResults.Rows.Count == 0)
        {
            ShowInfo("没有扫描结果可保存。请先执行扫描。");
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "快照文件|*.scan.json",
            DefaultExt = "scan.json",
            FileName = $"ip-snapshot-{DateTime.Now:yyyyMMdd-HHmmss}"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var entries = new List<ScanEntry>();
        foreach (DataGridViewRow row in gridResults.Rows)
        {
            if (row.IsNewRow) continue;
            entries.Add(new ScanEntry(
                row.Cells["colIP"].Value?.ToString() ?? "",
                row.Cells["colStatus"].Value?.ToString() ?? "",
                row.Cells["colLatency"].Value?.ToString() ?? "",
                row.Cells["colMAC"].Value?.ToString() ?? "",
                row.Cells["colVendor"].Value?.ToString() ?? "",
                row.Cells["colHostname"].Value?.ToString() ?? "",
                row.Cells["colDevice"].Value?.ToString() ?? "",
                row.Cells["colPorts"].Value?.ToString() ?? ""
            ));
        }

        var snapshot = new ScanSnapshot(DateTime.Now.ToString("O"), entries);
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(sfd.FileName, json, Encoding.UTF8);
        _lastSnapshotPath = sfd.FileName;
        ShowInfo($"快照已保存: {Path.GetFileName(sfd.FileName)}\n包含 {entries.Count} 条记录。");
    }

    private void btnCompare_Click(object? sender, EventArgs e)
    {
        if (gridResults.Rows.Count == 0)
        {
            ShowInfo("请先执行扫描，再加载历史快照进行对比。");
            return;
        }

        using var ofd = new OpenFileDialog
        {
            Filter = "快照文件|*.scan.json",
            Title = "选择历史快照进行对比"
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var json = File.ReadAllText(ofd.FileName, Encoding.UTF8);
            var snapshot = JsonSerializer.Deserialize<ScanSnapshot>(json);
            if (snapshot?.Entries == null || snapshot.Entries.Count == 0)
            {
                ShowWarn("快照文件为空或格式无效。");
                return;
            }

            var oldDict = snapshot.Entries.ToDictionary(e => e.IP);
            int added = 0, removed = 0, changed = 0, same = 0;
            var changes = new List<string>();

            foreach (DataGridViewRow row in gridResults.Rows)
            {
                if (row.IsNewRow) continue;
                var ip = row.Cells["colIP"].Value?.ToString() ?? "";
                var status = row.Cells["colStatus"].Value?.ToString() ?? "";
                var ports = row.Cells["colPorts"].Value?.ToString() ?? "";

                if (!oldDict.TryGetValue(ip, out var old))
                {
                    if (status == "在线") { added++; row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); changes.Add($"➕ 新增: {ip}"); }
                }
                else
                {
                    if (old.Status == "在线" && status == "离线") { removed++; row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230); changes.Add($"➖ 离线: {ip}"); }
                    else if (old.Status == "离线" && status == "在线") { added++; row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); changes.Add($"➕ 上线: {ip}"); }
                    else if (old.Ports != ports) { changed++; row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220); changes.Add($"🔀 端口变更: {ip} ({old.Ports} → {ports})"); }
                    else { same++; }
                }
            }

            // 检查快照中有但当前没有的
            var currentIps = new HashSet<string>();
            foreach (DataGridViewRow row in gridResults.Rows)
                if (!row.IsNewRow) currentIps.Add(row.Cells["colIP"].Value?.ToString() ?? "");
            foreach (var kv in oldDict)
                if (!currentIps.Contains(kv.Key) && kv.Value.Status == "在线")
                { removed++; changes.Add($"➖ 消失: {kv.Key} (上次在线)"); }

            var msg = $"📸 快照对比结果\n" +
                      $"快照时间: {snapshot.Created}\n\n" +
                      $"➕ 新增/上线: {added}\n" +
                      $"➖ 离线/消失: {removed}\n" +
                      $"🔀 端口变更: {changed}\n" +
                      $"✅ 无变化: {same}\n\n" +
                      $"{(changes.Count > 0 ? "变更详情:\n" + string.Join("\n", changes.Take(15)) : "无变更")}" +
                      $"{(changes.Count > 15 ? $"\n... 共 {changes.Count} 项变更" : "")}";

            MessageBox.Show(msg, "📸 快照对比", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowWarn($"快照加载失败: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    //  EXPORT
    // ═══════════════════════════════════════════
    private void btnExport_Click(object? sender, EventArgs e)
    {
        if (gridResults.Rows.Count == 0) { ShowInfo("没有可导出的数据。"); return; }
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV文件|*.csv|TXT文件|*.txt",
            DefaultExt = "csv",
            FileName = $"ip-scan-{DateTime.Now:yyyyMMdd-HHmmss}"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("IP地址,状态,延迟,MAC地址,厂商,主机名,设备类型,开放端口");
        foreach (DataGridViewRow row in gridResults.Rows)
        {
            if (row.IsNewRow) continue;
            var vals = new string[8];
            for (int i = 0; i < 8; i++) vals[i] = row.Cells[i].Value?.ToString() ?? "";
            sb.AppendLine(string.Join(",", vals.Select(v => v.Contains(',') ? $"\"{v}\"" : v)));
        }
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        ShowInfo($"已导出: {sfd.FileName}");
    }

    // ═══════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════
    private void ShowInfo(string msg) => MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    private void ShowWarn(string msg) => MessageBox.Show(msg, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
