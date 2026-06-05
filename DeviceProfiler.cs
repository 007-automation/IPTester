namespace IPTester;

/// <summary>
/// 根据开放端口推测设备类型
/// </summary>
public static class DeviceProfiler
{
    public record Profile(string Type, string Icon, int Priority);

    private static readonly List<(int[] ports, Profile profile)> _rules = new()
    {
        // 工控协议设备（高优先级）
        (new[] { 102 },            new Profile("S7comm PLC", "🔌", 100)),
        (new[] { 502 },            new Profile("Modbus TCP 设备", "🏭", 90)),
        (new[] { 502, 80 },        new Profile("Modbus Web 网关", "🌐", 95)),
        (new[] { 44818 },          new Profile("EtherNet/IP 设备", "🔧", 90)),
        (new[] { 44818, 2222 },    new Profile("Omron PLC (FINS)", "🇯🇵", 95)),
        (new[] { 4840 },           new Profile("OPC UA 服务器", "📡", 90)),
        (new[] { 4840, 443 },      new Profile("OPC UA (安全)", "🔒", 95)),
        (new[] { 34964 },          new Profile("Profinet 设备", "🔌", 90)),
        (new[] { 20000 },          new Profile("DNP3 设备", "⚡", 85)),
        (new[] { 9600 },           new Profile("FINS UDP 设备", "🇯🇵", 85)),

        // 自动化设备
        (new[] { 22, 502 },        new Profile("Linux Modbus 网关", "🐧", 80)),
        (new[] { 80, 502 },        new Profile("Modbus Web 接口", "🌐", 80)),
        (new[] { 80, 44818 },      new Profile("EtherNet/IP Web 接口", "🌐", 80)),
        (new[] { 80, 4840 },       new Profile("OPC UA Web 接口", "🌐", 80)),

        // IT/网络设备
        (new[] { 80, 443, 22 },    new Profile("Web 服务器 (Linux)", "🐧", 60)),
        (new[] { 80, 443 },        new Profile("Web 服务器", "🌍", 50)),
        (new[] { 22 },             new Profile("SSH 服务器", "💻", 40)),
        (new[] { 3389 },           new Profile("Windows 远程桌面", "🪟", 40)),
        (new[] { 3306 },           new Profile("MySQL 数据库", "🗄️", 40)),
        (new[] { 5432 },           new Profile("PostgreSQL 数据库", "🗄️", 40)),
        (new[] { 1433 },           new Profile("SQL Server 数据库", "🗄️", 40)),
        (new[] { 6379 },           new Profile("Redis 缓存", "⚡", 40)),
        (new[] { 27017 },          new Profile("MongoDB 数据库", "🍃", 40)),
        (new[] { 8080 },           new Profile("HTTP 代理/Web", "🌐", 35)),
        (new[] { 8443 },           new Profile("HTTPS 服务", "🔒", 35)),
        (new[] { 21 },             new Profile("FTP 服务器", "📁", 30)),
        (new[] { 25, 587 },        new Profile("邮件服务器(SMTP)", "📧", 35)),
        (new[] { 53 },             new Profile("DNS 服务器", "📋", 30)),
        (new[] { 161 },            new Profile("SNMP 设备", "📊", 30)),

        // HMI/面板
        (new[] { 5900 },           new Profile("VNC 远程/HMI", "🖥️", 45)),
        (new[] { 80, 5900 },       new Profile("Web HMI 面板", "🖥️", 50)),
    };

    /// <summary>根据开放端口列表推测设备类型</summary>
    public static string? Identify(int[] openPorts)
    {
        if (openPorts.Length == 0) return null;

        Profile? best = null;
        foreach (var (rulePorts, profile) in _rules)
        {
            // 所有规则端口都在开放列表中才算匹配
            if (rulePorts.All(rp => openPorts.Contains(rp)))
            {
                if (best == null || profile.Priority > best.Priority)
                    best = profile;
            }
        }
        return best != null ? $"{best.Icon} {best.Type}" : null;
    }
}
