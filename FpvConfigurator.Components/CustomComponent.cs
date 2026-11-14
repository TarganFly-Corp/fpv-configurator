using System.Globalization;
using FpvConfigurator.Core;

namespace FpvConfigurator.Components;

/// <summary>
/// Користувацька комплектуюча з довільною категорією (GPS-модуль, приймач, підвіс камери тощо).
/// Параметри перевіряються за схемою категорії під час створення.
/// </summary>
public sealed class CustomComponent : ComponentBase
{
    private readonly Dictionary<string, object?> _parameters;

    public CustomComponent(ComponentInfo info, ComponentCategory category,
                           IReadOnlyDictionary<string, object?> parameters)
        : base(info.Id, info.Name, info.Manufacturer, info.Price, info.WeightGrams, info.InStock)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(parameters);

        var errors = category.Validate(parameters);
        if (errors.Count > 0)
            throw new ArgumentException(
                $"Некоректні параметри для категорії «{category.DisplayName}»: {string.Join("; ", errors)}",
                nameof(parameters));

        Category = category;
        _parameters = new Dictionary<string, object?>(parameters);
    }

    public ComponentCategory Category { get; }

    public override ComponentType Type => ComponentType.Custom;
    public override string CategoryKey => Category.Key;
    public override IReadOnlyDictionary<string, object?> Parameters => _parameters;

    public override IReadOnlyDictionary<string, string> GetSpecs()
    {
        var specs = new Dictionary<string, string>();
        foreach (var parameter in Category.Parameters)
        {
            if (_parameters.TryGetValue(parameter.Key, out var value) && value is not null)
                specs[parameter.DisplayName] = Format(parameter, value);
        }
        return specs;
    }

    private static string Format(ParameterDefinition parameter, object value)
    {
        var text = value switch
        {
            bool b => b ? "так" : "ні",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
        return parameter.Unit is null ? text : $"{text} {parameter.Unit}";
    }
}