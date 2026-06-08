namespace DocumentGenerator.Contracts.Resources.Lookups;

public class LookupItemResource
{
    public Guid Id { get; set; }
    public Guid LookupId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
