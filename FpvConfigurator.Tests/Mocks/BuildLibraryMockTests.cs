using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Storage;
using FpvConfigurator.Tests.Mocks.Wrappers;
using FpvConfigurator.Tests.TestData;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>BuildLibrary (ФВ-15) з макетами сховища і каталогу (TC-74…TC-78).</summary>
public class BuildLibraryMockTests
{
    /// <summary>Набори для TC-74: назва збірки і деталі в ній.</summary>
    public static TheoryData<string, IComponent[]> BuildsToSave => new()
    {
        { "Порожня", [] },
        { "Freestyle 5", [TestParts.Frame(), TestParts.Motor()] },
        { "Повна", TestParts.FullSet() }
    };

    /// <summary>TC-74: нова назва, у сховище йдуть назва і Id деталей у порядку додавання.</summary>
    [Theory]
    [MemberData(nameof(BuildsToSave))]
    public void Save_NewName_WritesNameAndPartIds(string name, IComponent[] parts)
    {
        var build = new Build(name);
        foreach (var part in parts)
            build.Add(part);
        var storage = new StorageMock(new SavedBuild("Інша", []));
        var library = new BuildLibrary(storage.Object, new CatalogMock().Object);

        library.Save(build);

        var written = Assert.Single(storage.Written);
        Assert.Equal(name, written.Name);
        Assert.Equal(parts.Select(p => p.Id), written.PartIds);
    }

    /// <summary>TC-75: назва зайнята (і в іншому регістрі). Без overwrite помилка і нічого не записано, з overwrite перезапис.</summary>
    [Theory]
    [InlineData("Freestyle 5", false, true)]
    [InlineData("FREESTYLE 5", false, true)]
    [InlineData("Freestyle 5", true, false)]
    public void Save_NameTaken_DependsOnOverwrite(string name, bool overwrite, bool expectThrow)
    {
        var storage = new StorageMock(new SavedBuild("Freestyle 5", ["fr-1"]));
        var library = new BuildLibrary(storage.Object, new CatalogMock().Object);

        var ex = Record.Exception(() => library.Save(new Build(name), overwrite));

        Assert.Equal(expectThrow, ex is InvalidOperationException);
        Assert.Equal(expectThrow ? 0 : 1, storage.Written.Count);
    }

    /// <summary>TC-76: збірка відкривається, деталі знову беруться з каталогу.</summary>
    [Theory]
    [InlineData("Freestyle 5", new[] { "fr-1", "mt-1", "mt-1" })]
    [InlineData("Лише рама", new[] { "fr-1" })]
    [InlineData("Порожня", new string[0])]
    public void Open_SavedBuild_PartsTakenFromCatalog(string name, string[] partIds)
    {
        var storage = new StorageMock(new SavedBuild(name, partIds));
        var catalog = new CatalogMock(TestParts.Frame(), TestParts.Motor());
        var library = new BuildLibrary(storage.Object, catalog.Object);

        var build = library.Open(name);

        Assert.Equal(name, build.Name);
        Assert.Equal(partIds, build.Parts.Select(p => p.Id));
        catalog.VerifyLookups(Times.Exactly(partIds.Length));
    }

    /// <summary>TC-77: збірки з такою назвою немає.</summary>
    [Fact]
    public void Open_UnknownName_ThrowsKeyNotFound()
    {
        var storage = new StorageMock(new SavedBuild("Freestyle 5", []));
        var catalog = new Mock<IComponentCatalog>(MockBehavior.Strict);
        var library = new BuildLibrary(storage.Object, catalog.Object);

        var ex = Assert.Throws<KeyNotFoundException>(() => library.Open("Немає такої"));

        Assert.Contains("Немає такої", ex.Message);
    }

    /// <summary>TC-78: назви збірок за абеткою без урахування регістру.</summary>
    [Theory]
    [InlineData(new[] { "b", "A", "c" }, new[] { "A", "b", "c" })]
    [InlineData(new[] { "Racer", "freestyle", "Cine" }, new[] { "Cine", "freestyle", "Racer" })]
    [InlineData(new string[0], new string[0])]
    public void Names_FromStorage_SortedAlphabetically(string[] stored, string[] expected)
    {
        var storage = new StorageMock(stored.Select(n => new SavedBuild(n, [])).ToArray());
        var library = new BuildLibrary(storage.Object, new CatalogMock().Object);

        Assert.Equal(expected, library.Names());
    }

}