using FpvConfigurator.Core;

namespace FpvConfigurator.Services;

/// <summary>
/// Збірка дрона (ФВ-11, ФВ-14): обрані деталі з обмеженням кількості для стандартних категорій.
/// Незалежний клас: працює лише з деталями, які йому передали.
/// </summary>
public sealed class Build
{
    private static readonly Dictionary<ComponentType, int> Limits = new()
    {
        [ComponentType.Frame] = 1,
        [ComponentType.FlightController] = 1,
        [ComponentType.Esc] = 1,
        [ComponentType.Motor] = 4,
        [ComponentType.Propeller] = 4,
        [ComponentType.Camera] = 1,
        [ComponentType.Vtx] = 1,
        [ComponentType.Battery] = 1
    };

    private readonly List<IComponent> _parts = [];

    public Build(string name = "Нова збірка")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва збірки не може бути порожньою", nameof(name));
        Name = name;
    }

    public string Name { get; }

    /// <summary>Деталі в порядку додавання.</summary>
    public IReadOnlyList<IComponent> Parts => _parts;

    public decimal TotalPrice => _parts.Sum(p => p.Price);

    public double TotalWeightGrams => _parts.Sum(p => p.WeightGrams);

    /// <summary>Скільки деталей цього типу може бути у збірці. null означає «без обмеження».</summary>
    public static int? LimitFor(ComponentType type) => Limits.TryGetValue(type, out var limit) ? limit : null;

    public int CountOf(ComponentType type) => _parts.Count(p => p.Type == type);

    /// <summary>Додає деталь. Якщо для її категорії місця вже немає, кидає InvalidOperationException.</summary>
    public void Add(IComponent part)
    {
        ArgumentNullException.ThrowIfNull(part);
        var limit = LimitFor(part.Type);
        if (limit is not null && CountOf(part.Type) >= limit)
            throw new InvalidOperationException(
                $"У збірці вже є {limit} шт. категорії {part.CategoryKey}, деталь {part.Id} не додано");
        _parts.Add(part);
    }

    /// <summary>Видаляє одну деталь з таким Id (регістр не важливий). Повертає false, якщо такої немає.</summary>
    public bool Remove(string partId)
    {
        var index = _parts.FindIndex(p => string.Equals(p.Id, partId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;
        _parts.RemoveAt(index);
        return true;
    }
}