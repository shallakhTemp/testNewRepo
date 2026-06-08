namespace DocumentGenerator.Contracts.Resources.Lookups;

public class LookupResource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<LookupItemResource> Items { get; set; } = new();
}
