namespace FpvConfigurator.Services.Storage;

/// <summary>
/// Збережена збірка (ФВ-15): назва і Id деталей у порядку додавання.
/// Самі деталі не зберігаються, при відкритті вони знову беруться з каталогу.
/// </summary>
public sealed record SavedBuild(string Name, IReadOnlyList<string> PartIds);