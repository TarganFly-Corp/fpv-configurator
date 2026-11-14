namespace FpvConfigurator.Core;

/// <summary>Правило сумісності комплектуючих у збірці. Реалізації з'являться в ЛР4.</summary>
public interface ICompatibilityRule
{
    /// <summary>Коротка назва правила для звіту.</summary>
    string Name { get; }

    /// <summary>Повертає список проблем. Порожній список означає, що правило виконано.</summary>
    IEnumerable<string> Check(IReadOnlyCollection<IComponent> build);
}