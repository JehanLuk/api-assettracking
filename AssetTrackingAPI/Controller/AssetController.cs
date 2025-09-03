using AssetTrackingAPI.DTO;
using AssetTrackingAPI.Context;
using AssetTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrackingAPI.Controller;

[ApiController]
[Route("[controller]")]
public class AssetController(AssetContext _context) : ControllerBase
{
    // Getting all assets (GET -> /GetAll)
    [HttpGet("GetAllAssets")]
    public async Task<IActionResult> GetAllAssets()
    {
        var assets = await _context.Assets.ToListAsync();
        return Ok(assets);
    }

    // Getting asset by ID (GET -> /GetAsset/{id})
    [HttpGet("GetAsset/{id:int}")]
    public async Task<IActionResult> GetAssetById(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        return Ok(asset);
    }

    // Posting one asset (POST -> /PostAsset)
    [HttpPost("PostAsset")]
    public async Task<IActionResult> CreateAsset([FromBody] AssetDTO dto)
    {
        var asset = new Asset
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, asset);
    }

    [HttpPut("UpdateAsset/{id:int}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetDTO dto)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) 
            return NotFound(new { message = $"Asset with Id {id} not found!"});

        asset.Name = dto.Name;
        asset.Description = dto.Description;
        asset.Status = dto.Status;
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("DeleteAsset/{id:int}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) 
            return NotFound(new { message = $"Asset with Id {id} not found!"});

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Asset with Id {id} deleted successfully!"});
    }
}