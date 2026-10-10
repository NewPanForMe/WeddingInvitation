using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Services;

namespace WeddingInvitation.Api.Controllers.Admin;

/// <summary>后台统计。</summary>
[ApiController]
[Authorize]
[Route("api/admin/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService _statisticsService;

    public StatisticsController(StatisticsService statisticsService) => _statisticsService = statisticsService;

    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetAsync(CancellationToken cancellationToken)
    {
        var statistics = await _statisticsService.GetAsync(cancellationToken);
        return Ok(statistics);
    }
}
