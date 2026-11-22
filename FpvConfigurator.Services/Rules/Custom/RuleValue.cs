using System.Globalization;
using System.Text.Json;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules.Custom;

/// <summary>Перетворення і порівняння значень параметрів для правил користувача.</summary>
public static class RuleValue
{
    public static bool IsNumeric(ParameterType type) => type is ParameterType.Integer or ParameterType.Number;

    /// <summary>Чи можна порівнювати параметри цих двох типів.</summary>
    public static bool AreComparable(ParameterType left, ParameterType right) =>
        (IsNumeric(left) && IsNumeric(right))
        || (IsText(left) && IsText(right))
        || left == right;

    private static bool IsText(ParameterType type) => type is ParameterType.Text or ParameterType.Choice;

    /// <summary>
    /// Розбирає значення з поля форми за типом параметра.
    /// Числа приймаються і з крапкою, і з комою. Повертає null, якщо значення не підходить.
    /// </summary>
    public static object? Parse(ParameterDefinition parameter, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        text = text.Trim();
        switch (parameter.Type)
        {
            case ParameterType.Integer:
            case ParameterType.Number:
                return double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                    ? number
                    : null;
            case ParameterType.Boolean:
                return text.ToLowerInvariant() switch
                {
                    "так" or "true" => true,
                    "ні" or "false" => false,
                    _ => null
                };
            case ParameterType.Choice:
                return parameter.Options.FirstOrDefault(o => string.Equals(o, text, StringComparison.OrdinalIgnoreCase));
            default:
                return text;
        }
    }

    /// <summary>
    /// Порівнює два значення. Повертає null, якщо їх неможливо порівняти
    /// (одного значення немає або типи різні). Тоді правило деталь пропускає.
    /// </summary>
    public static bool? Compare(object? left, RuleOperator op, object? right)
    {
        left = Unwrap(left);
        right = Unwrap(right);
        if (left is null || right is null)
            return null;

        if (ToNumber(left) is { } a && ToNumber(right) is { } b)
            return op switch
            {
                RuleOperator.Equal => a.Equals(b),
                RuleOperator.NotEqual => !a.Equals(b),
                RuleOperator.Less => a < b,
                RuleOperator.LessOrEqual => a <= b,
                RuleOperator.Greater => a > b,
                RuleOperator.GreaterOrEqual => a >= b,
                _ => null
            };

        if (op.IsOrdering())
            return null;
        bool equal = left is string ls && right is string rs
            ? string.Equals(ls, rs, StringComparison.OrdinalIgnoreCase)
            : left.Equals(right);
        return op == RuleOperator.Equal ? equal : !equal;
    }

    /// <summary>Значення для повідомлення: числа в українському форматі, так/ні замість True/False.</summary>
    public static string Format(object? value) => Unwrap(value) switch
    {
        null => "—",
        bool b => b ? "так" : "ні",
        string s => s,
        var v when ToNumber(v) is { } n => n.ToString("0.##", CultureInfo.GetCultureInfo("uk-UA")),
        var v => v.ToString() ?? ""
    };

    private static double? ToNumber(object value) => value switch
    {
        int i => i,
        long l => l,
        float f => f,
        double d => d,
        decimal m => (double)m,
        _ => null
    };

    // Значення з JSON можуть прийти як JsonElement: розгортаємо в звичайні типи.
    private static object? Unwrap(object? value) => value is JsonElement e
        ? e.ValueKind switch
        {
            JsonValueKind.Number => e.GetDouble(),
            JsonValueKind.String => e.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        }
        : value;
}