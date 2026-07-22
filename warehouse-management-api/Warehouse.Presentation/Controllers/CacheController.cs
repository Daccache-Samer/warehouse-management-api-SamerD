using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Trackers;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheController(CacheStatisticsTracker cacheTracker) : ControllerBase
{
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult GetStatistics()
    {
        var stats = new
        {
            cacheTracker.HitCount,
            cacheTracker.MissCount,
            cacheTracker.LastRefreshTime,
            cacheTracker.CachedKeys,
            TotalCachedKeys = cacheTracker.CachedKeys.Count
        };

        return Ok(stats);
    }
}