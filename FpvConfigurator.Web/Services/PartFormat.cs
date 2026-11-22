using System.Globalization;
using FpvConfigurator.Core;

namespace FpvConfigurator.Web.Services;

/// <summary>Як показувати ціни, вагу і значення параметрів в інтерфейсі.</summary>
public static class PartFormat
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("uk-UA");

    public static string Price(decimal price) => price.ToString("#,0.##", Culture) + " грн";

    public static string Weight(double grams) => grams.ToString("#,0.##", Culture) + " г";

    /// <summary>Значення параметра з одиницями за схемою категорії (ФВ-09, ФВ-18). Немає значення: «—».</summary>
    public static string Value(ParameterDefinition parameter, object? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        if (value is null || value is string { Length: 0 })
            return "—";

        var text = value switch
        {
            bool b => b ? "так" : "ні",
            IFormattable f => f.ToString(null, Culture),
            _ => value.ToString() ?? ""
        };
        return parameter.Unit is null ? text : $"{text} {parameter.Unit}";
    }

    /// <summary>Опис обмежень параметра для підказки у формі: межі або список варіантів.</summary>
    public static string Limits(ParameterDefinition parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        var unit = parameter.Unit is null ? "" : " " + parameter.Unit;
        return parameter switch
        {
            { Type: ParameterType.Choice } => string.Join(", ", parameter.Options),
            { Min: not null, Max: not null } => $"від {parameter.Min.Value.ToString(Culture)} до {parameter.Max.Value.ToString(Culture)}{unit}",
            { Min: not null } => $"не менше {parameter.Min.Value.ToString(Culture)}{unit}",
            { Max: not null } => $"не більше {parameter.Max.Value.ToString(Culture)}{unit}",
            _ => ""
        };
    }

    public static string TypeName(ParameterType type) => type switch
    {
        ParameterType.Text => "Текст",
        ParameterType.Integer => "Ціле число",
        ParameterType.Number => "Дробове число",
        ParameterType.Boolean => "Так/ні",
        ParameterType.Choice => "Вибір зі списку",
        _ => type.ToString()
    };

    /// <summary>Назва категорії за ключем, або сам ключ, якщо категорії немає в реєстрі.</summary>
    public static string CategoryName(ICategoryRegistry registry, string key) =>
        registry.TryGet(key, out var category) ? category.DisplayName : key;

    /// <summary>
    /// Перетворює текст з форми на значення для схеми: ціле, дробове (крапка або кома), так/ні, текст.
    /// Якщо текст не розбирається як число, повертає його як є, і перевірка схеми покаже помилку типу.
    /// </summary>
    public static object? Parse(ParameterDefinition parameter, string? text)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        if (string.IsNullOrWhiteSpace(text))
            return null;
        text = text.Trim();

        return parameter.Type switch
        {
            ParameterType.Integer => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : text,
            ParameterType.Number => double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : text,
            ParameterType.Boolean => bool.TryParse(text, out var b) ? b : text,
            _ => text
        };
    }

    /// <summary>Текст винятку без хвоста «(Parameter 'x')», який додає ArgumentException.</summary>
    public static string Message(Exception ex) =>
        ex is ArgumentException { ParamName: not null } a
            ? a.Message.Replace($" (Parameter '{a.ParamName}')", "")
            : ex.Message;
}