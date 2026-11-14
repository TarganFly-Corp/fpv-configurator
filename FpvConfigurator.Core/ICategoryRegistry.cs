using System.Diagnostics.CodeAnalysis;

namespace FpvConfigurator.Core;

/// <summary>Реєстр категорій комплектуючих: стандартних і користувацьких.</summary>
public interface ICategoryRegistry
{
    /// <summary>Усі зареєстровані категорії.</summary>
    IReadOnlyCollection<ComponentCategory> All { get; }

    /// <summary>Додає категорію. Кидає виняток, якщо такий ключ уже є.</summary>
    void Register(ComponentCategory category);

    bool TryGet(string key, [NotNullWhen(true)] out ComponentCategory? category);

    /// <summary>Повертає категорію за ключем або кидає KeyNotFoundException.</summary>
    ComponentCategory Get(string key);
}