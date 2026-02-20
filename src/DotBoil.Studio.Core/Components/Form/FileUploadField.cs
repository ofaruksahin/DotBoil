using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace DotBoil.Studio.Core.Components.Form;

public class FileUploadField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "File Field";
    public override Type RendererType => typeof(FileUploadFieldRenderer);

    [FieldProperty(1,"Text", "Button text displayed for file upload.")]
    public string Text { get; set; }

    [FieldProperty(2,"Maximum File Count", "Maximum number of files that can be uploaded.")]
    public int MaximumFileCount { get; set; } = 1;
    
    [FieldProperty(3, "Maximum File Size", "Maximum file size in bytes.")]
    public long? MaximumFileSize { get; set; }

    [FieldProperty(4,"Multiple", "Allow selecting multiple files.")]
    public bool Multiple { get; set; }

    [FieldProperty(5, "Upload Icon", "Upload Icon displayed for file upload.", typeof(IconSelectionSettingsRenderer))]
    public Icon UploadIcon { get; set; } = new Icon
    {
        Value = Icons.Material.Filled.CloudUpload
    };
    
    [JsonIgnore]
    [FieldOutputProperty]
    public List<IBrowserFile> Value { get; set; }

    public FileUploadField()
    {
        Value = new List<IBrowserFile>();
    }
}