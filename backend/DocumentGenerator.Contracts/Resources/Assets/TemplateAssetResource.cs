namespace DocumentGenerator.Contracts.Resources.Assets;

public class TemplateAssetResource
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
