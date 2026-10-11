using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers.Admin;

/// <summary>后台宾客管理。</summary>
[ApiController]
[Authorize]
[Route("api/admin/guests")]
public class GuestsController : ControllerBase
{
    private readonly GuestService _guestService;

    public GuestsController(GuestService guestService) => _guestService = guestService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<GuestDto>>> ListAsync(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _guestService.ListAsync(keyword, status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GuestDto>> CreateAsync(
        [FromBody] GuestUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var guest = await _guestService.CreateAsync(request, cancellationToken);
        return Ok(guest);
    }

    [HttpPost("{id:long}")]
    public async Task<ActionResult<GuestDto>> UpdateAsync(
        long id,
        [FromBody] GuestUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var guest = await _guestService.UpdateAsync(id, request, cancellationToken);
        return Ok(guest);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<OperationResult>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await _guestService.DeleteAsync(id, cancellationToken);
        return Ok(new OperationResult(true, "删除成功"));
    }

    /// <summary>CSV 批量导入：姓名,称呼,电话,关系,桌号,可带人数（UTF-8，表头可省略）。</summary>
    [HttpPost("import")]
    public async Task<ActionResult<ImportResultDto>> ImportAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ApiException("请选择要导入的 CSV 文件", 400);
        }

        await using var stream = file.OpenReadStream();
        var result = await _guestService.ImportAsync(stream, cancellationToken);
        return Ok(result);
    }

    /// <summary>宾客专属请柬链接（供二维码使用）。</summary>
    [HttpGet("{id:long}/link")]
    public async Task<ActionResult<GuestLinkDto>> GetLinkAsync(long id, CancellationToken cancellationToken)
    {
        var link = await _guestService.GetLinkAsync(id, cancellationToken);
        return Ok(link);
    }
}
