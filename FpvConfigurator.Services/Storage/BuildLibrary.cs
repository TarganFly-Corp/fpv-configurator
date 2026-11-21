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

    /// <remarks>
    /// Тестові дані:
    /// 1. Збірки з 0, 2 і 14 деталями під вільною назвою: у сховище йдуть назва і Id у порядку додавання.
    /// 2. Зайнята назва з overwrite = false: InvalidOperationException, Save не викликано.
    /// 3. Зайнята назва з overwrite = true: Save один раз, Exists не викликається.
    /// 4. Макет IBuildStorage: Exists, Save. Verify для Save з It.Is на назву і список Id.
    /// </remarks>
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

    /// <remarks>
    /// Тестові дані:
    /// 1. Збережені збірки з 0, 1 і 3 деталями, деталі є в каталозі: та сама назва і деталі в тому самому порядку.
    /// 2. Назва, якої немає в сховищі (Load повертає null): KeyNotFoundException.
    /// 3. Збірка, у якій є Id деталі, видаленої з каталогу (FindById повертає null): користувач має дізнатися, якої деталі бракує.
    /// 4. Макети: IBuildStorage (Load), IComponentCatalog (FindById).
    /// </remarks>
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

    /// <remarks>
    /// Тестові дані:
    /// 1. Назви в різному регістрі і порядку ([b, A, c]) і порожній список.
    /// 2. Макет IBuildStorage: ListNames.
    /// </remarks>
    /// <summary>Назви збережених збірок за абеткою.</summary>
    public IReadOnlyList<string> Names() =>
        _storage.ListNames().Order(StringComparer.CurrentCultureIgnoreCase).ToList();
}