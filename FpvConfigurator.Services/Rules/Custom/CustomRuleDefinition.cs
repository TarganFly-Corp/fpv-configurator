using System.Text.Json.Serialization;

namespace FpvConfigurator.Services.Rules.Custom;

/// <summary>
/// Правило сумісності, яке користувач описав сам (ФВ-28):
/// «параметр деталі категорії зліва» операція «параметр деталі іншої категорії» або «значення».
/// Приклад: Esc.continuousCurrentA ≥ Motor.maxCurrentA.
/// Якщо RightCategory задано, порівнюються дві деталі. Інакше параметр порівнюється з Constant.
/// </summary>
public sealed record CustomRuleDefinition(
    string Name,
    string LeftCategory,
    string LeftParameter,
    RuleOperator Operator,
    string? RightCategory = null,
    string? RightParameter = null,
    string? Constant = null)
{
    /// <summary>Чи порівнює правило дві деталі (а не деталь зі значенням).</summary>
    [JsonIgnore]
    public bool ComparesParts => !string.IsNullOrWhiteSpace(RightCategory);

    public override string ToString() => ComparesParts
        ? $"{LeftCategory}.{LeftParameter} {Operator.Symbol()} {RightCategory}.{RightParameter}"
        : $"{LeftCategory}.{LeftParameter} {Operator.Symbol()} {Constant}";
}

/// <summary>Помилка в описі правила. Field показує, біля якого поля форми її вивести.</summary>
public sealed record RuleError(string Field, string Message);