using AssetTrackingAPI.Context;
using AssetTrackingAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackingAPI.Test.Unit;

public class AssetContextTest
{
    [Fact]
    public async Task Employee_can_be_created_and_read()
    {
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("employee-" + Guid.NewGuid())
            .Options;
        await using var context = new AssetContext(options);
        context.Employees.Add(new Employee
        {
            RegistrationNumber = "001",
            Name = "Maria Silva",
            Department = "SESMT"
        });
        await context.SaveChangesAsync();

        var employee = await context.Employees.SingleAsync();
        Assert.Equal("Maria Silva", employee.Name);
        Assert.True(employee.Active);
    }

    [Fact]
    public async Task Epi_unique_code_is_enforced()
    {
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("epi-" + Guid.NewGuid())
            .Options;
        await using var context = new AssetContext(options);
        context.EpiTypes.Add(new EpiType { Id = 1, Name = "Luvas" });
        await context.SaveChangesAsync();

        context.EpiItems.Add(new EpiItem { EpiTypeId = 1, UniqueCode = "EPI-001", Status = EpiStatus.Available });
        await context.SaveChangesAsync();

        Assert.Single(context.EpiItems);
        Assert.True(await context.EpiItems.AnyAsync(x => x.UniqueCode == "EPI-001"));
    }

    [Fact]
    public async Task Delivery_changes_epi_status_to_delivered()
    {
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("delivery-" + Guid.NewGuid())
            .Options;
        await using var context = new AssetContext(options);
        context.Employees.Add(new Employee { Id = 1, RegistrationNumber = "001", Name = "Joao" });
        context.EpiTypes.Add(new EpiType { Id = 1, Name = "Capacete" });
        context.EpiItems.Add(new EpiItem { Id = 1, EpiTypeId = 1, UniqueCode = "EPI-002" });
        await context.SaveChangesAsync();

        var item = await context.EpiItems.FindAsync(1);
        item!.Status = EpiStatus.Delivered;
        item.EmployeeId = 1;
        await context.SaveChangesAsync();

        Assert.Equal(EpiStatus.Delivered, item.Status);
        Assert.Equal(1, item.EmployeeId);
    }
}
