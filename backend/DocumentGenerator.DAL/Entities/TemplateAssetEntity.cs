namespace DocumentGenerator.DAL.Entities;

public class TemplateAssetEntity
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedOn { get; set; }

    // Navigation properties
    public TemplateEntity Template { get; set; } = null!;
}
