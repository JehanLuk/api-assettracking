using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.Inventory.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public class InventoryController(AssetContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var data = await db.EpiTypes.AsNoTracking().Select(type => new
        {
            type.Id,
            type.Name,
            Total = db.EpiItems.Count(item => item.EpiTypeId == type.Id),
            Available = db.EpiItems.Count(item => item.EpiTypeId == type.Id && item.Status == EpiStatus.Available),
            Delivered = db.EpiItems.Count(item => item.EpiTypeId == type.Id && item.Status == EpiStatus.Delivered),
            Maintenance = db.EpiItems.Count(item => item.EpiTypeId == type.Id && item.Status == EpiStatus.Maintenance)
        }).ToListAsync();
        return Ok(data);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> Movements() =>
        Ok(await db.StockMovements.AsNoTracking().Include(x => x.EpiType).Include(x => x.User)
            .OrderByDescending(x => x.MovementDate).ToListAsync());

    [HttpPost("movements")]
    [Authorize(Roles = "Administrator,WarehouseResponsible")]
    public async Task<IActionResult> CreateMovement(CreateMovementRequest request)
    {
        if (request.Quantity == 0) return BadRequest(new { message = "Quantity must not be zero." });
        if (!await db.EpiTypes.AnyAsync(x => x.Id == request.EpiTypeId))
            return BadRequest(new { message = "Invalid EPI type." });

        var movement = new StockMovement
        {
            EpiTypeId = request.EpiTypeId,
            Type = request.Type,
            Quantity = request.Quantity,
            MovementDate = DateTime.UtcNow
        };
        db.StockMovements.Add(movement);
        await db.SaveChangesAsync();
        db.AuditLogs.Add(new AuditLog { Action = "Movement", Entity = nameof(StockMovement), Details = $"Type={request.Type},Quantity={request.Quantity}" });
        await db.SaveChangesAsync();
        return Created($"/api/inventory/movements/{movement.Id}", movement);
    }
}

public record CreateMovementRequest(int EpiTypeId, StockMovementType Type, int Quantity);
