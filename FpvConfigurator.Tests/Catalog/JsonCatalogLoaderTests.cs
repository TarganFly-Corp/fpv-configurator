using FpvConfigurator.Catalog;
using FpvConfigurator.Components;

namespace FpvConfigurator.Tests.Catalog;

public class JsonCatalogLoaderTests
{
    private static JsonCatalogLoader CreateLoader() => CreateLoader(new ComponentCatalog());

    private static JsonCatalogLoader CreateLoader(IComponentCatalog catalog)
    {
        var registry = CategoryRegistry.WithStandardCategories();
        return new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog);
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
    public void Load_DuplicateComponentIdInOtherCase_Throws()
    {
        // Дефект #29: rx-1 і RX-1 вважаються різними деталями
        var json = Receiver.Replace("COMPONENTS", Rx1 + "," + Rx1.Replace("\"rx-1\"", "\"RX-1\""));

        Assert.Throws<InvalidDataException>(() => CreateLoader().Load(json));
    }

    [Fact]
    public void Load_UnknownCategory_ThrowsWithPartId()
    {
        // Дефект #30: замість InvalidDataException з Id деталі летить KeyNotFoundException
        var json = Receiver.Replace("COMPONENTS", Rx1.Replace("\"Receiver\"", "\"Gimbal\""));

        var ex = Assert.Throws<InvalidDataException>(() => CreateLoader().Load(json));
        Assert.Contains("rx-1", ex.Message);
        Assert.Contains("Gimbal", ex.Message);
    }


    [Fact]
    public void LoadFile_SampleCatalog_LoadsAllParts()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "custom-parts.json");

        var parts = CreateLoader().LoadFile(path);

        Assert.Equal(3, parts.Count);
        Assert.IsType<Frame>(parts.Single(p => p.Id == "fr-2"));
    }

    [Fact]
    public void Load_AddsPartsToInjectedCatalog()
    {
        var catalog = new ComponentCatalog();

        CreateLoader(catalog).Load(Receiver.Replace("COMPONENTS", Rx1));

        Assert.Equal(1, catalog.Count);
        Assert.IsType<CustomComponent>(catalog.FindById("RX-1"));
    }

    [Fact]
    public void Load_InvalidPart_CatalogStaysUnchanged()
    {
        var catalog = new ComponentCatalog();
        var bad = Rx1.Replace("\"rx-1\"", "\"rx-2\"").Replace("\"antennas\": 1", "\"antennas\": 9");
        var json = Receiver.Replace("COMPONENTS", Rx1 + "," + bad);

        Assert.Throws<InvalidDataException>(() => CreateLoader(catalog).Load(json));
        Assert.Equal(0, catalog.Count);
    }
}