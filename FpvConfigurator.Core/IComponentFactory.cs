namespace FpvConfigurator.Core;

/// <summary>Створює комплектуючу будь-якої зареєстрованої категорії з набору параметрів.</summary>
public interface IComponentFactory
{
    IComponent Create(string categoryKey, ComponentInfo info,
                      IReadOnlyDictionary<string, object?> parameters);
}