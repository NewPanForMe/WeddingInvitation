namespace WeddingInvitation.Application.Common;

/// <summary>背景音乐上传规则。</summary>
public static class BackgroundMusicUploadPolicy
{
    private static readonly string[] AllowedExtensions = [".mp3", ".m4a"];

    public static bool IsAllowedFileName(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    public static bool ExceedsMaximumSize(long contentLength, int maxSizeMb) =>
        contentLength > Math.Max(1, maxSizeMb) * 1024L * 1024L;

    public static string AllowedFormatsText => "mp3 / m4a";
}
