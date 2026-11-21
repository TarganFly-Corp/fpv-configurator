using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Tests.Mocks.Wrappers;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>JsonCatalogLoader з макетами реєстру, фабрики і каталогу (TC-66…TC-69).</summary>
public class JsonCatalogLoaderMockTests
{
    private const string ReceiverCategory = """
        { "categories": [ { "key": "Receiver", "displayName": "Приймач", "parameters": [
            { "key": "antennas", "displayName": "Антен", "type": "Integer", "min": 1, "max": 4 } ] } ] }
        """;

    /// <summary>TC-66: кожна деталь файлу створюється фабрикою і додається в каталог рівно один раз.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Load_ValidParts_EachAddedToCatalogOnce(int count)
    {
        var sut = new LoaderUnderTest();

        var parts = sut.Loader.Load(LoaderUnderTest.Frames(count));

        Assert.Equal(count, parts.Count);
        sut.Factory.Verify(f => f.Create("Frame", It.IsAny<ComponentInfo>(), It.IsAny<IReadOnlyDictionary<string, object?>>()), Times.Exactly(count));
        sut.VerifyAdded(Times.Exactly(count));
        for (var i = 1; i <= count; i++)
            sut.Catalog.Verify(c => c.Add(It.Is<IComponent>(p => p.Id == $"fr-{i}")), Times.Once());
        sut.Registry.Verify(r => r.Register(It.IsAny<ComponentCategory>()), Times.Never());
    }

    /// <summary>TC-67: фабрика відхиляє одну з деталей. Каталог не змінюється, у помилці Id деталі.</summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    [InlineData(3, 3)]
    public void Load_FactoryRejectsPart_ThrowsAndCatalogUntouched(int count, int badIndex)
    {
        var sut = new LoaderUnderTest().FactoryRejects($"fr-{badIndex}");

        var ex = Assert.Throws<InvalidDataException>(() => sut.Loader.Load(LoaderUnderTest.Frames(count)));

        Assert.Contains($"fr-{badIndex}", ex.Message);
        Assert.IsType<ArgumentException>(ex.InnerException);
        sut.VerifyAdded(Times.Never());
    }

    /// <summary>TC-68: деталь з таким Id уже є в каталозі.</summary>
    [Theory]
    [InlineData("fr-1")]
    [InlineData("fr-2")]
    [InlineData("fr-3")]
    public void Load_PartAlreadyInCatalog_ThrowsAndNothingAdded(string existingId)
    {
        var sut = new LoaderUnderTest().CatalogHas(existingId);

        var ex = Assert.Throws<InvalidDataException>(() => sut.Loader.Load(LoaderUnderTest.Frames(3)));

        Assert.Contains(existingId, ex.Message);
        sut.VerifyAdded(Times.Never());
    }

    /// <summary>TC-69: категорія з файлу реєструється в реєстрі, фабрика і каталог не потрібні.</summary>
    [Fact]
    public void Load_CategoryOnly_RegisteredOnce()
    {
        var registry = new Mock<ICategoryRegistry>();
        var factory = new Mock<IComponentFactory>(MockBehavior.Strict);
        var catalog = new Mock<IComponentCatalog>(MockBehavior.Strict);
        var loader = new JsonCatalogLoader(registry.Object, factory.Object, catalog.Object);

        var parts = loader.Load(ReceiverCategory);

        Assert.Empty(parts);
        registry.Verify(r => r.Register(It.Is<ComponentCategory>(c => c.Key == "Receiver" && c.Parameters.Count == 1)), Times.Once());
    }
}