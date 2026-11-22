using FpvConfigurator.Catalog;
using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Catalog;

/// <summary>Ознака наявності (ФВ-02) після створення деталі фабрикою (TC-99).</summary>
public class StockFlagTests
{
    /// <summary>Параметри стандартної деталі кожної категорії, з якими фабрика її створює.</summary>
    public static TheoryData<string> StandardIds => ["fr-4", "fc-3", "esc-3", "vtx-3"];

    /// <summary>TC-99: деталь з "inStock": false у файлі каталогу після завантаження теж не в наявності.</summary>
    [Theory]
    [MemberData(nameof(StandardIds))]
    public void Load_OutOfStockStandardPart_KeepsFlag(string id)
    {
        var registry = CategoryRegistry.WithStandardCategories();
        var catalog = new ComponentCatalog();
        new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog)
            .LoadFile(Path.Combine(AppContext.BaseDirectory, "standard-parts.json"));

        Assert.False(catalog.FindById(id)!.InStock);
    }

    /// <summary>TC-99: фабрика передає InStock з ComponentInfo в кожен стандартний клас.</summary>
    [Theory]
    [InlineData("Frame")]
    [InlineData("Motor")]
    [InlineData("Battery")]
    public void Create_StandardCategory_PassesInStock(string key)
    {
        var registry = CategoryRegistry.WithStandardCategories();
        var factory = ComponentFactory.WithStandardCreators(registry);
        var values = key switch
        {
            "Frame" => new Dictionary<string, object?> { ["maxPropInches"] = 5.0, ["stackMount"] = "20x20", ["wheelbaseMm"] = 200 },
            "Motor" => new Dictionary<string, object?> { ["statorSize"] = "2207", ["kv"] = 1950, ["minCells"] = 4, ["maxCells"] = 6, ["maxCurrentA"] = 40.0 },
            _ => new Dictionary<string, object?> { ["cells"] = 4, ["capacityMah"] = 1500, ["cRating"] = 100, ["connector"] = "XT60" }
        };

        var part = factory.Create(key, new ComponentInfo("x-1", "Тест", "Тест", 1m, 1, InStock: false), values);

        Assert.False(part.InStock);
    }
}