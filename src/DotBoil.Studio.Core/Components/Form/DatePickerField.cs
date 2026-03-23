using System.Text.Json.Serialization;
using DotBoil.Studio.Core.Attributes;
using DotBoil.Studio.Core.Contracts;
using DotBoil.Studio.Core.Enums;

namespace DotBoil.Studio.Core.Components.Form;

public class DatePickerField : BaseComponent
{
    public override string Category => "Form";
    public override string Title => "Date Picker";
    public override Type RendererType => typeof(DatePickerFieldRenderer);

    [FieldProperty(1, "Label", "The label text shown above the date input.")]
    public string Label { get; set; }

    [FieldProperty(2, "Helper Text", "Helper description shown below the picker.")]
    public string HelperText { get; set; }

    [FieldProperty(3, "Format", "Picker mode: Date, Time or Date & Time.", typeof(DatePickerFormatTypeSettingsRenderer))]
    public DatePickerFormatType Format { get; set; } = DatePickerFormatType.Date;

    [FieldProperty(4, "Hour Format", "Time display format (24H or 12H).", typeof(DatePickerHourFormatTypeSettingsRenderer))]
    public DatePickerHourFormatType HourFormat { get; set; } = DatePickerHourFormatType.Hour24;

    [FieldProperty(5, "Minimum", "Minimum allowed date/time value.")]
    public DateTime? Minimum { get; set; }

    [FieldProperty(6, "Maximum", "Maximum allowed date/time value.")]
    public DateTime? Maximum { get; set; }

    [FieldProperty(7, "Format String", "Display format (e.g. dd/MM/yyyy, MM/dd/yyyy, HH:mm).")]
    public string FormatString { get; set; } = "dd/MM/yyyy";

    [FieldOutputProperty]
    [JsonIgnore]
    public DateTime? Value { get; set; }
}