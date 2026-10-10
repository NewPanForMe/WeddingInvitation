using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Options;
using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Application.Services;

/// <summary>照片管理：文件写入服务器本地目录，数据库只保存访问路径。</summary>
public class PhotoService
{
    private static readonly string[] DefaultAllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly IApplicationDbContext _db;
    private readonly UploadOptions _options;

    public PhotoService(IApplicationDbContext db, IOptions<UploadOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<PhotoDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var photos = await _db.Photos
            .AsNoTracking()
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return photos.Select(Mapping.ToPhotoDto).ToList();
    }

    public async Task<PhotoDto> UploadAsync(
        Stream stream,
        string fileName,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowed = _options.AllowedExtensions is { Length: > 0 }
            ? _options.AllowedExtensions
            : DefaultAllowedExtensions;
        if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ApiException($"只允许上传 {string.Join("/", allowed)} 格式的图片", 400);
        }

        var maxBytes = Math.Max(1, _options.MaxSizeMB) * 1024L * 1024L;
        if (contentLength > maxBytes)
        {
            throw new ApiException($"图片大小不能超过 {_options.MaxSizeMB}MB", 400);
        }

        var root = GetUploadRoot();
        var dateSegment = DateTimeOffset.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var dateRoot = Path.Combine(root, dateSegment);
        Directory.CreateDirectory(dateRoot);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(dateRoot, storedName);
        await using (var target = File.Create(fullPath))
        {
            await stream.CopyToAsync(target, cancellationToken);
        }

        var sortOrder = await _db.Photos.AnyAsync(cancellationToken)
            ? await _db.Photos.MaxAsync(p => p.SortOrder, cancellationToken) + 1
            : 1;

        var photo = new Photo
        {
            Url = $"{_options.RequestPath.TrimEnd('/')}/{dateSegment}/{storedName}",
            Title = Path.GetFileNameWithoutExtension(fileName),
            SortOrder = sortOrder,
            Enabled = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync(cancellationToken);
        return Mapping.ToPhotoDto(photo);
    }

    public async Task<PhotoDto> UpdateAsync(long id, PhotoUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("照片不存在");

        photo.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
        photo.SortOrder = request.SortOrder;
        photo.Enabled = request.Enabled;

        await _db.SaveChangesAsync(cancellationToken);
        return Mapping.ToPhotoDto(photo);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("照片不存在");

        _db.Photos.Remove(photo);
        await _db.SaveChangesAsync(cancellationToken);
        TryDeleteFile(photo.Url);
    }

    public async Task SortAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken = default)
    {
        if (ids is null || ids.Count == 0)
        {
            return;
        }

        var photos = await _db.Photos.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
        for (var i = 0; i < ids.Count; i++)
        {
            var photo = photos.FirstOrDefault(p => p.Id == ids[i]);
            if (photo is not null)
            {
                photo.SortOrder = i + 1;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private void TryDeleteFile(string url)
    {
        try
        {
            var requestPath = _options.RequestPath.TrimEnd('/');
            if (!url.StartsWith(requestPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relative = url[requestPath.Length..].TrimStart('/');
            var root = GetUploadRoot();
            var fullPath = Path.GetFullPath(Path.Combine(root, relative));
            if (IsInsideDirectory(root, fullPath) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException)
        {
            // 文件删除失败不影响数据一致性，交由运维清理。
        }
    }

    private string GetUploadRoot()
    {
        return Path.GetFullPath(string.IsNullOrWhiteSpace(_options.RootPath) ? "upload" : _options.RootPath);
    }

    private static bool IsInsideDirectory(string root, string path)
    {
        var normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }
}
