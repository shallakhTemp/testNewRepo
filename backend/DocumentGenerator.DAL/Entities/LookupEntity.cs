namespace DocumentGenerator.DAL.Entities;

public class LookupEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Navigation properties
    public ICollection<LookupItemEntity> Items { get; set; } = new List<LookupItemEntity>();
}
