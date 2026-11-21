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
    [Theory(Skip = Stub.Reason)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Load_ValidParts_EachAddedToCatalogOnce(int count)
    {
        // TODO (розробник, #63): макети: реєстр знає категорію Frame, фабрика повертає деталь з Id з файлу, каталог порожній
        JsonCatalogLoader loader = null!;

        var parts = loader.Load(Frames(count));

        Assert.Equal(count, parts.Count);
    }

    /// <summary>TC-67: фабрика відхиляє одну з деталей. Каталог не змінюється, у помилці Id деталі.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(3, 3)]
    public void Load_FactoryRejectsPart_ThrowsAndCatalogUntouched(int count, int badIndex)
    {
        // TODO (розробник, #63): макети: фабрика кидає ArgumentException для деталі fr-badIndex
        JsonCatalogLoader loader = null!;

        var ex = Assert.Throws<InvalidDataException>(() => loader.Load(Frames(count)));

        Assert.Contains($"fr-{badIndex}", ex.Message);
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    /// <summary>TC-68: деталь з таким Id уже є в каталозі.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData("fr-1")]
    [InlineData("fr-3")]
    public void Load_PartAlreadyInCatalog_ThrowsAndNothingAdded(string existingId)
    {
        // TODO (розробник, #63): макети: каталог уже містить existingId
        JsonCatalogLoader loader = null!;

        var ex = Assert.Throws<InvalidDataException>(() => loader.Load(Frames(3)));

        Assert.Contains(existingId, ex.Message);
    }

    /// <summary>TC-69: категорія з файлу реєструється в реєстрі, фабрика і каталог не потрібні.</summary>
    [Fact(Skip = Stub.Reason)]
    public void Load_CategoryOnly_RegisteredOnce()
    {
        // TODO (розробник, #63): макети: фабрика і каталог Strict, бо файл без деталей не повинен їх чіпати
        JsonCatalogLoader loader = null!;

        var parts = loader.Load(ReceiverCategory);

        Assert.Empty(parts);
    }
}