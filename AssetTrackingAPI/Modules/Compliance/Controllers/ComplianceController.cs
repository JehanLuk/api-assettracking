using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.Compliance.Controllers;

[ApiController]
[Authorize]
[Route("api/compliance")]
public class ComplianceController(AssetContext db) : ControllerBase
{
    [HttpPost("inspections")]
    [Authorize(Roles = "Administrator,SesmtManager")]
    public async Task<IActionResult> Inspect(InspectionRequest request)
    {
        var item = await db.EpiItems.FindAsync(request.EpiItemId);
        if (item is null) return NotFound();
        var inspection = new Inspection
        {
            EpiItemId = item.Id,
            Condition = request.Condition,
            Notes = request.Notes?.Trim(),
            InspectionDate = DateTime.UtcNow
        };
        item.Status = request.Condition switch
        {
            EpiCondition.Good => item.EmployeeId is null ? EpiStatus.Available : EpiStatus.Delivered,
            EpiCondition.Damaged or EpiCondition.Rejected => EpiStatus.Maintenance,
            EpiCondition.Expired => EpiStatus.Rejected,
            _ => item.Status
        };
        db.Inspections.Add(inspection);
        await db.SaveChangesAsync();
        if (request.Condition != EpiCondition.Good)
        {
            db.ComplianceAlerts.Add(new ComplianceAlert
            {
                EpiItemId = item.Id,
                Type = "Inspection",
                Message = $"EPI reported as {request.Condition}."
            });
            await db.SaveChangesAsync();
        }
        return Ok(inspection);
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var now = DateTime.UtcNow;
        var certificates = await db.ApprovalCertificates.AsNoTracking().Include(x => x.EpiType).ToListAsync();
        var expiredCertificates = certificates.Where(x => x.ExpirationDate <= now).ToList();
        var nearExpiration = certificates.Where(x => x.ExpirationDate > now && x.ExpirationDate <= now.AddDays(30)).ToList();
        var alerts = await db.ComplianceAlerts.AsNoTracking().Where(x => !x.Resolved).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(new
        {
            expiredCertificates,
            nearExpiration,
            alerts,
            status = expiredCertificates.Count > 0 ? "NonCompliant" : "Compliant"
        });
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> Alerts() =>
        Ok(await db.ComplianceAlerts.AsNoTracking().Include(x => x.EpiItem)
            .Where(x => !x.Resolved).OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpPost("alerts/{id:int}/resolve")]
    [Authorize(Roles = "Administrator,SesmtManager")]
    public async Task<IActionResult> ResolveAlert(int id)
    {
        var alert = await db.ComplianceAlerts.FindAsync(id);
        if (alert is null) return NotFound();
        alert.Resolved = true;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record InspectionRequest(int EpiItemId, EpiCondition Condition, string? Notes);
