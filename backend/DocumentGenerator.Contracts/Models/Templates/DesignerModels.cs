namespace DocumentGenerator.Contracts.Models.Templates;

public enum DesignerElementType
{
    Text,
    Image,
    Field,
    Signature,
    Table,
    Divider,
    Header,
    Footer
}

public enum DesignerFieldType
{
    Text,
    LongText,
    Number,
    Date,
    Dropdown,
    Checkbox
}

public class DesignerDocument
{
    public PageSettings PageSettings { get; set; } = new();
    public List<DesignerElement> Elements { get; set; } = new();
}

public class PageSettings
{
    public string Width { get; set; } = "210mm";
    public string Height { get; set; } = "297mm";
    public string MarginTop { get; set; } = "20mm";
    public string MarginBottom { get; set; } = "20mm";
    public string MarginLeft { get; set; } = "15mm";
    public string MarginRight { get; set; } = "15mm";
}

public class DesignerElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DesignerElementType Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 200;
    public double Height { get; set; } = 30;
    
    // For Text/Header/Footer
    public string? Content { get; set; }
    
    // For Field
    public string? Label { get; set; }
    public DesignerFieldType? FieldType { get; set; }
    public bool Required { get; set; }
    public Guid? LookupId { get; set; }
    public string? Placeholder { get; set; }
    
    // For Image
    public string? ImageSrc { get; set; } // "assets:guid" or URL
    
    // For Table
    public List<string>? TableColumns { get; set; }
    
    // Styling
    public ElementStyles Styles { get; set; } = new();
}

public class ElementStyles
{
    public int FontSize { get; set; } = 14;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public string? FontFamily { get; set; }
    public string? Color { get; set; } = "#000000";
    public string? BackgroundColor { get; set; }
    public string TextAlign { get; set; } = "left";
    public string? BorderTop { get; set; }
    public string? BorderBottom { get; set; }
    public string? BorderLeft { get; set; }
    public string? BorderRight { get; set; }
    public int Padding { get; set; } = 4;
}

public class TemplateSaveModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string DesignJson { get; set; } = "{}";
}

public class TemplateFillModel
{
    public Guid TemplateId { get; set; }
    public Dictionary<string, string> FieldValues { get; set; } = new();
}