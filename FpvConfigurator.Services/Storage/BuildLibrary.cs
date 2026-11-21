using FpvConfigurator.Catalog;

namespace FpvConfigurator.Services.Storage;

/// <summary>
/// Бібліотека збірок користувача (ФВ-15): зберегти збірку, відкрити її знову, список назв.
/// Сховище і каталог отримує через конструктор (Constructor Injection).
/// </summary>
public sealed class BuildLibrary
{
    private readonly IBuildStorage _storage;
    private readonly IComponentCatalog _catalog;

    public BuildLibrary(IBuildStorage storage, IComponentCatalog catalog)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>
    /// Зберігає збірку під її назвою. Якщо назва зайнята і overwrite = false,
    /// кидає InvalidOperationException і нічого не записує.
    /// </summary>
    public void Save(Build build, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(build);
        if (!overwrite && _storage.Exists(build.Name))
            throw new InvalidOperationException($"Збірка «{build.Name}» вже є в сховищі");

        _storage.Save(new SavedBuild(build.Name, build.Parts.Select(p => p.Id).ToList()));
    }

    /// <summary>Відкриває збережену збірку: бере її з сховища і знову знаходить деталі в каталозі.</summary>
    public Build Open(string name)
    {
        var saved = _storage.Load(name)
                    ?? throw new KeyNotFoundException($"Збірки «{name}» немає в сховищі");

        var build = new Build(saved.Name);
        foreach (var id in saved.PartIds)
        {
            var part = _catalog.FindById(id)
           ?? throw new InvalidDataException(
               $"У збірці «{saved.Name}» є деталь «{id}», якої вже немає в каталозі");
            build.Add(part);
        }
        return build;
    }

    /// <summary>Назви збережених збірок за абеткою.</summary>
    public IReadOnlyList<string> Names() =>
        _storage.ListNames().Order(StringComparer.CurrentCultureIgnoreCase).ToList();
}