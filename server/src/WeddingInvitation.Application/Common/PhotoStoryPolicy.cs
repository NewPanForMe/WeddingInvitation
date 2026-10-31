using System.Text.RegularExpressions;
using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Application.Common;

/// <summary>照片章节标题与描述的公开展示规则。</summary>
public static partial class PhotoStoryPolicy
{
    private const int DescriptionMaxLength = 500;

    public static string? NormalizeDescription(string? value) =>
        InvitationAtmospherePolicy.NormalizeText(value, DescriptionMaxLength);

    public static string? ToPublicTitle(string? value)
    {
        var title = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return title is null || GeneratedFileNameRegex().IsMatch(title) ? null : title;
    }

    public static Photo? ResolveCoverPhoto(IReadOnlyList<Photo> photos, string? coverUrl)
    {
        if (string.IsNullOrWhiteSpace(coverUrl)) return null;
        return photos.FirstOrDefault(photo =>
            string.Equals(photo.Url, coverUrl, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(
        @"^(?:IMG[-_]?\d{6,}(?:[-_]\d+)*|DSC[-_]?\d+|微信图片[-_]?\d+|\d{8}[-_]?\d{6}(?:[-_]\d+)?)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GeneratedFileNameRegex();
}
