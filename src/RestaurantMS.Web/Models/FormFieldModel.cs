namespace RestaurantMS.Web.Models;

public class FormFieldModel
{
    public string Name { get; init; } = string.Empty;
    public string? Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string InputType { get; init; } = "text"; // text, number, email, date, time, textarea, select, toggle, stepper
    public string? Placeholder { get; init; }
    public string? HelperText { get; init; }
    public string? UnitPrefix { get; init; }
    public string? UnitSuffix { get; init; }
    public bool Required { get; init; }
    public bool ReadOnly { get; init; }
    public string ColClass { get; init; } = "col-12 col-md-6";
    public string? Min { get; init; }
    public string? Max { get; init; }
    public string? Step { get; init; }
    public int? MaxLength { get; init; }
    public string? Pattern { get; init; }
    public int Rows { get; init; } = 2;
    public IReadOnlyList<(string Value, string Text)>? Options { get; init; }
    public bool Checked { get; init; }
}
