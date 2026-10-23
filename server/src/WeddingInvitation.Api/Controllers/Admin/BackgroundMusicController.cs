using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers.Admin;

/// <summary>后台请柬背景音乐上传。</summary>
[ApiController]
[Authorize]
[Route("api/admin/background-music")]
public class BackgroundMusicController : ControllerBase
{
    private readonly BackgroundMusicService _backgroundMusicService;

    public BackgroundMusicController(BackgroundMusicService backgroundMusicService) =>
        _backgroundMusicService = backgroundMusicService;

    [HttpPost("upload")]
    [RequestSizeLimit(20L * 1024 * 1024)]
    public async Task<ActionResult<BackgroundMusicUploadDto>> UploadAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ApiException("请选择要上传的背景音乐", 400);
        }

        await using var stream = file.OpenReadStream();
        var uploaded = await _backgroundMusicService.UploadAsync(
            stream,
            file.FileName,
            file.Length,
            cancellationToken);
        return Ok(uploaded);
    }
}
