using System.Text.Json;
using FpvConfigurator.Catalog;
using FpvConfigurator.Core;

namespace FpvConfigurator.Web.Services;

/// <summary>
/// Каталог веб-застосунку (ФВ-17, ФВ-21…ФВ-23). Завантажує стартові файли каталогу,
/// додає категорії і деталі користувача, імпортує і експортує JSON.
/// Категорії і деталі, додані через інтерфейс або імпортом, записуються у файл користувача
/// і завантажуються знову під час наступного запуску.
/// Сам нічого не перевіряє і не створює: працює через реєстр, фабрику, каталог і завантажувач з DI.
/// </summary>
public sealed class UserCatalog
{
    private readonly ICategoryRegistry _registry;
    private readonly IComponentCatalog _catalog;
    private readonly JsonCatalogLoader _loader;
    private readonly string _userFile;
    private readonly HashSet<string> _userCategories = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _userParts = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _startupErrors = [];
    private readonly Lock _lock = new();

    public UserCatalog(ICategoryRegistry registry, IComponentCatalog catalog, JsonCatalogLoader loader, string userFile)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        if (string.IsNullOrWhiteSpace(userFile))
            throw new ArgumentException("Шлях до файлу користувача не може бути порожнім", nameof(userFile));
        _userFile = Path.GetFullPath(userFile);
    }

    /// <summary>Подія після кожної зміни каталогу: сторінки оновлюють себе.</summary>
    public event Action? Changed;

    public ICategoryRegistry Registry => _registry;

    public IComponentCatalog Catalog => _catalog;

    /// <summary>Файл, у якому зберігаються категорії і деталі користувача.</summary>
    public string UserFile => _userFile;

    /// <summary>Помилки під час завантаження файлів на старті. Застосунок працює далі без цих файлів.</summary>
    public IReadOnlyList<string> StartupErrors => _startupErrors;

    public int UserPartCount => _userParts.Count;

    public bool IsUserPart(string id) => _userParts.Contains(id);

    public bool IsUserCategory(string key) => _userCategories.Contains(key);

    /// <summary>
    /// Завантажує стартові файли каталогу (вони не потрапляють у файл користувача),
    /// а потім файл користувача, якщо він уже є.
    /// </summary>
    public void Initialize(IEnumerable<string> seedFiles)
    {
        ArgumentNullException.ThrowIfNull(seedFiles);
        lock (_lock)
        {
            foreach (var file in seedFiles)
            {
                try
                {
                    _loader.LoadFile(file);
                }
                catch (Exception ex) when (IsCatalogError(ex) || ex is IOException)
                {
                    _startupErrors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }

            if (!File.Exists(_userFile))
                return;
            try
            {
                ImportCore(File.ReadAllText(_userFile));
            }
            catch (Exception ex) when (IsCatalogError(ex))
            {
                _startupErrors.Add($"{Path.GetFileName(_userFile)}: {ex.Message}");
            }
        }
    }

    /// <summary>Додає категорію користувача (ФВ-21). Повторний ключ: InvalidOperationException від реєстру.</summary>
    public void AddCategory(ComponentCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);
        lock (_lock)
        {
            _registry.Register(category);
            _userCategories.Add(category.Key);
            Persist();
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Створює деталь через фабрику і додає в каталог (ФВ-22). Повторний Id: InvalidOperationException,
    /// некоректні параметри: ArgumentException від фабрики або класу деталі.
    /// </summary>
    public IComponent AddPart(IComponentFactory factory, string categoryKey, ComponentInfo info,
                              IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(factory);
        IComponent part;
        lock (_lock)
        {
            if (_catalog.Contains(info.Id))
                throw new InvalidOperationException($"Деталь з Id «{info.Id}» вже є в каталозі");
            part = factory.Create(categoryKey, info, values);
            _catalog.Add(part);
            _userParts.Add(part.Id);
            Persist();
        }
        Changed?.Invoke();
        return part;
    }

    /// <summary>Видаляє деталь користувача. Деталі зі стартових файлів видаляти не можна.</summary>
    public void RemovePart(string id)
    {
        lock (_lock)
        {
            if (!_userParts.Contains(id))
                throw new InvalidOperationException($"Деталь «{id}» не додана користувачем, її не можна видалити");
            _catalog.Remove(id);
            _userParts.Remove(id);
            Persist();
        }
        Changed?.Invoke();
    }

    /// <summary>
    /// Імпортує категорії і деталі з JSON у форматі catalog/custom-parts.json (ФВ-23).
    /// Спершу файл пробно завантажується в копію каталогу: якщо в ньому є помилка,
    /// каталог застосунку не змінюється взагалі.
    /// </summary>
    public ImportResult Import(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ImportResult result;
        lock (_lock)
        {
            DryRun(json);
            result = ImportCore(json);
            Persist();
        }
        Changed?.Invoke();
        return result;
    }

    /// <summary>
    /// JSON для завантаження: увесь каталог (користувацькі категорії і всі деталі)
    /// або лише те, що додав користувач.
    /// </summary>
    public string Export(bool userOnly)
    {
        lock (_lock)
        {
            return userOnly
                ? JsonCatalogWriter.Write(UserCategoriesInOrder(), UserPartsInOrder())
                : JsonCatalogWriter.Write(_registry.All, _catalog.All);
        }
    }

    /// <summary>Чи є виняток помилкою даних каталогу, яку треба показати користувачу.</summary>
    public static bool IsCatalogError(Exception ex) =>
        ex is JsonException or InvalidDataException or InvalidOperationException or ArgumentException or KeyNotFoundException;

    private ImportResult ImportCore(string json)
    {
        var before = _registry.All.Select(c => c.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var parts = _loader.Load(json);
        var categories = _registry.All.Where(c => !before.Contains(c.Key)).ToList();

        foreach (var category in categories)
            _userCategories.Add(category.Key);
        foreach (var part in parts)
            _userParts.Add(part.Id);
        return new ImportResult(categories, parts);
    }

    private void DryRun(string json)
    {
        var registry = new CategoryRegistry();
        foreach (var category in _registry.All)
            registry.Register(category);
        var catalog = new ComponentCatalog(_catalog.All);
        var loader = new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog);
        loader.Load(json);
    }

    private IEnumerable<ComponentCategory> UserCategoriesInOrder() =>
        _registry.All.Where(c => _userCategories.Contains(c.Key));

    private IEnumerable<IComponent> UserPartsInOrder() =>
        _catalog.All.Where(p => _userParts.Contains(p.Id));

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_userFile)!);
        var temp = _userFile + ".tmp";
        File.WriteAllText(temp, JsonCatalogWriter.Write(UserCategoriesInOrder(), UserPartsInOrder()));
        File.Move(temp, _userFile, overwrite: true);
    }
}

/// <summary>Що додав імпорт: нові категорії і деталі.</summary>
public sealed record ImportResult(IReadOnlyList<ComponentCategory> Categories, IReadOnlyList<IComponent> Parts);