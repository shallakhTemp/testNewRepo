namespace DocumentGenerator.DAL.Entities;

public class TemplateEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DesignJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Navigation properties
    public ICollection<TemplateAssetEntity> Assets { get; set; } = new List<TemplateAssetEntity>();
}