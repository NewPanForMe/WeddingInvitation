using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers;

/// <summary>出席回复（无需登录，凭宾客 token）。</summary>
[ApiController]
[Route("api/rsvp")]
public class RsvpController : ControllerBase
{
    private readonly RsvpService _rsvpService;

    public RsvpController(RsvpService rsvpService) => _rsvpService = rsvpService;

    [HttpPost]
    public async Task<ActionResult<RsvpResultDto>> SubmitAsync(
        [FromBody] RsvpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _rsvpService.SubmitAsync(request, cancellationToken);
        return Ok(result);
    }
}
