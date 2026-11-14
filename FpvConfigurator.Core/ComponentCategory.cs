namespace FpvConfigurator.Core;

/// <summary>
/// Категорія комплектуючих (рама, мотор, GPS-модуль тощо) і схема її параметрів.
/// Стандартні категорії описані в коді, користувацькі можна додати під час роботи програми.
/// </summary>
public sealed class ComponentCategory
{
    public ComponentCategory(string key, string displayName,
                             IEnumerable<ParameterDefinition> parameters, bool isStandard = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ категорії не може бути порожнім", nameof(key));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Назва категорії не може бути порожньою", nameof(displayName));
        ArgumentNullException.ThrowIfNull(parameters);

        var list = parameters.ToList();
        var duplicate = list.GroupBy(p => p.Key).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException($"Параметр «{duplicate.Key}» описано двічі", nameof(parameters));

        Key = key;
        DisplayName = displayName;
        IsStandard = isStandard;
        Parameters = list;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public bool IsStandard { get; }
    public IReadOnlyList<ParameterDefinition> Parameters { get; }

    public ParameterDefinition? FindParameter(string key) =>
        Parameters.FirstOrDefault(p => p.Key == key);

    /// <summary>Перевіряє набір значень за схемою. Повертає список помилок (порожній, якщо все добре).</summary>
    public IReadOnlyList<string> Validate(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var errors = new List<string>();

        foreach (var parameter in Parameters)
        {
            values.TryGetValue(parameter.Key, out var value);
            var error = parameter.Validate(value);
            if (error is not null)
                errors.Add(error);
        }

        foreach (var key in values.Keys.Where(k => FindParameter(k) is null))
            errors.Add($"Категорія «{DisplayName}» не має параметра «{key}»");

        return errors;
    }

    public override string ToString() => $"{DisplayName} ({Key})";
}