using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>
/// Записує категорії і деталі в JSON у тому самому форматі, що й catalog/custom-parts.json (ФВ-23).
/// Результат знову читається JsonCatalogLoader.
/// </summary>
public static class JsonCatalogWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Повертає текст JSON з переданими категоріями і деталями. Стандартні категорії
    /// не записуються: вони описані в коді і є в кожному реєстрі.
    /// </summary>
    public static string Write(IEnumerable<ComponentCategory> categories, IEnumerable<IComponent> components)
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(components);

        var file = new
        {
            Categories = categories.Where(c => !c.IsStandard).Select(c => new
            {
                c.Key,
                c.DisplayName,
                Parameters = c.Parameters.Select(p => new
                {
                    p.Key,
                    p.DisplayName,
                    p.Type,
                    p.Unit,
                    Required = p.Required ? (bool?)null : false,
                    p.Min,
                    p.Max,
                    Options = p.Options.Count > 0 ? p.Options : null
                })
            }),
            Components = components.Select(c => new
            {
                Category = c.CategoryKey,
                c.Id,
                c.Name,
                c.Manufacturer,
                c.Price,
                c.WeightGrams,
                InStock = c.InStock ? (bool?)null : false,
                Parameters = c.Parameters.Where(kv => kv.Value is not null && kv.Value is not "")
                                         .ToDictionary(kv => kv.Key, kv => kv.Value)
            })
        };
        return JsonSerializer.Serialize(file, Options);
    }
}