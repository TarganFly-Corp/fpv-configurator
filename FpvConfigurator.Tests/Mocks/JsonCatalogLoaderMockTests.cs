using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>JsonCatalogLoader з макетами реєстру, фабрики і каталогу (TC-66…TC-69).</summary>
public class JsonCatalogLoaderMockTests
{
    /// <summary>JSON з рамами fr-1…fr-count. Значення параметрів не важливі: фабрика підмінена.</summary>
    private static string Frames(int count) =>
        "{ \"components\": [" + string.Join(",", Enumerable.Range(1, count).Select(i =>
            $"{{ \"category\": \"Frame\", \"id\": \"fr-{i}\", \"name\": \"Frame {i}\", \"manufacturer\": \"X\", " +
            $"\"price\": 1000, \"weightGrams\": 100, \"parameters\": {{ \"wheelbaseMm\": 225 }} }}")) + "] }";

    private const string ReceiverCategory = """
        { "categories": [ { "key": "Receiver", "displayName": "Приймач", "parameters": [
            { "key": "antennas", "displayName": "Антен", "type": "Integer", "min": 1, "max": 4 } ] } ] }
        """;

    /// <summary>TC-66: кожна деталь файлу створюється фабрикою і додається в каталог рівно один раз.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Load_ValidParts_EachAddedToCatalogOnce(int count)
    {
        // Макети (розробник): реєстр знає категорію Frame, фабрика повертає деталь з Id з файлу, каталог порожній
        var frame = StandardCategories.Frame;
        var registry = new Mock<ICategoryRegistry>();
        registry.Setup(r => r.TryGet("Frame", out frame)).Returns(true);
        var factory = new Mock<IComponentFactory>();
        factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Returns((string _, ComponentInfo info, IReadOnlyDictionary<string, object?> _) => Mock.Of<IComponent>(c => c.Id == info.Id));
        var catalog = new Mock<IComponentCatalog>();
        var loader = new JsonCatalogLoader(registry.Object, factory.Object, catalog.Object);

        var parts = loader.Load(Frames(count));

        Assert.Equal(count, parts.Count);
        factory.Verify(f => f.Create("Frame", It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()), Times.Exactly(count));
        catalog.Verify(c => c.Add(It.IsAny<IComponent>()), Times.Exactly(count));
        catalog.Verify(c => c.Add(It.Is<IComponent>(p => p.Id == $"fr-{count}")), Times.Once());
        registry.Verify(r => r.Register(It.IsAny<ComponentCategory>()), Times.Never());
    }

    /// <summary>TC-67: фабрика відхиляє одну з деталей. Каталог не змінюється, у помилці Id деталі.</summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(3, 3)]
    public void Load_FactoryRejectsPart_ThrowsAndCatalogUntouched(int count, int badIndex)
    {
        // Макети (розробник): фабрика кидає ArgumentException для деталі fr-badIndex
        var frame = StandardCategories.Frame;
        var registry = new Mock<ICategoryRegistry>();
        registry.Setup(r => r.TryGet("Frame", out frame)).Returns(true);
        var factory = new Mock<IComponentFactory>();
        factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Returns((string _, ComponentInfo info, IReadOnlyDictionary<string, object?> _) => Mock.Of<IComponent>(c => c.Id == info.Id));
        factory.Setup(f => f.Create(It.IsAny<string>(), It.Is<ComponentInfo>(i => i.Id == $"fr-{badIndex}"), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Throws(new ArgumentException("колісна база поза межами"));
        var catalog = new Mock<IComponentCatalog>();
        var loader = new JsonCatalogLoader(registry.Object, factory.Object, catalog.Object);

        var ex = Assert.Throws<InvalidDataException>(() => loader.Load(Frames(count)));

        Assert.Contains($"fr-{badIndex}", ex.Message);
        Assert.IsType<ArgumentException>(ex.InnerException);
        catalog.Verify(c => c.Add(It.IsAny<IComponent>()), Times.Never());
    }

    /// <summary>TC-68: деталь з таким Id уже є в каталозі.</summary>
    [Theory]
    [InlineData("fr-1")]
    [InlineData("fr-3")]
    public void Load_PartAlreadyInCatalog_ThrowsAndNothingAdded(string existingId)
    {
        // Макети (розробник): каталог уже містить existingId
        var frame = StandardCategories.Frame;
        var registry = new Mock<ICategoryRegistry>();
        registry.Setup(r => r.TryGet("Frame", out frame)).Returns(true);
        var factory = new Mock<IComponentFactory>();
        factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()))
               .Returns((string _, ComponentInfo info, IReadOnlyDictionary<string, object?> _) => Mock.Of<IComponent>(c => c.Id == info.Id));
        var catalog = new Mock<IComponentCatalog>();
        catalog.Setup(c => c.Contains(existingId)).Returns(true);
        var loader = new JsonCatalogLoader(registry.Object, factory.Object, catalog.Object);

        var ex = Assert.Throws<InvalidDataException>(() => loader.Load(Frames(3)));

        Assert.Contains(existingId, ex.Message);
        catalog.Verify(c => c.Add(It.IsAny<IComponent>()), Times.Never());
    }

    /// <summary>TC-69: категорія з файлу реєструється в реєстрі, фабрика і каталог не потрібні.</summary>
    [Fact]
    public void Load_CategoryOnly_RegisteredOnce()
    {
        // Макети (розробник): фабрика і каталог Strict, бо файл без деталей не повинен їх чіпати
        var registry = new Mock<ICategoryRegistry>();
        var factory = new Mock<IComponentFactory>(MockBehavior.Strict);
        var catalog = new Mock<IComponentCatalog>(MockBehavior.Strict);
        var loader = new JsonCatalogLoader(registry.Object, factory.Object, catalog.Object);

        var parts = loader.Load(ReceiverCategory);

        Assert.Empty(parts);
        registry.Verify(r => r.Register(It.Is<ComponentCategory>(c => c.Key == "Receiver" && c.Parameters.Count == 1)), Times.Once());
    }
}