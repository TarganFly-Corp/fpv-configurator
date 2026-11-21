using FpvConfigurator.Catalog;
using FpvConfigurator.Core;

namespace FpvConfigurator.Services;

/// <summary>
/// Сервіс збірки (ФВ-11…ФВ-14): додає у збірку деталі з каталогу і перевіряє її набором правил.
/// Каталог і правила отримує через конструктор (Constructor Injection).
/// </summary>
public sealed class BuildService
{
    private readonly IComponentCatalog _catalog;
    private readonly IReadOnlyList<ICompatibilityRule> _rules;
    /// <remarks>
    /// Тестові дані:
    /// 1. Макет IComponentCatalog і порожній список правил: об'єкт створюється.
    /// 2. null замість каталогу або списку правил: ArgumentNullException.
    /// </remarks>
    public BuildService(IComponentCatalog catalog, IEnumerable<ICompatibilityRule> rules)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.ToList();
    }

    /// <summary>Правила, якими сервіс перевіряє збірку.</summary>
    public IReadOnlyList<ICompatibilityRule> Rules => _rules;

    /// <remarks>
    /// Тестові дані:
    /// 1. Id, який каталог знаходить, у тому самому і в іншому регістрі (mt-1, MT-1): деталь у збірці.
    /// 2. Id, якого немає в каталозі, і порожній рядок: KeyNotFoundException з Id у тексті.
    /// 3. Деталь категорії, вже заповненої у збірці (п'ятий мотор): InvalidOperationException від Build.
    /// 4. Макет каталогу налаштовується на FindById, Verify: FindById викликано один раз.
    /// </remarks>
    /// <summary>Знаходить деталь у каталозі за Id і додає її у збірку.</summary>
    public IComponent AddPart(Build build, string partId)
    {
        ArgumentNullException.ThrowIfNull(build);
        var part = _catalog.FindById(partId)
                   ?? throw new KeyNotFoundException($"Деталі «{partId}» немає в каталозі");
        build.Add(part);
        return part;
    }

    /// <remarks>
    /// Тестові дані:
    /// 1. Різна кількість правил і проблем у кожному: 0 правил; правила без проблем; 1 проблема; кілька проблем у кількох правилах.
    /// 2. Збірка з відомою ціною і вагою (рама 1200 грн 110 г і батарея 1500 грн 210 г).
    /// 3. Макети ICompatibilityRule: Name і Check. Verify: кожне правило викликано один раз з деталями збірки, до каталогу звернень немає.
    /// </remarks>
    /// <summary>Перевіряє збірку всіма правилами і рахує ціну та вагу.</summary>
    public BuildReport Check(Build build)
    {
        ArgumentNullException.ThrowIfNull(build);
        var problems = _rules
            .SelectMany(rule => rule.Check(build.Parts).Select(message => new BuildProblem(rule.Name, message)))
            .ToList();
        return new BuildReport(problems, build.TotalPrice, build.TotalWeightGrams);
    }
}