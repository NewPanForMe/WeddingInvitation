using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers;

/// <summary>请柬公开接口（无需登录）。</summary>
[ApiController]
[Route("api/invite")]
public class InviteController : ControllerBase
{
    private readonly InviteService _inviteService;

    public InviteController(InviteService inviteService) => _inviteService = inviteService;

    /// <summary>按宾客专属 token 获取请柬数据。</summary>
    [HttpGet("{token}")]
    public async Task<ActionResult<InviteResponse>> GetAsync(string token, CancellationToken cancellationToken)
    {
        var response = await _inviteService.GetAsync(token, cancellationToken);
        return Ok(response);
    }
}
