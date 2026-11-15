using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>
/// Каталог деталей у пам'яті: пошук за Id, фільтр за категорією і наявністю,
/// пошук у назві і виробнику, сортування за ціною і вагою (ФВ-25…ФВ-27).
/// </summary>
public sealed class ComponentCatalog
{
    private readonly Dictionary<string, IComponent> _byId = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IComponent> _items = [];

    public ComponentCatalog()
    {
    }

    public ComponentCatalog(IEnumerable<IComponent> components)
    {
        ArgumentNullException.ThrowIfNull(components);
        foreach (var component in components)
            Add(component);
    }

    /// <summary>Усі деталі в порядку додавання.</summary>
    public IReadOnlyList<IComponent> All => _items;

    public int Count => _items.Count;

    /// <summary>Додає деталь. Id нечутливі до регістру, повторний Id є помилкою (ФВ-25).</summary>
    public void Add(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (!_byId.TryAdd(component.Id, component))
            throw new InvalidOperationException($"Деталь з Id «{component.Id}» вже є в каталозі");
        _items.Add(component);
    }

    public bool Contains(string id) => _byId.ContainsKey(id);

    /// <summary>Повертає деталь за Id або null, якщо такої немає.</summary>
    public IComponent? FindById(string id) => _byId.GetValueOrDefault(id);

    /// <summary>Фільтрує і сортує деталі за запитом (ФВ-26, ФВ-27).</summary>
    public IReadOnlyList<IComponent> Query(CatalogQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        IEnumerable<IComponent> result = _items;

        if (!string.IsNullOrWhiteSpace(query.CategoryKey))
            result = result.Where(p => string.Equals(p.CategoryKey, query.CategoryKey, StringComparison.OrdinalIgnoreCase));

        if (query.InStockOnly)
            result = result.Where(p => p.InStock);

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var text = query.Text.Trim();
            result = result.Where(p => p.Name.Contains(text) || p.Manufacturer.Contains(text));
        }

        result = query.Sort switch
        {
            CatalogSort.PriceAscending => result.OrderBy(p => p.Price),
            CatalogSort.PriceDescending => result.OrderByDescending(p => p.Price),
            CatalogSort.WeightAscending => result.OrderBy(p => p.WeightGrams),
            _ => result
        };

        return result.ToList();
    }
}