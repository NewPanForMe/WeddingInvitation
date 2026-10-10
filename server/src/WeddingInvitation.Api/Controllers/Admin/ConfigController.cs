using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers.Admin;

/// <summary>后台请柬配置（地址只维护详细地址，不使用经纬度）。</summary>
[ApiController]
[Authorize]
[Route("api/admin/config")]
public class ConfigController : ControllerBase
{
    private readonly ConfigService _configService;

    public ConfigController(ConfigService configService) => _configService = configService;

    [HttpGet]
    public async Task<ActionResult<InvitationConfigDto>> GetAsync(CancellationToken cancellationToken)
    {
        var config = await _configService.GetAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut]
    public async Task<ActionResult<InvitationConfigDto>> UpdateAsync(
        [FromBody] InvitationConfigUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _configService.UpdateAsync(request, cancellationToken);
        return Ok(config);
    }
}
