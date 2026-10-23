namespace WeddingInvitation.Application.Common;

/// <summary>请柬浪漫文案与背景音乐的展示规则。</summary>
public static class InvitationAtmospherePolicy
{
    public static string? NormalizeText(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return normalized is { Length: > 0 } ? normalized[..Math.Min(normalized.Length, maxLength)] : null;
    }

    public static bool CanShowBackgroundMusic(bool enabled, string? musicUrl) =>
        enabled && !string.IsNullOrWhiteSpace(musicUrl);
}
