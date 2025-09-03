using Microsoft.AspNetCore.Mvc;
using AssetTrackingAPI.Context;

namespace AssetTrackingAPI.Controller;

[ApiController]
[Route("[controller]")]
public class HealthController(AssetContext _context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        bool dbIsOk;
        try {
            dbIsOk = await _context.Database.CanConnectAsync();
        } catch {
            dbIsOk = false;
        }

        var status = new
        {
            api = "Online",
            database = dbIsOk ? "Connected" : "Unavailable",
            timestamp = DateTime.UtcNow
        };

        return Ok(status);
    }
}