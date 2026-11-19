namespace FpvConfigurator.Catalog;

/// <summary>
/// Запит до каталогу. Порожні поля не обмежують результат.
/// </summary>
/// <param name="CategoryKey">Ключ категорії, наприклад Frame або Receiver.</param>
/// <param name="Text">Рядок для пошуку в назві і виробнику.</param>
/// <param name="InStockOnly">Лише деталі, які є в наявності.</param>
/// <param name="Sort">Порядок сортування.</param>
public sealed record CatalogQuery(
    string? CategoryKey = null,
    string? Text = null,
    bool InStockOnly = false,
    CatalogSort Sort = CatalogSort.None);