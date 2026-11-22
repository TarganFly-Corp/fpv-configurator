using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Tests.TestData;

namespace FpvConfigurator.Tests.Catalog;

/// <summary>JsonCatalogWriter (ФВ-23) і ComponentCatalog.Remove (TC-87…TC-90).</summary>
public class JsonCatalogWriterTests
{
    private static (CategoryRegistry Registry, ComponentCatalog Catalog, JsonCatalogLoader Loader) Fresh()
    {
        var registry = CategoryRegistry.WithStandardCategories();
        var catalog = new ComponentCatalog();
        return (registry, catalog, new JsonCatalogLoader(registry, ComponentFactory.WithStandardCreators(registry), catalog));
    }

    /// <summary>TC-87: експорт стартового каталогу і повторне завантаження дають ті самі деталі з тими самими характеристиками.</summary>
    [Fact]
    public void Write_ThenLoad_SameParts()
    {
        var source = Fresh();
        source.Loader.LoadFile(Path.Combine(AppContext.BaseDirectory, "standard-parts.json"));
        source.Loader.LoadFile(Path.Combine(AppContext.BaseDirectory, "custom-parts.json"));

        var json = JsonCatalogWriter.Write(source.Registry.All, source.Catalog.All);
        var target = Fresh();
        target.Loader.Load(json);

        Assert.Equal(source.Catalog.All.Select(p => p.Id), target.Catalog.All.Select(p => p.Id));
        foreach (var part in source.Catalog.All)
        {
            var copy = target.Catalog.FindById(part.Id)!;
            Assert.Equal(part.CategoryKey, copy.CategoryKey);
            Assert.Equal(part.GetType(), copy.GetType());
            Assert.Equal((part.Name, part.Manufacturer, part.Price, part.WeightGrams, part.InStock),
                         (copy.Name, copy.Manufacturer, copy.Price, copy.WeightGrams, copy.InStock));
            Assert.Equal(part.GetSpecs(), copy.GetSpecs());
        }
    }

    /// <summary>TC-88: стандартні категорії не записуються, користувацькі записуються з усіма полями схеми.</summary>
    [Fact]
    public void Write_OnlyCustomCategoriesWithSchema()
    {
        var source = Fresh();
        source.Loader.LoadFile(Path.Combine(AppContext.BaseDirectory, "custom-parts.json"));

        var json = JsonCatalogWriter.Write(source.Registry.All, []);
        var target = Fresh();
        target.Loader.Load(json);

        Assert.DoesNotContain("\"Frame\"", json);
        var receiver = target.Registry.Get("Receiver");
        var frequency = receiver.FindParameter("frequencyGhz")!;
        Assert.Equal((ParameterType.Number, "ГГц", 0.8, 2.5), (frequency.Type, frequency.Unit, frequency.Min, frequency.Max));
        Assert.Equal(["ELRS", "Crossfire", "Tracer", "FrSky"], receiver.FindParameter("protocol")!.Options);
        Assert.False(receiver.FindParameter("telemetry")!.Required);
    }

    /// <summary>TC-89: порожній набір дає файл, який завантажується без помилок. null: ArgumentNullException.</summary>
    [Fact]
    public void Write_EmptyAndNull()
    {
        var json = JsonCatalogWriter.Write([], []);

        Assert.Empty(Fresh().Loader.Load(json));
        Assert.Throws<ArgumentNullException>(() => JsonCatalogWriter.Write(null!, []));
        Assert.Throws<ArgumentNullException>(() => JsonCatalogWriter.Write([], null!));
    }

    /// <summary>TC-90: Remove видаляє деталь за Id без урахування регістру, повторно повертає false.</summary>
    [Theory]
    [InlineData("fr-1")]
    [InlineData("FR-1")]
    public void Remove_ById_IgnoresCase(string id)
    {
        var catalog = new ComponentCatalog([TestParts.Frame(), TestParts.Motor()]);

        Assert.True(catalog.Remove(id));
        Assert.False(catalog.Remove(id));
        Assert.False(catalog.Contains("fr-1"));
        Assert.Equal(["mt-1"], catalog.All.Select(p => p.Id));
    }
}