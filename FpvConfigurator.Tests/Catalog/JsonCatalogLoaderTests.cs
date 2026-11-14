using FpvConfigurator.Catalog;
using FpvConfigurator.Components;

namespace FpvConfigurator.Tests.Catalog;

public class JsonCatalogLoaderTests
{
    private static JsonCatalogLoader CreateLoader()
    {
        var registry = CategoryRegistry.WithStandardCategories();
        return new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry));
    }

    private const string Receiver = """
        {
          "categories": [ { "key": "Receiver", "displayName": "Приймач", "parameters": [
              { "key": "protocol", "displayName": "Протокол", "type": "Choice", "options": [ "ELRS", "Crossfire" ] },
              { "key": "antennas", "displayName": "Антен", "type": "Integer", "min": 1, "max": 4 } ] } ],
          "components": [ COMPONENTS ]
        }
        """;

    private const string Rx1 = """
        { "category": "Receiver", "id": "rx-1", "name": "EP2", "manufacturer": "HappyModel",
          "price": 450, "weightGrams": 0.5, "parameters": { "protocol": "ELRS", "antennas": 1 } }
        """;

    [Fact]
    public void Load_CustomCategoryAndPart()
    {
        var parts = CreateLoader().Load(Receiver.Replace("COMPONENTS", Rx1));

        var rx = Assert.IsType<CustomComponent>(Assert.Single(parts));
        Assert.Equal("Receiver", rx.CategoryKey);
        Assert.Equal(1, rx.Parameters["antennas"]);
    }

    [Fact]
    public void Load_ValueOutOfRange_ThrowsWithPartId()
    {
        var json = Receiver.Replace("COMPONENTS", Rx1.Replace("\"antennas\": 1", "\"antennas\": 9"));

        var ex = Assert.Throws<InvalidDataException>(() => CreateLoader().Load(json));
        Assert.Contains("rx-1", ex.Message);
    }

    [Fact]
    public void Load_DuplicateComponentId_Throws()
    {
        var json = Receiver.Replace("COMPONENTS", Rx1 + "," + Rx1);

        Assert.Throws<InvalidDataException>(() => CreateLoader().Load(json));
    }

    [Fact]
    public void LoadFile_SampleCatalog_LoadsAllParts()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "custom-parts.json");

        var parts = CreateLoader().LoadFile(path);

        Assert.Equal(3, parts.Count);
        Assert.IsType<Frame>(parts.Single(p => p.Id == "fr-2"));
    }
}