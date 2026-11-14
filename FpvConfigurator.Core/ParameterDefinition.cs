namespace FpvConfigurator.Core;

/// <summary>Опис одного параметра комплектуючої: назва, тип, одиниці, межі.</summary>
public sealed class ParameterDefinition
{
    public ParameterDefinition(string key, string displayName, ParameterType type,
                               string? unit = null, bool required = true,
                               double? min = null, double? max = null,
                               IReadOnlyList<string>? options = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ параметра не може бути порожнім", nameof(key));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Назва параметра не може бути порожньою", nameof(displayName));
        if (min is not null && max is not null && min > max)
            throw new ArgumentException("Мінімум не може бути більшим за максимум", nameof(min));
        if (type == ParameterType.Choice && (options is null || options.Count == 0))
            throw new ArgumentException("Для типу Choice потрібен список варіантів", nameof(options));

        Key = key;
        DisplayName = displayName;
        Type = type;
        Unit = unit;
        Required = required;
        Min = min;
        Max = max;
        Options = options ?? [];
    }

    public string Key { get; }
    public string DisplayName { get; }
    public ParameterType Type { get; }
    public string? Unit { get; }
    public bool Required { get; }
    public double? Min { get; }
    public double? Max { get; }
    public IReadOnlyList<string> Options { get; }

    /// <summary>Перевіряє значення. Повертає текст помилки або null, якщо значення коректне.</summary>
    public string? Validate(object? value)
    {
        if (value is null || value is string { Length: 0 })
            return Required ? $"Параметр «{DisplayName}» обов'язковий" : null;

        switch (Type)
        {
            case ParameterType.Text:
                return value is string ? null : $"Параметр «{DisplayName}» має бути текстом";

            case ParameterType.Boolean:
                return value is bool ? null : $"Параметр «{DisplayName}» має бути так/ні";

            case ParameterType.Choice:
                return value is string s && Options.Contains(s)
                    ? null
                    : $"Параметр «{DisplayName}» має бути одним із: {string.Join(", ", Options)}";

            case ParameterType.Integer:
                if (value is not (int or long))
                    return $"Параметр «{DisplayName}» має бути цілим числом";
                return CheckRange(Convert.ToDouble(value));

            case ParameterType.Number:
                if (value is not (int or long or float or double or decimal))
                    return $"Параметр «{DisplayName}» має бути числом";
                return CheckRange(Convert.ToDouble(value));

            default:
                return $"Невідомий тип параметра {Type}";
        }
    }

    private string? CheckRange(double number)
    {
        if (Min is not null && number < Min)
            return $"Параметр «{DisplayName}» не може бути меншим за {Min}";
        if (Max is not null && number > Max)
            return $"Параметр «{DisplayName}» не може бути більшим за {Max}";
        return null;
    }
}