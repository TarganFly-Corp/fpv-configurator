using FpvConfigurator.Core;
using FpvConfigurator.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Tests.Web;

/// <summary>UserCatalog веб-застосунку (ФВ-21…ФВ-23): справжні сервіси і файли в тимчасовій папці (TC-91…TC-94).</summary>
public sealed class UserCatalogTests : IDisposable
{
    private readonly WebTestData _data = new();

    public void Dispose() => _data.Dispose();

    private static ComponentCategory Gimbal() => new("Gimbal", "Підвіс камери",
        [new ParameterDefinition("axes", "Осей", ParameterType.Integer, min: 1, max: 3)]);

    /// <summary>TC-91: стартові файли завантажено без помилок, деталей користувача немає.</summary>
    [Fact]
    public void Initialize_SeedFiles_LoadedWithoutErrors()
    {
        var catalog = _data.StartApp(out _);

        Assert.Empty(catalog.StartupErrors);
        Assert.Equal(27, catalog.Catalog.Count);
        Assert.Equal(10, catalog.Registry.All.Count);
        Assert.Equal(0, catalog.UserPartCount);
    }

    /// <summary>TC-92: категорія і деталь, додані через інтерфейс, є і після «перезапуску».</summary>
    [Fact]
    public void AddCategoryAndPart_SurviveRestart()
    {
        var first = _data.StartApp(out var provider);
        first.AddCategory(Gimbal());
        first.AddPart(provider.GetRequiredService<IComponentFactory>(), "Gimbal",
            new ComponentInfo("gb-1", "Mini Gimbal", "Caddx", 1500m, 20),
            new Dictionary<string, object?> { ["axes"] = 2 });

        var second = _data.StartApp(out _);

        Assert.Empty(second.StartupErrors);
        Assert.True(second.IsUserCategory("gimbal"));
        Assert.True(second.IsUserPart("GB-1"));
        Assert.Equal("2", second.Catalog.FindById("gb-1")!.GetSpecs()["Осей"]);
    }

    /// <summary>TC-93: файл з помилкою в останній деталі не змінює каталог і реєстр (пробне завантаження).</summary>
    [Fact]
    public void Import_InvalidFile_CatalogUnchanged()
    {
        var catalog = _data.StartApp(out _);
        var json = """
            { "categories": [ { "key": "Antenna", "displayName": "Антена", "parameters": [ { "key": "gain", "displayName": "Підсилення", "type": "Number" } ] } ],
              "components": [
                { "category": "Antenna", "id": "ant-1", "name": "Lollipop", "manufacturer": "Foxeer", "price": 300, "weightGrams": 4, "parameters": { "gain": 2.5 } },
                { "category": "Antenna", "id": "ant-2", "name": "Bad", "manufacturer": "X", "price": 1, "weightGrams": 1, "parameters": { "gain": "багато" } } ] }
            """;

        Assert.Throws<InvalidDataException>(() => catalog.Import(json));

        Assert.False(catalog.Registry.TryGet("Antenna", out _));
        Assert.Null(catalog.Catalog.FindById("ant-1"));
        Assert.False(File.Exists(_data.UserFile));
    }

    /// <summary>TC-94: видалити можна лише деталь користувача, деталь зі стартового файлу ні.</summary>
    [Fact]
    public void RemovePart_OnlyUserParts()
    {
        var catalog = _data.StartApp(out var provider);
        catalog.AddPart(provider.GetRequiredService<IComponentFactory>(), "Frame",
            new ComponentInfo("fr-9", "Test 5\"", "Test", 1000m, 100),
            new Dictionary<string, object?> { ["maxPropInches"] = 5.0, ["stackMount"] = "30.5x30.5", ["wheelbaseMm"] = 220 });

        catalog.RemovePart("FR-9");

        Assert.Null(catalog.Catalog.FindById("fr-9"));
        Assert.Throws<InvalidOperationException>(() => catalog.RemovePart("fr-1"));
        Assert.NotNull(catalog.Catalog.FindById("fr-1"));
    }
}