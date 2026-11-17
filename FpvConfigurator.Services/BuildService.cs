using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services.Rules;

namespace FpvConfigurator.Services;

/// <summary>
/// Сервіс збірки (ФВ-11…ФВ-14): додає у збірку деталі з каталогу і перевіряє її набором правил.
/// Каталог і правила отримує через конструктор (Constructor Injection).
/// </summary>
public sealed class BuildService
{
    private readonly IComponentCatalog _catalog;
    private readonly IReadOnlyList<ICompatibilityRule> _rules;

    public BuildService(IComponentCatalog catalog, IEnumerable<ICompatibilityRule> rules)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.ToList();
    }

    /// <summary>Сервіс зі стандартними правилами, щоб не передавати їх щоразу.</summary>
    public BuildService(IComponentCatalog catalog)
        : this(catalog, [new StackMountRule(), new PropellerSizeRule(), new CellCountRule(), new VideoSystemRule()])
    {
    }

    /// <summary>Правила, якими сервіс перевіряє збірку.</summary>
    public IReadOnlyList<ICompatibilityRule> Rules => _rules;

    /// <summary>Знаходить деталь у каталозі за Id і додає її у збірку.</summary>
    public IComponent AddPart(Build build, string partId)
    {
        ArgumentNullException.ThrowIfNull(build);
        var part = _catalog.FindById(partId)
                   ?? throw new KeyNotFoundException($"Деталі «{partId}» немає в каталозі");
        build.Add(part);
        return part;
    }

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