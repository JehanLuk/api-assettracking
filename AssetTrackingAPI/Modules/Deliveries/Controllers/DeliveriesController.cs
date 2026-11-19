using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.Deliveries.Controllers;

[ApiController]
[Authorize]
[Route("api/deliveries")]
public class DeliveriesController(AssetContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Deliveries.AsNoTracking().Include(x => x.Employee).Include(x => x.ResponsibleUser)
            .Include(x => x.Items).ThenInclude(x => x.EpiItem).OrderByDescending(x => x.DeliveryDate).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Administrator,WarehouseResponsible")]
    public async Task<IActionResult> Deliver(DeliveryRequest request)
    {
        if (!await db.Employees.AnyAsync(x => x.Id == request.EmployeeId))
            return BadRequest(new { message = "Invalid employee." });
        if (request.EpiItemIds.Count == 0) return BadRequest(new { message = "At least one EPI is required." });

        var items = await db.EpiItems.Where(x => request.EpiItemIds.Contains(x.Id)).ToListAsync();
        if (items.Count != request.EpiItemIds.Count || items.Any(x => x.Status != EpiStatus.Available))
            return Conflict(new { message = "One or more EPIs are unavailable." });

        var delivery = new Delivery { EmployeeId = request.EmployeeId, DeliveryDate = DateTime.UtcNow };
        foreach (var item in items)
        {
            item.Status = EpiStatus.Delivered;
            item.EmployeeId = request.EmployeeId;
            delivery.Items.Add(new DeliveryItem { EpiItem = item });
        }
        db.Deliveries.Add(delivery);
        await db.SaveChangesAsync();
        db.AuditLogs.Add(new AuditLog { Action = "Deliver", Entity = nameof(Delivery), Details = $"Id={delivery.Id}" });
        await db.SaveChangesAsync();
        return Created($"/api/deliveries/{delivery.Id}", delivery);
    }

    [HttpPost("{epiItemId:int}/return")]
    [Authorize(Roles = "Administrator,WarehouseResponsible")]
    public async Task<IActionResult> Return(int epiItemId, ReturnRequest request)
    {
        var item = await db.EpiItems.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == epiItemId);
        if (item is null) return NotFound();
        if (item.EmployeeId is null) return Conflict(new { message = "EPI is not currently delivered." });

        var record = new ReturnRecord
        {
            EpiItemId = item.Id,
            EmployeeId = item.EmployeeId.Value,
            ReturnDate = DateTime.UtcNow,
            Reason = request.Reason?.Trim()
        };
        item.Status = request.AvailableForUse ? EpiStatus.Available : EpiStatus.Maintenance;
        item.EmployeeId = null;
        db.Returns.Add(record);
        await db.SaveChangesAsync();
        db.AuditLogs.Add(new AuditLog { Action = "Return", Entity = nameof(ReturnRecord), Details = $"EpiId={item.Id}" });
        await db.SaveChangesAsync();
        return Ok(record);
    }
}

public record DeliveryRequest(int EmployeeId, List<int> EpiItemIds);
public record ReturnRequest(bool AvailableForUse, string? Reason);
