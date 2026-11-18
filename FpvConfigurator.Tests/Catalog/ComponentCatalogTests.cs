using FpvConfigurator.Catalog;
using FpvConfigurator.Components;
using FpvConfigurator.Core;

namespace FpvConfigurator.Tests.Catalog;

public class ComponentCatalogTests
{
    private static readonly ComponentCategory Gps = new("Gps", "GPS-модуль",
    [
        new ParameterDefinition("chip", "Чип", ParameterType.Text)
    ]);

    private static ComponentCatalog CreateCatalog() => new(
    [
        new Frame("fr-1", "Source One V5", "TBS", 1200m, 110, 5.1, "30.5x30.5", 225),
        new Motor("mt-1", "2207 1950KV", "T-Motor", 900m, 33, "2207", 1950, 4, 6, 40),
        new Battery("bt-1", "1300mAh 6S", "CNHL", 1500m, 210, 6, 1300, 100, "XT60"),
        new CustomComponent(new ComponentInfo("gps-1", "M10 Mini", "HGLRC", 900m, 5.8, InStock: false),
                            Gps, new Dictionary<string, object?> { ["chip"] = "u-blox M10" })
    ]);

    [Fact]
    public void FindById_IgnoresCase()
    {
        var catalog = CreateCatalog();

        Assert.Equal("Source One V5", catalog.FindById("FR-1")?.Name);
        Assert.Null(catalog.FindById("fr-404"));
    }

    [Fact]
    public void Add_DuplicateIdInOtherCase_Throws()
    {
        var catalog = CreateCatalog();
        var copy = new Motor("MT-1", "2306 2450KV", "T-Motor", 950m, 32, "2306", 2450, 4, 6, 42);

        Assert.Throws<InvalidOperationException>(() => catalog.Add(copy));
        Assert.Equal(4, catalog.Count);
    }

    [Fact]
    public void Query_ByCategory_ReturnsOnlyThatCategory()
    {
        var result = CreateCatalog().Query(new CatalogQuery(CategoryKey: "motor"));

        Assert.Equal("mt-1", Assert.Single(result).Id);
    }

    [Fact]
    public void Query_InStockOnly_SkipsMissingParts()
    {
        var result = CreateCatalog().Query(new CatalogQuery(InStockOnly: true));

        Assert.Equal(["fr-1", "mt-1", "bt-1"], result.Select(p => p.Id));
    }

    [Fact]
    public void Query_TextSearch_IgnoresCase()
    {
        var result = CreateCatalog().Query(new CatalogQuery(Text: "tbs"));

        Assert.Equal("fr-1", Assert.Single(result).Id);
    }

    [Fact]
    public void Query_TextSearch_LooksInNameAndManufacturer()
    {
        var catalog = CreateCatalog();

        Assert.Equal("mt-1", Assert.Single(catalog.Query(new CatalogQuery(Text: "1950KV"))).Id);
        Assert.Equal("bt-1", Assert.Single(catalog.Query(new CatalogQuery(Text: "CNHL"))).Id);
    }

    [Fact]
    public void Query_SortByPriceDescending()
    {
        var result = CreateCatalog().Query(new CatalogQuery(Sort: CatalogSort.PriceDescending));

        Assert.Equal(["bt-1", "fr-1", "mt-1", "gps-1"], result.Select(p => p.Id));
    }

    [Fact]
    public void LoadFile_SampleCatalog_SearchesCustomParts()
    {
        var registry = CategoryRegistry.WithStandardCategories();
        var catalog = new ComponentCatalog();
        var loader = new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog);
        var path = Path.Combine(AppContext.BaseDirectory, "custom-parts.json");

        loader.LoadFile(path);

        Assert.Equal(3, catalog.Count);
        Assert.Equal("gps-1", Assert.Single(catalog.Query(new CatalogQuery(CategoryKey: "gps"))).Id);
        Assert.IsType<CustomComponent>(catalog.FindById("RX-1"));
    }
}