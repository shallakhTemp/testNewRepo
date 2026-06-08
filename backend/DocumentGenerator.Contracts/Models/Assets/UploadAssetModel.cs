using System.ComponentModel.DataAnnotations;

namespace DocumentGenerator.Contracts.Models.Assets;

public class UploadAssetModel
{
    [Required]
    public Guid TemplateId { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}
