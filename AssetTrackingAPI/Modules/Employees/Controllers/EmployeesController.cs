using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Modules.Employees.Controllers;

[ApiController]
[Authorize]
[Route("api/employees")]
public class EmployeesController(AssetContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Employees.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,SesmtManager")]
    public async Task<IActionResult> Create(EmployeeRequest request)
    {
        var employee = new Employee
        {
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Name = request.Name.Trim(),
            Department = request.Department?.Trim(),
            Position = request.Position?.Trim()
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        await Log("Create", nameof(Employee), employee.Id);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrator,SesmtManager")]
    public async Task<IActionResult> Update(int id, EmployeeRequest request)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null) return NotFound();
        employee.RegistrationNumber = request.RegistrationNumber.Trim();
        employee.Name = request.Name.Trim();
        employee.Department = request.Department?.Trim();
        employee.Position = request.Position?.Trim();
        await db.SaveChangesAsync();
        await Log("Update", nameof(Employee), employee.Id);
        return NoContent();
    }

    [HttpGet("{id:int}/epi-history")]
    public async Task<IActionResult> EpiHistory(int id)
    {
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (employee is null) return NotFound();
        var deliveries = await db.DeliveryItems
            .AsNoTracking()
            .Include(x => x.Delivery)
            .Where(x => x.Delivery != null && x.Delivery.EmployeeId == id)
            .Select(x => new { x.EpiItemId, x.Delivery!.DeliveryDate, Action = "Delivered" })
            .ToListAsync();
        var returns = await db.Returns.AsNoTracking()
            .Where(x => x.EmployeeId == id)
            .Select(x => new { x.EpiItemId, ReturnDate = x.ReturnDate, Action = "Returned" })
            .ToListAsync();
        return Ok(new { employee, deliveries, returns });
    }

    private async Task Log(string action, string entity, int id)
    {
        db.AuditLogs.Add(new AuditLog { Action = action, Entity = entity, Details = $"Id={id}" });
        await db.SaveChangesAsync();
    }
}

public record EmployeeRequest(string RegistrationNumber, string Name, string? Department, string? Position);
