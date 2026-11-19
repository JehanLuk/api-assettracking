using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.Auditing.Controllers;

[ApiController]
[Authorize(Roles = "Administrator,SesmtManager")]
[Route("api")]
public class AuditController(AssetContext db) : ControllerBase
{
    [HttpGet("audit")]
    public async Task<IActionResult> Audit() =>
        Ok(await db.AuditLogs.AsNoTracking().OrderByDescending(x => x.Timestamp).Take(500).ToListAsync());

    [HttpGet("reports/deliveries")]
    public async Task<IActionResult> DeliveryReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = db.Deliveries.AsNoTracking().Include(x => x.Employee).Include(x => x.Items).ThenInclude(x => x.EpiItem).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.DeliveryDate >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        if (to.HasValue) query = query.Where(x => x.DeliveryDate <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));
        return Ok(await query.OrderByDescending(x => x.DeliveryDate).ToListAsync());
    }

    [HttpGet("dashboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.UtcNow;
        return Ok(new
        {
            employees = await db.Employees.CountAsync(),
            epiItems = await db.EpiItems.CountAsync(),
            available = await db.EpiItems.CountAsync(x => x.Status == EpiStatus.Available),
            delivered = await db.EpiItems.CountAsync(x => x.Status == EpiStatus.Delivered),
            maintenance = await db.EpiItems.CountAsync(x => x.Status == EpiStatus.Maintenance),
            expiredCertificates = await db.ApprovalCertificates.CountAsync(x => x.ExpirationDate <= now),
            openAlerts = await db.ComplianceAlerts.CountAsync(x => !x.Resolved)
        });
    }
}
