namespace WeddingInvitation.Application.Options;

/// <summary>启动时自动补种的管理员账号，对应 appsettings 的 AdminSeed 节。</summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    /// <summary>是否在管理员表为空时自动创建默认管理员。</summary>
    public bool Enabled { get; set; } = true;

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "admin123";
}
