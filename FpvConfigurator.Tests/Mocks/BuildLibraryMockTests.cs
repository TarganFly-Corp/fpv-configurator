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
    [Theory(Skip = Stub.Reason)]
    [MemberData(nameof(BuildsToSave))]
    public void Save_NewName_WritesNameAndPartIds(string name, IComponent[] parts)
    {
        var build = new Build(name);
        foreach (var part in parts)
            build.Add(part);
        // TODO (розробник, #63): макет сховища: назва вільна
        BuildLibrary library = null!;

        library.Save(build);

        var ids = parts.Select(p => p.Id).ToList();
    }

    /// <summary>TC-75: назва зайнята. Без overwrite помилка і нічого не записано, з overwrite перезапис.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Save_NameTaken_DependsOnOverwrite(bool overwrite, bool expectThrow)
    {
        var build = new Build("Freestyle 5");
        // TODO (розробник, #63): макет сховища: назва вже зайнята
        BuildLibrary library = null!;

        var ex = Record.Exception(() => library.Save(build, overwrite));

        Assert.Equal(expectThrow, ex is InvalidOperationException);
    }

    /// <summary>TC-76: збірка відкривається, деталі знову беруться з каталогу.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData("Freestyle 5", new[] { "fr-1", "mt-1", "mt-1" }, 3)]
    [InlineData("Лише рама", new[] { "fr-1" }, 1)]
    [InlineData("Порожня", new string[0], 0)]
    public void Open_SavedBuild_PartsTakenFromCatalog(string name, string[] partIds, int expectedParts)
    {
        // TODO (розробник, #63): макети: сховище повертає збірку, каталог знає fr-1 і mt-1
        BuildLibrary library = null!;

        var build = library.Open(name);

        Assert.Equal(name, build.Name);
        Assert.Equal(expectedParts, build.Parts.Count);
        Assert.Equal(partIds, build.Parts.Select(p => p.Id));
    }

    /// <summary>TC-77: збірки з такою назвою немає.</summary>
    [Fact(Skip = Stub.Reason)]
    public void Open_UnknownName_ThrowsKeyNotFound()
    {
        // TODO (розробник, #63): макет сховища: Load повертає null
        BuildLibrary library = null!;

        var ex = Assert.Throws<KeyNotFoundException>(() => library.Open("Немає такої"));

        Assert.Contains("Немає такої", ex.Message);
    }

    /// <summary>TC-78: назви збірок за абеткою без урахування регістру.</summary>
    [Theory(Skip = Stub.Reason)]
    [InlineData(new[] { "b", "A", "c" }, new[] { "A", "b", "c" })]
    [InlineData(new[] { "Racer", "freestyle", "Cine" }, new[] { "Cine", "freestyle", "Racer" })]
    [InlineData(new string[0], new string[0])]
    public void Names_FromStorage_SortedAlphabetically(string[] stored, string[] expected)
    {
        // TODO (розробник, #63): макет сховища
        BuildLibrary library = null!;

        Assert.Equal(expected, library.Names());
    }
}