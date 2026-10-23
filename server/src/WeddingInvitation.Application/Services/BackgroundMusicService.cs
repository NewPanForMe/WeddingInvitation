using System.Globalization;
using Microsoft.Extensions.Options;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Options;

namespace WeddingInvitation.Application.Services;

/// <summary>请柬背景音乐的本地文件存储。</summary>
public class BackgroundMusicService
{
    private readonly UploadOptions _uploadOptions;
    private readonly BackgroundMusicUploadOptions _musicOptions;

    public BackgroundMusicService(
        IOptions<UploadOptions> uploadOptions,
        IOptions<BackgroundMusicUploadOptions> musicOptions)
    {
        _uploadOptions = uploadOptions.Value;
        _musicOptions = musicOptions.Value;
    }

    public async Task<BackgroundMusicUploadDto> UploadAsync(
        Stream stream,
        string fileName,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (!BackgroundMusicUploadPolicy.IsAllowedFileName(fileName))
        {
            throw new ApiException($"只允许上传 {BackgroundMusicUploadPolicy.AllowedFormatsText} 格式的背景音乐", 400);
        }

        if (BackgroundMusicUploadPolicy.ExceedsMaximumSize(contentLength, _musicOptions.MaxSizeMB))
        {
            throw new ApiException($"背景音乐大小不能超过 {_musicOptions.MaxSizeMB}MB", 400);
        }

        var dateSegment = DateTimeOffset.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var root = Path.GetFullPath(_uploadOptions.RootPath);
        var relativeDirectory = Path.Combine("music", dateSegment);
        var directory = Path.Combine(root, relativeDirectory);
        Directory.CreateDirectory(directory);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, storedName);
        await using (var target = File.Create(fullPath))
        {
            await stream.CopyToAsync(target, cancellationToken);
        }

        var requestPath = _uploadOptions.RequestPath.TrimEnd('/');
        return new BackgroundMusicUploadDto($"{requestPath}/music/{dateSegment}/{storedName}");
    }
}
