using FpvConfigurator.Services.Storage;

namespace FpvConfigurator.Tests.Services;

/// <summary>JsonFileBuildStorage (ФВ-20): справжній файл у тимчасовій папці (TC-80…TC-83).</summary>
public sealed class JsonFileBuildStorageTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "fpv-tests-" + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_folder, "builds.json");

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }

    /// <summary>TC-80: збережену збірку бачить новий об'єкт сховища, тобто вона пережила перезапуск.</summary>
    [Theory]
    [InlineData("Порожня", new string[0])]
    [InlineData("5\" фрістайл", new[] { "fr-1", "mt-1", "mt-1", "mt-1", "mt-1" })]
    public void Save_ThenNewInstance_LoadsSameBuild(string name, string[] ids)
    {
        new JsonFileBuildStorage(FilePath).Save(new SavedBuild(name, ids));

        var loaded = new JsonFileBuildStorage(FilePath).Load(name);

        Assert.NotNull(loaded);
        Assert.Equal(name, loaded.Name);
        Assert.Equal(ids, loaded.PartIds);
    }

    /// <summary>TC-81: назви нечутливі до регістру, повторне збереження перезаписує збірку.</summary>
    [Fact]
    public void Save_SameNameOtherCase_Overwrites()
    {
        var storage = new JsonFileBuildStorage(FilePath);
        storage.Save(new SavedBuild("Freestyle", ["fr-1"]));

        storage.Save(new SavedBuild("FREESTYLE", ["fr-3"]));

        Assert.True(storage.Exists("freestyle"));
        Assert.Equal(["FREESTYLE"], storage.ListNames());
        Assert.Equal(["fr-3"], storage.Load("Freestyle")!.PartIds);
    }

    /// <summary>TC-82: Delete прибирає збірку і з файлу. Для неіснуючої назви повертає false.</summary>
    [Theory]
    [InlineData("Freestyle", true)]
    [InlineData("freestyle", true)]
    [InlineData("Немає такої", false)]
    public void Delete_ReturnsWhetherRemoved(string name, bool expected)
    {
        var storage = new JsonFileBuildStorage(FilePath);
        storage.Save(new SavedBuild("Freestyle", ["fr-1"]));

        Assert.Equal(expected, storage.Delete(name));
        Assert.Equal(!expected, new JsonFileBuildStorage(FilePath).Exists("Freestyle"));
    }

    /// <summary>TC-83: файлу ще немає, сховище порожнє. Порожній шлях: ArgumentException.</summary>
    [Fact]
    public void NewFile_EmptyStorage_AndEmptyPathRejected()
    {
        var storage = new JsonFileBuildStorage(FilePath);

        Assert.Empty(storage.ListNames());
        Assert.Null(storage.Load("будь-яка"));
        Assert.False(File.Exists(FilePath));
        Assert.Throws<ArgumentException>(() => new JsonFileBuildStorage(" "));
    }
}