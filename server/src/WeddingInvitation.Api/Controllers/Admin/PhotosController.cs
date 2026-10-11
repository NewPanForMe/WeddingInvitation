using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers.Admin;

/// <summary>后台照片管理（服务器本地目录存储）。</summary>
[ApiController]
[Authorize]
[Route("api/admin/photos")]
public class PhotosController : ControllerBase
{
    private readonly PhotoService _photoService;

    public PhotosController(PhotoService photoService) => _photoService = photoService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PhotoDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var photos = await _photoService.ListAsync(cancellationToken);
        return Ok(photos);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<PhotoDto>> UploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ApiException("请选择要上传的图片", 400);
        }

        await using var stream = file.OpenReadStream();
        var photo = await _photoService.UploadAsync(stream, file.FileName, file.Length, cancellationToken);
        return Ok(photo);
    }

    [HttpPost("{id:long}")]
    public async Task<ActionResult<PhotoDto>> UpdateAsync(
        long id,
        [FromBody] PhotoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var photo = await _photoService.UpdateAsync(id, request, cancellationToken);
        return Ok(photo);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<OperationResult>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await _photoService.DeleteAsync(id, cancellationToken);
        return Ok(new OperationResult(true, "删除成功"));
    }

    /// <summary>按传入的 id 顺序重排照片。</summary>
    [HttpPost("sort")]
    public async Task<ActionResult<OperationResult>> SortAsync(
        [FromBody] PhotoSortRequest request,
        CancellationToken cancellationToken)
    {
        await _photoService.SortAsync(request?.Ids ?? [], cancellationToken);
        return Ok(new OperationResult(true, "排序已保存"));
    }
}
