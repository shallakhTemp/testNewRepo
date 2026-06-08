using System.ComponentModel.DataAnnotations;

namespace DocumentGenerator.Contracts.Models.Lookups;

public class LookupModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
