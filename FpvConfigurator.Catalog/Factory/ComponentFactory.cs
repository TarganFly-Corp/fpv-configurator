using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>
/// Створює комплектуючі за ключем категорії. Для категорії можна зареєструвати власний «конструктор»
/// (так підключено стандартні класи). Якщо його немає, створюється CustomComponent.
/// </summary>
public sealed class ComponentFactory : IComponentFactory
{
    public delegate IComponent Creator(ComponentInfo info, IReadOnlyDictionary<string, object?> p);

    private readonly ICategoryRegistry _registry;
    private readonly Dictionary<string, Creator> _creators = new(StringComparer.OrdinalIgnoreCase);

    public ComponentFactory(ICategoryRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>Фабрика, яка вміє створювати всі вісім стандартних класів.</summary>
    public static ComponentFactory WithStandardCreators(ICategoryRegistry registry)
    {
        var f = new ComponentFactory(registry);
        f.RegisterCreator(StandardCategories.Frame.Key, (i, p) => new Frame(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Dbl(p, "maxPropInches"), Str(p, "stackMount"), Int(p, "wheelbaseMm"), i.InStock));
        f.RegisterCreator(StandardCategories.Motor.Key, (i, p) => new Motor(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Str(p, "statorSize"), Int(p, "kv"), Int(p, "minCells"), Int(p, "maxCells"), Dbl(p, "maxCurrentA"), i.InStock));
        f.RegisterCreator(StandardCategories.Propeller.Key, (i, p) => new Propeller(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Dbl(p, "diameterInches"), Dbl(p, "pitch"), Int(p, "blades"), i.InStock));
        f.RegisterCreator(StandardCategories.Battery.Key, (i, p) => new Battery(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Int(p, "cells"), Int(p, "capacityMah"), Int(p, "cRating"), Str(p, "connector"), i.InStock));
        f.RegisterCreator(StandardCategories.FlightController.Key, (i, p) => new FlightController(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Str(p, "mcu"), Str(p, "stackMount"), Int(p, "minCells"), Int(p, "maxCells"), Str(p, "firmware"), Int(p, "uartCount"), i.InStock));
        f.RegisterCreator(StandardCategories.Esc.Key, (i, p) => new Esc(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Int(p, "continuousCurrentA"), Int(p, "minCells"), Int(p, "maxCells"), (bool)p["isFourInOne"]!,
            p.TryGetValue("stackMount", out var m) ? m as string ?? "" : "", Str(p, "protocol"), i.InStock));
        f.RegisterCreator(StandardCategories.Camera.Key, (i, p) => new Camera(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Str(p, "videoSystem"), Int(p, "widthMm"), Str(p, "resolution"), Int(p, "fovDegrees"), i.InStock));
        f.RegisterCreator(StandardCategories.Vtx.Key, (i, p) => new Vtx(i.Id, i.Name, i.Manufacturer, i.Price, i.WeightGrams,
            Str(p, "videoSystem"), Int(p, "maxPowerMw"), Dbl(p, "frequencyGhz"), Str(p, "mount"), i.InStock));
        return f;
    }

    /// <remarks>
    /// Тестові дані:
    /// 1. Категорія, яку реєстр знає: «конструктор» використовується в Create.
    /// 2. Категорія, якої реєстр не знає (Get кидає KeyNotFoundException), і null замість «конструктора».
    /// </remarks>
    public void RegisterCreator(string categoryKey, Creator creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        _registry.Get(categoryKey); // категорія має існувати
        _creators[categoryKey] = creator;
    }

    /// <remarks>
    /// Тестові дані:
    /// 1. Стандартна категорія з коректними параметрами: об'єкт свого класу (Frame, Motor тощо).
    /// 2. Категорія без «конструктора»: CustomComponent.
    /// 3. Значення на межах схеми і за ними (для Integer min…max: min, max, min − 1, max + 1): за межами ArgumentException з назвою категорії.
    /// 4. Невідома категорія: виняток реєстру KeyNotFoundException не перехоплюється.
    /// 5. Макет ICategoryRegistry: Get(ключ) повертає категорію або кидає виняток.
    /// </remarks>
    public IComponent Create(string categoryKey, ComponentInfo info, IReadOnlyDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(info);
        ArgumentNullException.ThrowIfNull(parameters);

        var category = _registry.Get(categoryKey);
        var errors = category.Validate(parameters);
        if (errors.Count > 0)
            throw new ArgumentException(
                $"Некоректні параметри для категорії «{category.DisplayName}»: {string.Join("; ", errors)}",
                nameof(parameters));

        return _creators.TryGetValue(category.Key, out var creator)
            ? creator(info, parameters)
            : new CustomComponent(info, category, parameters);
    }

    private static string Str(IReadOnlyDictionary<string, object?> p, string key) => (string)p[key]!;
    private static int Int(IReadOnlyDictionary<string, object?> p, string key) => Convert.ToInt32(p[key]);
    private static double Dbl(IReadOnlyDictionary<string, object?> p, string key) => Convert.ToDouble(p[key]);
}