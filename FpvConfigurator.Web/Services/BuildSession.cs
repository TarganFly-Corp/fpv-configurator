using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Storage;

namespace FpvConfigurator.Web.Services;

/// <summary>
/// Поточна збірка користувача (ФВ-19, ФВ-20). Один об'єкт на вкладку браузера (scoped у Blazor Server),
/// тому збірка зберігається під час переходів між сторінками.
/// Перевірку і збереження робить BuildService і BuildLibrary, тут лише стан сторінки.
/// </summary>
public sealed class BuildSession
{
    private readonly BuildService _service;
    private readonly BuildLibrary _library;

    public BuildSession(BuildService service, BuildLibrary library)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _library = library ?? throw new ArgumentNullException(nameof(library));
    }

    /// <summary>Подія після кожної зміни збірки (лічильник у меню, сторінка збірки).</summary>
    public event Action? Changed;

    public Build Current { get; private set; } = new();

    /// <summary>Назва, під якою збірку востаннє збережено або відкрито. null, якщо збірка нова.</summary>
    public string? SavedAs { get; private set; }

    /// <summary>Перевірка всіма правилами, ціна і вага (ФВ-12, ФВ-14).</summary>
    public BuildReport Report => _service.Check(Current);

    /// <summary>Додає деталь з каталогу. Якщо місця для категорії немає, InvalidOperationException.</summary>
    public IComponent Add(string partId)
    {
        var part = _service.AddPart(Current, partId);
        Changed?.Invoke();
        return part;
    }

    /// <summary>Додає деталь стільки разів, скільки ще є місця в категорії. Повертає кількість доданих.</summary>
    public int Fill(string partId, ComponentType type)
    {
        var free = (Build.LimitFor(type) ?? 1) - Current.CountOf(type);
        for (var i = 0; i < free; i++)
            _service.AddPart(Current, partId);
        Changed?.Invoke();
        return Math.Max(free, 0);
    }

    public bool Remove(string partId)
    {
        var removed = Current.Remove(partId);
        if (removed)
            Changed?.Invoke();
        return removed;
    }

    /// <summary>Нова порожня збірка.</summary>
    public void New(string name = "Нова збірка")
    {
        Current = new Build(name);
        SavedAs = null;
        Changed?.Invoke();
    }

    /// <summary>Змінює назву поточної збірки. Деталі лишаються ті самі.</summary>
    public void Rename(string name)
    {
        var renamed = new Build(name.Trim());
        foreach (var part in Current.Parts)
            renamed.Add(part);
        Current = renamed;
        Changed?.Invoke();
    }

    /// <summary>Відкриває збережену збірку. Помилки BuildLibrary.Open не перехоплює.</summary>
    public void Open(string name)
    {
        Current = _library.Open(name);
        SavedAs = Current.Name;
        Changed?.Invoke();
    }

    /// <summary>Зберігає збірку під її назвою. Зайнята назва без overwrite: InvalidOperationException.</summary>
    public void Save(bool overwrite = false)
    {
        _library.Save(Current, overwrite);
        SavedAs = Current.Name;
        Changed?.Invoke();
    }
}