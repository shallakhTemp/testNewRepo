namespace DocumentGenerator.DAL.Entities;

public class LookupItemEntity
{
    public Guid Id { get; set; }
    public Guid LookupId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    // Navigation properties
    public LookupEntity Lookup { get; set; } = null!;
}
