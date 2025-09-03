using Microsoft.EntityFrameworkCore;
using AssetTrackingAPI.Model;

namespace AssetTrackingAPI.Context;

public class AssetContext : DbContext 
{
    public AssetContext(DbContextOptions<AssetContext> options) : base(options) {} 

    public DbSet<Asset> Assets { get; set; }
}