namespace WeddingInvitation.Application.Options;

/// <summary>请柬分享地址配置，对应 appsettings 的 Invite 节。</summary>
public class InviteOptions
{
    public const string SectionName = "Invite";

    /// <summary>请柬站点地址，用于生成宾客专属链接与二维码内容，例如 https://www.tunn.work/Wedding/i。</summary>
    public string BaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>链接路径模板，{token} 会被替换。</summary>
    public string PathTemplate { get; set; } = "/i/{token}";

    public string BuildUrl(string token)
    {
        var baseUrl = (BaseUrl ?? string.Empty).TrimEnd('/');
        var path = string.IsNullOrWhiteSpace(PathTemplate) ? "/i/{token}" : PathTemplate;
        return $"{baseUrl}{path.Replace("{token}", Uri.EscapeDataString(token), StringComparison.OrdinalIgnoreCase)}";
    }
}
