using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using Moq;

namespace FpvConfigurator.Tests.Mocks.Wrappers;

/// <summary>
/// Обгортка для тестів JsonCatalogLoader: завантажувач разом з макетами трьох його залежностей.
/// За замовчуванням реєстр знає категорію Frame, фабрика створює деталь з Id з файлу, каталог порожній.
/// </summary>
internal sealed class LoaderUnderTest
{
    public LoaderUnderTest()
    {
        var frame = StandardCategories.Frame;
        Registry.Setup(r => r.TryGet("Frame", out frame)).Returns(true);
        Factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Returns((string _, ComponentInfo info, IReadOnlyDictionary<string, object?> _) => Mock.Of<IComponent>(c => c.Id == info.Id));
    }

    public Mock<ICategoryRegistry> Registry { get; } = new();
    public Mock<IComponentFactory> Factory { get; } = new();
    public Mock<IComponentCatalog> Catalog { get; } = new();

    public JsonCatalogLoader Loader => new(Registry.Object, Factory.Object, Catalog.Object);

    /// <summary>Фабрика кидає ArgumentException для деталі з цим Id.</summary>
    public LoaderUnderTest FactoryRejects(string partId)
    {
        Factory.Setup(f => f.Create(It.IsAny<string>(), It.Is<ComponentInfo>(i => i.Id == partId), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Throws(new ArgumentException("значення поза межами схеми"));
        return this;
    }

    /// <summary>У каталозі вже є деталь з цим Id.</summary>
    public LoaderUnderTest CatalogHas(string partId)
    {
        Catalog.Setup(c => c.Contains(partId)).Returns(true);
        return this;
    }

    /// <summary>JSON з рамами fr-1…fr-count. Значення параметрів не важливі: фабрика підмінена.</summary>
    public static string Frames(int count) =>
        "{ \"components\": [" + string.Join(",", Enumerable.Range(1, count).Select(i =>
            $"{{ \"category\": \"Frame\", \"id\": \"fr-{i}\", \"name\": \"Frame {i}\", \"manufacturer\": \"X\", " +
            $"\"price\": 1000, \"weightGrams\": 100, \"parameters\": {{ \"wheelbaseMm\": 225 }} }}")) + "] }";

    public void VerifyAdded(Times times) => Catalog.Verify(c => c.Add(It.IsAny<IComponent>()), times);
}