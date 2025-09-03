namespace AssetTrackingAPI.DTO;

public class AssetDTO
{
    [Required(ErrorMessage = "The asset must have a name!")]
    [StringLength(255)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "The description is required!")]
    [Column(TypeName = "text")]
    public string Description { get; set; }

    public AssetEnumStatus Status { get; set; } = AssetEnumStatus.Active;
}