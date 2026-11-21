using FpvConfigurator.Catalog;
using FpvConfigurator.Core;
using FpvConfigurator.Services;
using FpvConfigurator.Services.Storage;
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
        // Макет сховища (розробник): назва вільна
        var storage = new Mock<IBuildStorage>();
        storage.Setup(s => s.Exists(name)).Returns(false);
        var library = new BuildLibrary(storage.Object, Mock.Of<IComponentCatalog>());

        library.Save(build);

        var ids = parts.Select(p => p.Id).ToList();
        storage.Verify(s => s.Save(It.Is<SavedBuild>(b => b.Name == name && b.PartIds.SequenceEqual(ids))), Times.Once());
    }

    /// <summary>TC-75: назва зайнята. Без overwrite помилка і нічого не записано, з overwrite перезапис.</summary>
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Save_NameTaken_DependsOnOverwrite(bool overwrite, bool expectThrow)
    {
        var build = new Build("Freestyle 5");
        // Макет сховища (розробник): назва вже зайнята
        var storage = new Mock<IBuildStorage>();
        storage.Setup(s => s.Exists("Freestyle 5")).Returns(true);
        var library = new BuildLibrary(storage.Object, Mock.Of<IComponentCatalog>());

        var ex = Record.Exception(() => library.Save(build, overwrite));

        Assert.Equal(expectThrow, ex is InvalidOperationException);
        storage.Verify(s => s.Save(It.IsAny<SavedBuild>()), expectThrow ? Times.Never() : Times.Once());
    }

    /// <summary>TC-76: збірка відкривається, деталі знову беруться з каталогу.</summary>
    [Theory]
    [InlineData("Freestyle 5", new[] { "fr-1", "mt-1", "mt-1" }, 3)]
    [InlineData("Лише рама", new[] { "fr-1" }, 1)]
    [InlineData("Порожня", new string[0], 0)]
    public void Open_SavedBuild_PartsTakenFromCatalog(string name, string[] partIds, int expectedParts)
    {
        // Макети (розробник): сховище повертає збірку, каталог знає fr-1 і mt-1
        var storage = new Mock<IBuildStorage>();
        storage.Setup(s => s.Load(name)).Returns(new SavedBuild(name, partIds));
        var catalog = new Mock<IComponentCatalog>();
        catalog.Setup(c => c.FindById("fr-1")).Returns(TestParts.Frame());
        catalog.Setup(c => c.FindById("mt-1")).Returns(TestParts.Motor());
        var library = new BuildLibrary(storage.Object, catalog.Object);

        var build = library.Open(name);

        Assert.Equal(name, build.Name);
        Assert.Equal(expectedParts, build.Parts.Count);
        Assert.Equal(partIds, build.Parts.Select(p => p.Id));
        catalog.Verify(c => c.FindById(It.IsAny<string>()), Times.Exactly(partIds.Length));
    }

    /// <summary>TC-77: збірки з такою назвою немає.</summary>
    [Fact]
    public void Open_UnknownName_ThrowsKeyNotFound()
    {
        // Макет сховища (розробник): Load повертає null
        var storage = new Mock<IBuildStorage>();
        storage.Setup(s => s.Load(It.IsAny<string>())).Returns((SavedBuild?)null);
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
        // Макет сховища (розробник)
        var storage = new Mock<IBuildStorage>();
        storage.Setup(s => s.ListNames()).Returns(stored);
        var library = new BuildLibrary(storage.Object, Mock.Of<IComponentCatalog>());

        Assert.Equal(expected, library.Names());
    }
}