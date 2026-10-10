namespace WeddingInvitation.Application.Options;

/// <summary>数据库连接串来源配置，对应 appsettings 的 PostgreSql 节。</summary>
public class PostgreSqlOptions
{
    public const string SectionName = "PostgreSql";

    /// <summary>取串接口地址（含 connName 参数）。</summary>
    public string? ConnectionStringEndpoint { get; set; }

    /// <summary>连接名，接口地址未带 connName 时用于拼接。</summary>
    public string? ConnectionName { get; set; }

    /// <summary>是否优先使用远程接口取串并解密。</summary>
    public bool UseRemote { get; set; } = true;

    /// <summary>本地兜底连接串（Base64 两层的 Npgsql 连接串），仅离线开发/应急使用。</summary>
    public string? ConnectionString { get; set; }
}
