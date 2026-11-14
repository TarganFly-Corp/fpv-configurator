using System.Text.Json;
using System.Text.Json.Serialization;
using FpvConfigurator.Core;

namespace FpvConfigurator.Catalog;

/// <summary>
/// Завантажує з JSON нові категорії і комплектуючі. Так користувач додає власну деталь без зміни коду.
/// </summary>
public sealed class JsonCatalogLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ICategoryRegistry _registry;
    private readonly IComponentFactory _factory;

    public JsonCatalogLoader(ICategoryRegistry registry, IComponentFactory factory)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IReadOnlyList<IComponent> LoadFile(string path) => Load(File.ReadAllText(path));

    public IReadOnlyList<IComponent> Load(string json)
    {
        var file = JsonSerializer.Deserialize<CatalogFile>(json, Options)
                   ?? throw new InvalidDataException("Порожній файл каталогу");

        foreach (var c in file.Categories)
        {
            var parameters = c.Parameters.Select(p => new ParameterDefinition(
                p.Key, p.DisplayName, p.Type, p.Unit, p.Required, p.Min, p.Max, p.Options));
            _registry.Register(new ComponentCategory(c.Key, c.DisplayName, parameters));
        }

        var result = new List<IComponent>();
        var ids = new HashSet<string>();
        foreach (var dto in file.Components)
        {
            if (!ids.Add(dto.Id))
                throw new InvalidDataException($"Деталь з Id «{dto.Id}» описано двічі");

            var category = _registry.Get(dto.Category);
            var values = dto.Parameters.ToDictionary(
                kv => kv.Key,
                kv => ToValue(kv.Value, category.FindParameter(kv.Key)));

            var info = new ComponentInfo(dto.Id, dto.Name, dto.Manufacturer, dto.Price, dto.WeightGrams, dto.InStock);
            try
            {
                result.Add(_factory.Create(dto.Category, info, values));
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"Деталь «{dto.Id}»: {ex.Message}", ex);
            }
        }
        return result;
    }

    private static object? ToValue(JsonElement e, ParameterDefinition? definition) => e.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number when definition?.Type == ParameterType.Integer && e.TryGetInt32(out var i) => i,
        JsonValueKind.Number => e.GetDouble(),
        _ => e.GetRawText()
    };

    private sealed class CatalogFile
    {
        public List<CategoryDto> Categories { get; set; } = [];
        public List<ComponentDto> Components { get; set; } = [];
    }

    private sealed class CategoryDto
    {
        public string Key { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public List<ParameterDto> Parameters { get; set; } = [];
    }

    private sealed class ParameterDto
    {
        public string Key { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public ParameterType Type { get; set; }
        public string? Unit { get; set; }
        public bool Required { get; set; } = true;
        public double? Min { get; set; }
        public double? Max { get; set; }
        public List<string>? Options { get; set; }
    }

    private sealed class ComponentDto
    {
        public string Category { get; set; } = "";
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public decimal Price { get; set; }
        public double WeightGrams { get; set; }
        public bool InStock { get; set; } = true;
        public Dictionary<string, JsonElement> Parameters { get; set; } = [];
    }
}