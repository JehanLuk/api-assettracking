using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetTrackingAPI.Model;

namespace AssetTrackingAPI.Model;

public class Asset
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "The asset must have a name!")]
    [StringLength(255)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "The description is required!")]
    [Column(TypeName = "text")]
    public string Description { get; set; } = default!;

    [Required]
    public AssetEnumStatus Status { get; set; } = AssetEnumStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
}