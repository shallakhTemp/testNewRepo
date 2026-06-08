using System.ComponentModel.DataAnnotations;

namespace DocumentGenerator.Contracts.Models.Lookups;

public class LookupItemModel
{
    [Required]
    public Guid LookupId { get; set; }

    [Required]
    [StringLength(200)]
    public string Value { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Text { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
