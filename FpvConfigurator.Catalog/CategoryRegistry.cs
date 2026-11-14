using System.Diagnostics.CodeAnalysis;
using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>Реєстр категорій у пам'яті. Ключі нечутливі до регістру.</summary>
public sealed class CategoryRegistry : ICategoryRegistry
{
    private readonly Dictionary<string, ComponentCategory> _categories = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Реєстр, у якому вже є вісім стандартних категорій.</summary>
    public static CategoryRegistry WithStandardCategories()
    {
        var registry = new CategoryRegistry();
        foreach (var category in StandardCategories.All)
            registry.Register(category);
        return registry;
    }

    public IReadOnlyCollection<ComponentCategory> All => _categories.Values;

    public void Register(ComponentCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (!_categories.TryAdd(category.Key, category))
            throw new InvalidOperationException($"Категорія «{category.Key}» вже зареєстрована");
    }

    public bool TryGet(string key, [NotNullWhen(true)] out ComponentCategory? category) =>
        _categories.TryGetValue(key, out category);

    public ComponentCategory Get(string key) =>
        TryGet(key, out var category)
            ? category
            : throw new KeyNotFoundException($"Категорію «{key}» не знайдено");
}