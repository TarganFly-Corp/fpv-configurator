using FpvConfigurator.Core;

namespace FpvConfigurator.Services.Rules.Custom;

/// <summary>
/// Правила сумісності користувача (ФВ-28). Для BuildService це одне правило «Правила користувача»,
/// а всередині список описів, який можна змінювати під час роботи програми.
/// Кожне повідомлення починається з назви правила в лапках.
/// </summary>
public sealed class CustomRuleSet : ICompatibilityRule
{
    private readonly ICategoryRegistry _registry;
    private readonly List<CustomRuleDefinition> _rules = [];
    private readonly Lock _lock = new();

    public CustomRuleSet(ICategoryRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public string Name => "Правила користувача";

    /// <summary>Подія після додавання або видалення правила.</summary>
    public event Action? Changed;

    public IReadOnlyList<CustomRuleDefinition> Rules
    {
        get { lock (_lock) return _rules.ToList(); }
    }

    /// <remarks>
    /// Тестові дані:
    /// 1. Правильне правило Esc.continuousCurrentA ≥ Motor.maxCurrentA: помилок немає.
    /// 2. Порожня назва, назва, яка вже є, невідома категорія, невідомий параметр: помилка біля свого поля.
    /// 3. Операція «≥» для текстового параметра, число проти тексту: помилка біля операції або правої частини.
    /// 4. Значення «abc» для числового параметра, «2,5» з комою: перше помилка, друге правильне.
    /// </remarks>
    /// <summary>Перевіряє опис правила за схемами категорій. Повертає список помилок.</summary>
    public IReadOnlyList<RuleError> Validate(CustomRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var errors = new List<RuleError>();

        if (string.IsNullOrWhiteSpace(rule.Name))
            errors.Add(new("name", "Вкажіть назву правила"));
        else if (Rules.Any(r => string.Equals(r.Name, rule.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            errors.Add(new("name", $"Правило «{rule.Name.Trim()}» вже є"));

        var left = FindParameter(rule.LeftCategory, rule.LeftParameter, "left", errors);

        if (rule.ComparesParts)
        {
            var right = FindParameter(rule.RightCategory, rule.RightParameter, "right", errors);
            if (left is not null && right is not null)
            {
                if (!RuleValue.AreComparable(left.Type, right.Type))
                    errors.Add(new("right", $"Параметри «{left.DisplayName}» і «{right.DisplayName}» мають різні типи"));
                else if (rule.Operator.IsOrdering() && !RuleValue.IsNumeric(left.Type))
                    errors.Add(new("operator", $"Операція {rule.Operator.Symbol()} працює лише з числами"));
            }
        }
        else if (left is not null)
        {
            if (rule.Operator.IsOrdering() && !RuleValue.IsNumeric(left.Type))
                errors.Add(new("operator", $"Операція {rule.Operator.Symbol()} працює лише з числами"));
            if (string.IsNullOrWhiteSpace(rule.Constant))
                errors.Add(new("constant", "Вкажіть значення для порівняння"));
            else if (RuleValue.Parse(left, rule.Constant) is null)
                errors.Add(new("constant", ConstantHint(left, rule.Constant)));
        }

        return errors;
    }

    /// <summary>Додає правило. Якщо опис неправильний, кидає ArgumentException з першою помилкою.</summary>
    public void Add(CustomRuleDefinition rule)
    {
        var errors = Validate(rule);
        if (errors.Count > 0)
            throw new ArgumentException(errors[0].Message, nameof(rule));
        lock (_lock)
            _rules.Add(rule with { Name = rule.Name.Trim() });
        Changed?.Invoke();
    }

    /// <summary>Видаляє правило за назвою (регістр не важливий). Повертає false, якщо такого немає.</summary>
    public bool Remove(string name)
    {
        bool removed;
        lock (_lock)
            removed = _rules.RemoveAll(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;
        if (removed)
            Changed?.Invoke();
        return removed;
    }

    /// <remarks>
    /// Тестові дані:
    /// 1. ESC 35 A і мотор 45 A з правилом «≥»: одна проблема з назвою правила, деталями і значеннями.
    /// 2. ESC 50 A і мотор 40 A: проблем немає.
    /// 3. Чотири однакові мотори: одна проблема, а не чотири.
    /// 4. У збірці немає деталі однієї з категорій: проблем немає.
    /// 5. Порівняння зі значенням (Battery.cells ≥ 4) і з текстом (Frame.stackMount = 30.5x30.5).
    /// </remarks>
    public IEnumerable<string> Check(IReadOnlyCollection<IComponent> build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            if (!_registry.TryGet(rule.LeftCategory, out var leftCategory)
                || Find(leftCategory, rule.LeftParameter) is not { } leftParameter)
                continue;

            foreach (var left in PartsOf(build, rule.LeftCategory))
            {
                var leftValue = ValueOf(left, leftParameter.Key);
                if (rule.ComparesParts)
                {
                    if (!_registry.TryGet(rule.RightCategory!, out var rightCategory)
                        || Find(rightCategory, rule.RightParameter) is not { } rightParameter)
                        continue;
                    foreach (var right in PartsOf(build, rule.RightCategory!)
                                 .Where(p => !string.Equals(p.Id, left.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        var rightValue = ValueOf(right, rightParameter.Key);
                        if (RuleValue.Compare(leftValue, rule.Operator, rightValue) == false)
                            problems.Add($"«{rule.Name}»: {left.Name}, {Describe(leftParameter, leftValue)}, " +
                                         $"а має бути {rule.Operator.Symbol()} {Describe(rightParameter, rightValue)} ({right.Name})");
                    }
                }
                else
                {
                    var constant = RuleValue.Parse(leftParameter, rule.Constant);
                    if (RuleValue.Compare(leftValue, rule.Operator, constant) == false)
                        problems.Add($"«{rule.Name}»: {left.Name}, {Describe(leftParameter, leftValue)}, " +
                                     $"а має бути {rule.Operator.Symbol()} {RuleValue.Format(constant)}{Unit(leftParameter)}");
                }
            }
        }
        return problems;
    }

    /// <summary>Читабельний опис правила для інтерфейсу: «Мотор · Макс. струм ≤ 40».</summary>
    public string Describe(CustomRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var left = DisplayOf(rule.LeftCategory, rule.LeftParameter);
        var right = rule.ComparesParts ? DisplayOf(rule.RightCategory!, rule.RightParameter) : rule.Constant;
        return $"{left} {rule.Operator.Symbol()} {right}";
    }

    private ParameterDefinition? FindParameter(string? categoryKey, string? parameterKey, string field, List<RuleError> errors)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
        {
            errors.Add(new(field, "Оберіть категорію"));
            return null;
        }
        if (!_registry.TryGet(categoryKey, out var category))
        {
            errors.Add(new(field, $"Категорії «{categoryKey}» немає"));
            return null;
        }
        var parameter = Find(category, parameterKey);
        if (parameter is null)
            errors.Add(new(field, string.IsNullOrWhiteSpace(parameterKey)
                ? $"Оберіть параметр категорії «{category.DisplayName}»"
                : $"У категорії «{category.DisplayName}» немає параметра «{parameterKey}»"));
        return parameter;
    }

    private static ParameterDefinition? Find(ComponentCategory category, string? key) =>
        category.Parameters.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<IComponent> PartsOf(IEnumerable<IComponent> build, string categoryKey) =>
        build.Where(p => string.Equals(p.CategoryKey, categoryKey, StringComparison.OrdinalIgnoreCase))
             .DistinctBy(p => p.Id, StringComparer.OrdinalIgnoreCase);

    private static object? ValueOf(IComponent part, string key) =>
        part.Parameters.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

    private static string Describe(ParameterDefinition parameter, object? value) =>
        $"{parameter.DisplayName.ToLowerInvariant()} {RuleValue.Format(value)}{Unit(parameter)}";

    private static string Unit(ParameterDefinition parameter) =>
        string.IsNullOrEmpty(parameter.Unit) ? "" : $" {parameter.Unit}";

    private string DisplayOf(string categoryKey, string? parameterKey)
    {
        if (!_registry.TryGet(categoryKey, out var category))
            return $"{categoryKey}.{parameterKey}";
        var parameter = Find(category, parameterKey);
        return $"{category.DisplayName} · {parameter?.DisplayName ?? parameterKey}";
    }

    private static string ConstantHint(ParameterDefinition parameter, string text) => parameter.Type switch
    {
        ParameterType.Integer or ParameterType.Number => $"«{text}» не є числом",
        ParameterType.Boolean => "Вкажіть «так» або «ні»",
        ParameterType.Choice => $"Значення має бути одним із: {string.Join(", ", parameter.Options)}",
        _ => $"Неправильне значення «{text}»"
    };
}