using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>
/// Каталог деталей: зберігання, пошук за Id і запити з фільтрами (ФВ-25…ФВ-27).
/// Сервіси залежать від цього інтерфейсу, а не від класу ComponentCatalog.
/// </summary>
public interface IComponentCatalog
{
    /// <summary>Усі деталі в порядку додавання.</summary>
    IReadOnlyList<IComponent> All { get; }

    int Count { get; }

    /// <summary>Додає деталь. Повторний Id (без урахування регістру) є помилкою.</summary>
    void Add(IComponent component);

    bool Contains(string id);

    /// <summary>Повертає деталь за Id або null, якщо такої немає.</summary>
    IComponent? FindById(string id);

    /// <summary>Фільтрує і сортує деталі за запитом.</summary>
    IReadOnlyList<IComponent> Query(CatalogQuery query);
}