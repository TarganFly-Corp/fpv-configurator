using FpvConfigurator.Services.Storage;
using FpvConfigurator.Tests.Mocks.Wrappers;
using Moq;

namespace FpvConfigurator.Tests.Mocks;

/// <summary>BuildLibrary.Rename і Delete (ФВ-20) з макетом сховища (TC-84…TC-86).</summary>
public class BuildLibraryRenameTests
{
    private static (BuildLibrary Library, StorageMock Storage) Create()
    {
        var storage = new StorageMock(new SavedBuild("Freestyle", ["fr-1", "mt-1"]), new SavedBuild("Cinewhoop", ["fr-3"]));
        return (new BuildLibrary(storage.Object, new CatalogMock().Object), storage);
    }

    /// <summary>TC-84: вільна нова назва. Спершу записується збірка з новою назвою і тими самими деталями, потім видаляється стара.</summary>
    [Theory]
    [InlineData("Freestyle", "Основний 5\"", true)]
    [InlineData("freestyle", " Основний ", true)]
    [InlineData("Freestyle", "FREESTYLE", false)]
    public void Rename_FreeName_SavesNewAndDeletesOld(string oldName, string newName, bool deletesOld)
    {
        var (library, storage) = Create();

        library.Rename(oldName, newName);

        var written = Assert.Single(storage.Written);
        Assert.Equal(newName.Trim(), written.Name);
        Assert.Equal(["fr-1", "mt-1"], written.PartIds);
        Assert.Equal(deletesOld ? ["Freestyle"] : [], storage.Deleted);
    }

    /// <summary>TC-85: нова назва зайнята, стара не існує або нова порожня. Сховище не змінюється.</summary>
    [Theory]
    [InlineData("Freestyle", "cinewhoop", typeof(InvalidOperationException))]
    [InlineData("Немає", "Нова", typeof(KeyNotFoundException))]
    [InlineData("Freestyle", "  ", typeof(ArgumentException))]
    public void Rename_Invalid_ThrowsAndChangesNothing(string oldName, string newName, Type exception)
    {
        var (library, storage) = Create();

        var ex = Record.Exception(() => library.Rename(oldName, newName));

        Assert.IsType(exception, ex);
        storage.Mock.Verify(s => s.Save(It.IsAny<SavedBuild>()), Times.Never);
        storage.Mock.Verify(s => s.Delete(It.IsAny<string>()), Times.Never);
    }

    /// <summary>TC-86: Delete передає назву в сховище. Якщо сховище не знайшло збірку, KeyNotFoundException.</summary>
    [Theory]
    [InlineData("Cinewhoop", false)]
    [InlineData("Немає", true)]
    public void Delete_PassesNameToStorage(string name, bool expectThrow)
    {
        var (library, storage) = Create();

        var ex = Record.Exception(() => library.Delete(name));

        Assert.Equal(expectThrow, ex is KeyNotFoundException);
        storage.Mock.Verify(s => s.Delete(name), Times.Once);
    }
}