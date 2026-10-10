namespace WeddingInvitation.Application.Options;

/// <summary>后台接口 JWT 配置，对应 appsettings 的 Security:Jwt 节。</summary>
public class JwtOptions
{
    public const string SectionName = "Security:Jwt";

    /// <summary>对称密钥，长度至少 32 字节。</summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "WeddingInvitation";

    public string Audience { get; set; } = "WeddingInvitation.Admin";

    /// <summary>令牌有效期（分钟），默认 120。</summary>
    public int ExpiresMinutes { get; set; } = 120;
}
