using FpvConfigurator.Services.Storage;
using Moq;

namespace FpvConfigurator.Tests.Mocks.Wrappers;

/// <summary>
/// Обгортка над макетом IBuildStorage: сховище, у якому вже лежать передані збірки.
/// Exists і Load шукають назву без урахування регістру. Усе, що передали в Save, потрапляє в Written, у Delete потрапляє в Deleted.
/// </summary>
internal sealed class StorageMock
{
    public StorageMock(params SavedBuild[] saved)
    {
        SavedBuild? Find(string name) =>
            saved.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

        Mock.Setup(s => s.Exists(It.IsAny<string>())).Returns((string name) => Find(name) is not null);
        Mock.Setup(s => s.Load(It.IsAny<string>())).Returns((string name) => Find(name));
        Mock.Setup(s => s.ListNames()).Returns(saved.Select(b => b.Name).ToList());
        Mock.Setup(s => s.Save(It.IsAny<SavedBuild>())).Callback((SavedBuild b) => Written.Add(b));
        Mock.Setup(s => s.Delete(It.IsAny<string>()))
            .Callback((string name) => Deleted.Add(name))
            .Returns((string name) => Find(name) is not null);
    }

    public Mock<IBuildStorage> Mock { get; } = new();

    public IBuildStorage Object => Mock.Object;

    /// <summary>Збірки, передані в Save, у порядку викликів.</summary>
    public List<SavedBuild> Written { get; } = [];

    /// <summary>Назви, передані в Delete, у порядку викликів.</summary>
    public List<string> Deleted { get; } = [];
}