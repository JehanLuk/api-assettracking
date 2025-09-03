using Microsoft.AspNetCore.Mvc;
using AssetTrackingAPI.Context;

[ApiController]
[Route("[controller]")]
public class HealthController(AssetContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        bool dbIsOk;
        try {
            dbIsOk = await context.DataBase.CanConnectAsync();
        } catch {
            dbIsOk = false;
        }

        var status = new
        {
            api = "Online",
            database = dbIsOk ? "Connected" : "Unavaible",
            timestamp = DateTime.UtcNow
        }

        return Ok(status);
    }
}