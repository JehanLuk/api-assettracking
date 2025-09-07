using Xunit;
using AssetTrackingAPI.Controller;
using AssetTrackingAPI.Context;
using AssetTrackingAPI.DTO;
using AssetTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AssetTrackingAPI.Test.Unit;

public class AssetServiceTest
{
    // GET /Asset/GetAsset/{id} - asset does not exist
    [Fact]
    public async Task GetAssetById_ReturnsNotFound_WhenAssetDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_GetById_NotExists")
            .Options;
        await using var context = new AssetContext(options);
        var controller = new AssetController(context);

        int nonExistentId = 999;

        // Act
        var result = await controller.GetAssetById(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // GET /Asset/GetAllAssets - no assets
    [Fact]
    public async Task GetAllAssets_ReturnsEmptyList_WhenNoAssets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_GetAll_Empty")
            .Options;
        await using var context = new AssetContext(options);
        var controller = new AssetController(context);

        // Act
        var result = await controller.GetAllAssets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var assets = Assert.IsType<List<Asset>>(okResult.Value);
        Assert.Empty(assets);
    }

    // GET /Asset/GetAllAssets - with assets
    [Fact]
    public async Task GetAllAssets_ReturnsAllAssets_WhenAssetsExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_GetAll_Filled")
            .Options;
        await using var context = new AssetContext(options);
        context.Assets.AddRange(
            new Asset { Name = "PC1", Description = "Dell", Status = AssetEnumStatus.Active },
            new Asset { Name = "PC2", Description = "HP", Status = AssetEnumStatus.InUse }
        );
        await context.SaveChangesAsync();
        var controller = new AssetController(context);

        // Act
        var result = await controller.GetAllAssets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var assets = Assert.IsType<List<Asset>>(okResult.Value);
        Assert.Equal(2, assets.Count);
    }

    // GET /Asset/GetAsset/{id} - asset exists
    [Fact]
    public async Task GetAssetById_ReturnsAsset_WhenAssetExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_GetById_Exists")
            .Options;
        await using var context = new AssetContext(options);
        var asset = new Asset { Name = "Laptop", Description = "Dell", Status = AssetEnumStatus.Active };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();
        var controller = new AssetController(context);

        // Act
        var result = await controller.GetAssetById(asset.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedAsset = Assert.IsType<Asset>(okResult.Value);
        Assert.Equal(asset.Id, returnedAsset.Id);
        Assert.Equal(AssetEnumStatus.Active, returnedAsset.Status);
    }

    // POST /Asset/PostAsset - valid DTO
    [Fact]
    public async Task CreateAsset_ReturnsCreated_WhenDtoIsValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_Post_Valid")
            .Options;
        await using var context = new AssetContext(options);
        var controller = new AssetController(context);
        var dto = new AssetDTO
        {
            Name = "Laptop",
            Description = "Dell",
            Status = AssetEnumStatus.Active
        };

        // Act
        var result = await controller.CreateAsset(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var asset = Assert.IsType<Asset>(createdResult.Value);
        Assert.Equal("Laptop", asset.Name);
        Assert.Equal("Dell", asset.Description);
        Assert.Equal(AssetEnumStatus.Active, asset.Status);
    }

    // PUT /Asset/UpdateAsset/{id} - asset exists
    [Fact]
    public async Task UpdateAsset_ReturnsNoContent_WhenAssetExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_Put_Exists")
            .Options;
        await using var context = new AssetContext(options);
        var asset = new Asset { Name = "PC", Description = "Old", Status = AssetEnumStatus.Active };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();
        var controller = new AssetController(context);
        var dto = new AssetDTO { Name = "PC", Description = "Updated", Status = AssetEnumStatus.InUse };

        // Act
        var result = await controller.UpdateAsset(asset.Id, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedAsset = await context.Assets.FindAsync(asset.Id)!;
        Assert.Equal("Updated", updatedAsset!.Description);
        Assert.Equal(AssetEnumStatus.InUse, updatedAsset.Status);
    }

    // PUT /Asset/UpdateAsset/{id} - asset doest not exist
    [Fact]
    public async Task UpdateAsset_ReturnsNotFound_WhenAssetDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_Put_NotExists")
            .Options;
        await using var context = new AssetContext(options);
        var controller = new AssetController(context);
        var dto = new AssetDTO { Name = "PC", Description = "Updated", Status = AssetEnumStatus.InUse };

        // Act
        var result = await controller.UpdateAsset(999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // DELETE /Asset/DeleteAsset/{id} - asset exists
    [Fact]
    public async Task DeleteAsset_ReturnsOk_WhenAssetExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_Delete_Exists")
            .Options;
        await using var context = new AssetContext(options);
        var asset = new Asset { Name = "PC", Description = "Dell", Status = AssetEnumStatus.Active };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();
        var controller = new AssetController(context);

        // Act
        var result = await controller.DeleteAsset(asset.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("deleted successfully", okResult.Value!.ToString());
    }

    // DELETE /Asset/DeleteAsset/{id} - asset does not exist
    [Fact]
    public async Task DeleteAsset_ReturnsNotFound_WhenAssetDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssetContext>()
            .UseInMemoryDatabase("Db_Delete_NotExists")
            .Options;
        await using var context = new AssetContext(options);
        var controller = new AssetController(context);

        // Act
        var result = await controller.DeleteAsset(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}