using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.EPIs.Controllers;

[ApiController]
[Authorize]
[Route("api/epis")]
public class EpiController(AssetContext db) : ControllerBase
{
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes() =>
        Ok(await db.EpiTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpPost("types")]
    [Authorize(Roles = "Administrator,SesmtManager,WarehouseResponsible")]
    public async Task<IActionResult> CreateType(CreateEpiTypeRequest request)
    {
        var type = new EpiType { Name = request.Name.Trim(), Description = request.Description?.Trim() };
        db.EpiTypes.Add(type);
        await db.SaveChangesAsync();
        return Created($"/api/epis/types/{type.Id}", type);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems() =>
        Ok(await db.EpiItems.AsNoTracking().Include(x => x.EpiType).Include(x => x.Employee).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await db.EpiItems.AsNoTracking().Include(x => x.EpiType).Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("qr/{uniqueCode}")]
    public async Task<IActionResult> GetByQrCode(string uniqueCode)
    {
        var item = await db.EpiItems.AsNoTracking().Include(x => x.EpiType).Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.UniqueCode == uniqueCode);
        return item is null ? NotFound(new { message = "EPI not found." }) : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,WarehouseResponsible")]
    public async Task<IActionResult> CreateItem(CreateEpiItemRequest request)
    {
        if (!await db.EpiTypes.AnyAsync(x => x.Id == request.EpiTypeId))
            return BadRequest(new { message = "Invalid EPI type." });
        if (await db.EpiItems.AnyAsync(x => x.UniqueCode == request.UniqueCode))
            return Conflict(new { message = "Unique code already exists." });

        var item = new EpiItem { EpiTypeId = request.EpiTypeId, UniqueCode = request.UniqueCode.Trim(), Status = EpiStatus.Available };
        db.EpiItems.Add(item);
        await db.SaveChangesAsync();
        db.AuditLogs.Add(new AuditLog { Action = "Create", Entity = nameof(EpiItem), Details = $"Id={item.Id}" });
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpPost("{id:int}/certificate")]
    [Authorize(Roles = "Administrator,SesmtManager")]
    public async Task<IActionResult> RegisterCertificate(int id, CertificateRequest request)
    {
        var item = await db.EpiItems.Include(x => x.EpiType).FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        var certificate = new ApprovalCertificate
        {
            EpiTypeId = item.EpiTypeId,
            CertificateNumber = request.CertificateNumber.Trim(),
            ExpirationDate = DateTime.SpecifyKind(request.ExpirationDate, DateTimeKind.Utc)
        };
        db.ApprovalCertificates.Add(certificate);
        await db.SaveChangesAsync();
        return Ok(certificate);
    }

    [HttpGet("certificates")]
    public async Task<IActionResult> GetCertificates() =>
        Ok(await db.ApprovalCertificates.AsNoTracking().Include(x => x.EpiType).OrderBy(x => x.ExpirationDate).ToListAsync());
}

public record CreateEpiTypeRequest(string Name, string? Description);
public record CreateEpiItemRequest(int EpiTypeId, string UniqueCode);
public record CertificateRequest(string CertificateNumber, DateTime ExpirationDate);
